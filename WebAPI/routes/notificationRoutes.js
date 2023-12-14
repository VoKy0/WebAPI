const express = require('express');
const router = express.Router();
const {addNotification, getNotificationsByUserId, deleteNotification, seenNotification} = require('../Controllers/notificationsController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addNotification);
router.route('/delete').post(verifyToken, deleteNotification);
router.route('/get/user_id').get(verifyToken, getNotificationsByUserId);
router.route('/update-seen/id/seen').post(verifyToken, seenNotification);

module.exports = router;