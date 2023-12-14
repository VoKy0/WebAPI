const express = require('express');
const router = express.Router();
const {addSubscription, cancelSubscriptionByDetails, updateSubscription, getSubscriptionsByUserId, getSubscriptionById, getSubscriptionByDetails} = require('../Controllers/subscriptionsController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addSubscription);
router.route('/update').post(verifyToken, updateSubscription);
router.route('/get/id').get(verifyToken, getSubscriptionById);
router.route('/get/user_id/service_id').get(verifyToken, getSubscriptionByDetails);
router.route('/get/user_id').get(verifyToken, getSubscriptionsByUserId);
router.route('/cancel/user_id/service_id').post(verifyToken, cancelSubscriptionByDetails);

module.exports = router;