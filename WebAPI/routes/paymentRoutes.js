const express = require('express');
const router = express.Router();
const {createCheckoutSession} = require('../Controllers/paymentController')
const {verifyToken} = require('../middleware/auth');

router.route('/create-checkout-session').post(createCheckoutSession)
// success, fail
module.exports = router;