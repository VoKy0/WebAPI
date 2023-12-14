const config = require('config')
const stripe = require('stripe')(config.get('STRIPE.SECRET_STRIPE_KEY'));
const PlanModel = require("../Models/Plan");
const SubscriptionModel = require("../Models/Subscription");
const ServiceModel = require("../Models/Service");
const ServiceAuthModel = require("../Models/ServiceAuth");
const UserModel = require('../Models/User');
const NotificationModel = require("../Models/Notification");
const {generateAccessToken} = require('../services/JWTServices')
const {sendWSMessage} = require("../services/WebSocketServices");
const createAutoPaymentIntent = async (req, res) =>{
    try {
        const amount = req.body.request;
        const description = req.body.description;
        const userId = req.body.user_id;
        const serviceId = req.body.service_id;
        const db = req.app.get('db');
        const userInfo = await UserModel.findById(db, userId);
        const subscription = await  SubscriptionModel.getActiveByDetails(db, userId, serviceId)
        const session = await stripe.checkout.sessions.retrieve(subscription[0].session_id);
        const paymentIntent = await stripe.paymentIntents.retrieve(session.payment_intent);
        const paymentMethod = await stripe.paymentMethods.retrieve(paymentIntent.payment_method);
        let customer = await stripe.customers.list({ email: session.customer_details.email });
        if (customer.data.length === 0) {
            customer = await stripe.customers.create({
                email: session.customer_details.email,
                name: userInfo.user_name,
                payment_method: paymentMethod.id,
            });
            console.log(customer)
        }else{
            customer = customer.data[0]
        }
        // }else if(customer.data[0].invoice_settings.default_payment_method === null){
        //     // const method = await stripe.paymentMethods.detach(
        //     //     paymentMethod.id,
        //     //
        //     // );
        //     const method = await stripe.paymentMethods.attach(
        //         paymentMethod.id,
        //         {
        //             customer: customer.data[0].id,
        //         }
        //     );
        //     customer = await stripe.customers.update(customer.data[0].id, {
        //         invoice_settings: {
        //             default_payment_method: method.id,
        //         },
        //     })
        // }



        const dueDate = new Date();
        dueDate.setDate(dueDate.getDate() + 14);

        const invoice = await stripe.invoices.create({
            customer: customer.id,
            collection_method: 'send_invoice',
            default_payment_method: customer.invoice_settings.default_payment_method,
            due_date: Math.floor(dueDate.getTime() / 1000), // Convert to seconds
        });

        await stripe.invoiceItems.create({
                customer: customer.id,
                amount: amount,
                currency: 'usd',
                invoice: invoice.id,
                description: description
        });


        stripe.invoices.pay(invoice.id).then(payInvoice => {
            res.status(200).json({success: true, message: 'Invoice created and receipt sent', data: [payInvoice]});
        }).catch(error => {
            console.log(error);
            res.status(200).json({success: false, message: error.message, data: []});
        })
        // res.status(200).json({success: true, message: 'Invoice created and receipt sent', data: [customer]});

    } catch (err) {
        console.log(err)
        res.status(500).json({"success": false, "message": err.message, "data": []})
    }
}

