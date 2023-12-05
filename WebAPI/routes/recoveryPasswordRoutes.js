const express = require('express');
const router = express.Router();
const {recoverPassword} = require('../controllers/authController')
const {confirmEmail} = require('../controllers/authController')
const {verifyToken} = require('../middleware/auth');
router.route('/send-verify-email').post(confirmEmail)
router.route('/reset-password').post(verifyToken, recoverPassword)

module.exports = router;