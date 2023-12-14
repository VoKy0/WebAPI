const express = require('express');
const router = express.Router();
const {getAccessTokenBySubscriptionId} = require('../Controllers/subscriptionAuthenticationController')
const {verifyToken} = require('../middleware/auth');

router.route('/get/subscription_id').get(verifyToken, getAccessTokenBySubscriptionId);
module.exports = router;