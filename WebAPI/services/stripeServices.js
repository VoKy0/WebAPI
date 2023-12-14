const config = require('config')
const stripe = require('stripe')(config.get('STRIPE.SECRET_STRIPE_KEY'));
const PlanModel = require("../Models/Plan");
const SubscriptionModel = require("../Models/SubscriptionsModel");
const ServiceModel = require("../Models/Service");
const ServiceAuthModel = require("../Models/ServiceAuth");
const UserModel = require('../Models/User');


const createAutoPaymentIntent = async (amount, description, userId, db) =>{
    try {
        const userInfo = await UserModel.findById(db, userId);

        let customer = await stripe.customers.list({ email: userInfo.email });
        if (customer.data.length === 0) {
            console.log('here create customer')
            customer = await stripe.customers.create({
                email: userInfo.email,
                name: userInfo.user_name,
            });
        } else {
            customer = customer.data[0];
        }


        const dueDate = new Date();
        dueDate.setDate(dueDate.getDate() + 14);

        const invoice = await stripe.invoices.create({
            customer: customer.id,
            collection_method: 'send_invoice',
            due_date: Math.floor(dueDate.getTime() / 1000), // Convert to seconds
        });

        await stripe.invoiceItems.create({
            customer: customer.id,
            amount: amount,
            currency: 'usd',
            invoice: invoice.id,
            description: description
        });
        console.log('customer', customer)

        const payInvoice = await stripe.invoices.pay(invoice.id);
        return payInvoice
    } catch (err) {
        console.log(err)
        return null;
    }
}

module.exports = {
    createAutoPaymentIntent
}