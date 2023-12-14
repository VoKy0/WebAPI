const express = require('express');
const router = express.Router();
const {recoverPassword} = require('../Controllers/authController')
const {confirmEmail} = require('../Controllers/authController')
const {verifyToken} = require('../middleware/auth');
router.route('/send-verify-email').post(confirmEmail)
router.route('/reset-password').post(verifyToken, recoverPassword)

module.exports = router;