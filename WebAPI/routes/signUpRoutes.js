const express = require('express');
const router = express.Router();
const {signUp, setUsernameOnSignUp} = require('../Controllers/authController');
const {verifyToken} = require('../middleware/auth');
router.route('/').post(signUp);
router.route('/set-username').post(verifyToken, setUsernameOnSignUp)
module.exports = router;