const paymentSuccess = async (req, res) => {
    console.log('here in paymentsuccess');
    const db = req.app.get('db');
    const mailer = req.app.get('mailer');
    const sjs = req.app.get('sjs');
    let newNoti;
    const serviceAuthen = await ServiceAuthModel.findByServiceId(db, req.query.service_id);
    const sessionId = req.query.session_id;
    let subscriptionId;
    db.schema.hasTable("subscriptions").then(async function(exists) {
        if (exists) {
            try {
                let activeSubscription = await SubscriptionModel.getActiveByDetails(db, req.query.user_id, req.query.service_id);
                if (activeSubscription.length) {
                    activeSubscription = activeSubscription[0];
                    sjs.killJobScheduler(activeSubscription.id);
                } else {
                    activeSubscription = null;
                }
                const userInfo = await UserModel.findById(db, req.query.user_id);
                const planInfo = await PlanModel.findById(db, req.query.plan_id);
                const serviceInfo = await ServiceModel.findById(db, planInfo.service_id);
                const planLimitDetails = await PlanModel.findPlanLimitDetailById(db, req.query.plan_id);
                const session = await stripe.checkout.sessions.retrieve(sessionId);
                const endDate = new Date();
                const startDate = new Date();
                endDate.setDate(endDate.getDate() + (planLimitDetails.ql_interval == 'monthly'? 30 : 1) * req.query.quantity);
                if (session.amount_total != 0) {
                    const paymentIntent = await stripe.paymentIntents.retrieve(session.payment_intent);

                    db.transaction(function(trx) {
                        db('invoices').insert({
                            date: new Date(),
                            currency_code: 'usd',
                            tax_rate: '0%',
                            tax_amount: 0.00,
                            gross_amount: paymentIntent.amount / 100,
                            net_amount: paymentIntent.amount / 100
                        }).transacting(trx).returning('*').then(row => {
                            const cancelOldSubscriptionPromise = activeSubscription? db('subscriptions').where({id: activeSubscription.id}).update({status: 'cancelled'}).returning('*').transacting(trx) : null;
                            const addSubscriptionPromise = db('subscriptions').insert({invoice_id: row[0].id, user_id: req.query.user_id, service_id: req.query.service_id, plan_id: req.query.plan_id, start_date: startDate, end_date: endDate, payment_status: session.payment_status, session_id: sessionId, quantity: req.query.quantity, description: `API: ${serviceInfo.name}; ${planInfo.name.toUpperCase()} plan`, amount: session.amount_total / 100 }).returning('*').transacting(trx);
                            const addInvoiceItemPromise = db('invoice_items').insert({
                                quantity: req.query.quantity,
                                invoice_id: row[0].id,
                                price: planInfo.price,
                                description: `Subscribe ${serviceInfo.name} API with ${planInfo.name} plan.`,
                                total_amount: req.query.quantity *  planInfo.price
                            }).transacting(trx);
                            return Promise.all([cancelOldSubscriptionPromise, addSubscriptionPromise, addInvoiceItemPromise])
                        }).then(async rows => {
                            subscriptionId = rows[1][0].id;
                            sjs.createJobScheduler(subscriptionId);
                            const {accessToken} = await generateAccessToken(subscriptionId, rows[1][0].start_date, serviceAuthen.private_key, planInfo.price == 0? null : `${rows[1][0].quantity * (planLimitDetails.ql_interval == 'monthly'? 30 : 1)}d`);
                            const addSubAuthenPromise = db('subscription_authentication').insert({
                                subscription_id: subscriptionId,
                                access_token: accessToken
                            }).transacting(trx).returning('*');
                            const addNotificationPromise = db('notifications').insert({
                                title: 'New Subscription',
                                description: `You have successfully subscribed to ${serviceInfo.name} ${planInfo.name.toUpperCase()} plan`,
                                user_id: req.query.user_id,
                                href: `${config.get('PROTOCOL')}://${config.get("CLIENT_HOST")}/user/subscriptions/${subscriptionId}`
                            }).transacting(trx).returning('*');
                            Promise.all([addSubAuthenPromise, addNotificationPromise]).then(results => {
                                newNoti = results[1][0];
                                trx.commit();
                            });
                            // trx.commit();
                        }).catch(trx.rollback);
                    }).then(function(result) {
                        sendWSMessage(JSON.stringify({
                            action: 'notice',
                            message: newNoti
                        }));
                        mailer.sendMailAsync(userInfo.email, "PAYMENT SUCCESS", "payment success");
                        res.redirect(`${config.get('PROTOCOL')}://${config.get("CLIENT_HOST")}/user/subscriptions/${subscriptionId}`)
                    }).catch(error => {
                        console.log(error);
                    })

                } else {
                    db.transaction(function(trx) {
                        const cancelOldSubscriptionPromise = activeSubscription? db('subscriptions').where({
                            id: activeSubscription.id
                        }).update({status: 'cancelled'}).returning('*').transacting(trx) : null;
                        const addSubscriptionPromise = db('subscriptions').insert({
                            user_id: req.query.user_id,
                            service_id: req.query.service_id,
                            plan_id: req.query.plan_id,
                            start_date: new Date(),
                            end_date: endDate,
                            payment_status: session.payment_status,
                            status: 'active',
                            session_id: sessionId,
                            quantity: req.query.quantity,
                            description: `API: ${serviceInfo.name}; ${planInfo.name.toUpperCase()} plan`,
                            amount: session.amount_total / 100
                        }).returning('*').transacting(trx)
                        Promise.all([cancelOldSubscriptionPromise, addSubscriptionPromise]).then(async rows => {
                            subscriptionId = rows[1][0].id;
                            sjs.createJobScheduler(subscriptionId);
                            const {accessToken} = await generateAccessToken(subscriptionId, rows[1][0].start_date, serviceAuthen.private_key, planInfo.price == 0? null : `${rows[1][0].quantity * (planLimitDetails.ql_interval == 'monthly'? 30 : 1)}d`);
                            const addSubAuthenPromise = db('subscription_authentication').insert({
                                subscription_id: subscriptionId,
                                access_token: accessToken
                            }).transacting(trx).returning('*');
                            const addNotificationPromise = db('notifications').insert({
                                title: 'New Subscription',
                                description: `You have successfully subscribed to ${serviceInfo.name} ${planInfo.name.toUpperCase()} plan`,
                                user_id: req.query.user_id,
                                href: `${config.get('PROTOCOL')}://${config.get("CLIENT_HOST")}/user/subscriptions/${subscriptionId}`
                            }).transacting(trx).returning('*');
                            Promise.all([addSubAuthenPromise, addNotificationPromise]).then(results => {
                                newNoti = results[1][0];
                                trx.commit();
                            });

                        }).catch(trx.rollback);
                    }).then(function(result) {
                        mailer.sendMailAsync(userInfo.email, "PAYMENT SUCCESS", "payment success");
                        sendWSMessage(JSON.stringify({
                            action: 'notice',
                            message: newNoti
                        }));
                        res.redirect(`${config.get('PROTOCOL')}://${config.get("CLIENT_HOST")}/user/subscriptions/${subscriptionId}`)
                    }).catch(error => {
                        console.log(error);
                    })
                }


            } catch(err) {
                console.log('error here: ', err.message);
            }

        }
    })
}

