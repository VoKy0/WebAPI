const config = require('config')
const stripe = require('stripe')(config.get('STRIPE.SECRET_STRIPE_KEY'))
const PlanModel = require("../models/Plan");
const ServiceModel = require("../models/Service");

const createCheckoutSession = async (req, res) => {
    try {
        const db = req.app.get('db');
        const planInfo = await PlanModel.findById(db, req.body.plan_id);
        console.log(planInfo);
        const serviceInfo = await ServiceModel.findById(db, planInfo.service_id);
        console.log(serviceInfo);
        const session = await stripe.checkout.sessions.create({
            payment_method_types: ['card'],
            line_items: [{
                price_data: {
                    currency: 'usd',
                    product_data: {
                        name: serviceInfo.name
                    },
                    unit_amount_decimal: planInfo.price * 100 //serviceOrder.price
                },
                quantity: 1
            }],
            mode: 'payment',
            success_url: `http://localhost:3000/success.html`, // trang sau khi thanh toán thành công
            cancel_url: `http://localhost:3000/cancel.html`, // trang sau khi tạm ngưng thanh toán
        })
        console.log(session);
        res.json({"success": false, "message": "Payment success!", "url": session.url, "session": session});

    } catch (err) {
        res.status(500).json({"success": false, "message": err.message})
    }
}


module.exports = {createCheckoutSession}


// fake data for fetching
// {
//     "orders": [
//     {
//         "id": 1,
//         "quantity": 1
//     },
//     {
//         "id": 3,
//         "quantity": 1
//     }
// ]
// }