const express = require('express');
const router = express.Router();
const {confirmEmailOnSignUp, verifyAccount} = require("../controllers/authController");
const {verifyToken} = require("../middleware/auth")
router.route('/send-verify-email').post(confirmEmailOnSignUp);
router.route('/verify').post(verifyToken, verifyAccount);

module.exports = router;