const paymentCancel = async (req, res) => {

}

const getPaymentIntentBySessionId = async (req, res)=>{
    try {
        const sessionId = req.query.session_id;
        const session = await stripe.checkout.sessions.retrieve(sessionId);
        const intent = await stripe.paymentIntents.retrieve(session.payment_intent);
        res.json({ 'success': true, "message": "Success", 'data': [intent] });

    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, data: [{amount: 0}]})
    }
}


const createCheckoutSession = async (req, res) => {
    try {
        console.log('quantity:', req.body.quantity)
        const db = req.app.get('db');
        let token =
            req.body.token || req.query.token || req.headers["x-access-token"] || req.headers.authorization;
        if (token.startsWith('Bearer ')) {
            token = token.substring(7);
        }
        const planInfo = await PlanModel.findById(db, req.body.plan_id);
        const serviceInfo = await ServiceModel.findById(db, planInfo.service_id);
        const session = await stripe.checkout.sessions.create({
            payment_method_types: ['card'],
            line_items: [{
                price_data: {
                    currency: 'usd',
                    product_data: {
                        name: serviceInfo.name
                    },
                    unit_amount_decimal: planInfo.price * 100
                },
                quantity: req.body.quantity
            }],

            mode: 'payment',
            success_url: `${config.get("PROTOCOL")}://${config.get("CLIENT_HOST")}/api/payment/success?user_id=${req.body.user_id}&service_id=${req.body.service_id}&plan_id=${req.body.plan_id}&session_id={CHECKOUT_SESSION_ID}&quantity=${req.body.quantity}&token=${token}`, // trang sau khi thanh toán thành công
            cancel_url: req.body.current_url, // trang sau khi tạm ngưng thanh toán
        })

        console.log('session', session)
        res.json({"success": true, "message": "Payment success!", "data": [session]});

    } catch (err) {
        console.log(err)
        res.status(500).json({"success": false, "message": err.message, "data": [null]})
    }
}
const getCheckoutSession = async (req, res)=>{
    try {
        const sessionId = req.query.session_id;
        const session = await stripe.checkout.sessions.retrieve(sessionId);
        // console.log('Payment Details:',paymentStatus);

        res.json({ 'success': true, "message": "Success", 'data': [session] });

    } catch (err) {
        res.status(500).json({"success": false, "message": err.message})
    }
}
const getPaymentMethod = async (req, res)=>{
    try {
            const method = await stripe.paymentMethods.retrieve("pm_1OMvD7AJcoaTPifSnPOt6HHu");
            res.json({ 'success': true, "message": "Success", 'data': [method] });

    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}
const getPaymentMethodBySessionId = async (req, res)=>{
    try {
        const sessionId = req.query.session_id;
        const session = await stripe.checkout.sessions.retrieve(sessionId);
        if(session.payment_status === 'paid'){
            const intent = await stripe.paymentIntents.retrieve(session.payment_intent);
            const method = await stripe.paymentMethods.retrieve(intent.payment_method);
            res.json({ 'success': true, "message": "Success", 'data': [method] });
        } else {
            res.json({ 'success': false, "message": "Failure Session is unpaid", 'data': [] });
        }
    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}
const getPaymentMethodByCustomerId = async (req, res)=>{
    try {
        const paymentMethods = await stripe.customers.listPaymentMethods(
            req.query.customer_id,
            {
                limit: 3,
            }
        );
        res.status(200).json({"success": 'success', "data": [paymentMethods]})
    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}
const getCustomer = async (req, res)=>{
    try {
        const customer = await stripe.customers.retrieve(req.query.customer_id)

        res.status(200).json({"success": 'success', "data": [customer]})
    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}
const createInvoice = async (req, res) => {
    try {
        const customerId = req.body.customer_id
        // const paymentIntentId = req.body.payment_intent_id
        const items = req.body.items

        const dueDate = new Date();
        dueDate.setDate(dueDate.getDate() + 14);

        // const paymentIntent = await stripe.paymentIntents.retrieve(paymentIntentId);
        // if (paymentIntent.status === 'succeeded') {
            const invoice = await stripe.invoices.create({
                customer: customerId,
                collection_method: 'send_invoice',
                due_date: Math.floor(dueDate.getTime() / 1000), // Convert to seconds
            });
            await Promise.all(items.map(async (item) => {
                return stripe.invoiceItems.create({
                    customer: customerId,
                    amount: item.amount,
                    currency: 'usd',
                    invoice: invoice.id,
                    description: item.name
                });
            }));


            const sentInvoice = await stripe.invoices.pay(invoice.id);

            // Send a success response
            res.status(200).json({success: true, message: 'Invoice created and receipt sent', invoice: sentInvoice});
        // }else {
        //     res.status(200).json({success: false, message: 'PaymentIntent is not successful'});
        // }
    } catch (err) {
        console.log(err)
        res.status(500).json({ success: false, message: 'Invoice created and payment fail' });
    }

}

module.exports =
    {
        createCheckoutSession,
        getCheckoutSession,
        // createPaymentIntent,
        createAutoPaymentIntent,
        getPaymentIntentBySessionId,
        getPaymentMethodBySessionId,
        paymentSuccess,
        paymentCancel,
        createInvoice,
        getPaymentMethodByCustomerId,
        getPaymentMethod,
        getCustomer
    }


