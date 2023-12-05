const express = require('express');
const router = express.Router();
const {verifyToken} = require('../middleware/auth');
const {sendSMSOtp, verifiedOtp} = require('../controllers/authController')

router.route('/send-sms').post(sendSMSOtp);
router.route('/verified').post(verifiedOtp);

module.exports = router;