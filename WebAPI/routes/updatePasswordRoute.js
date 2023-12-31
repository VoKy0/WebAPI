const express = require('express');
const router = express.Router();
const {updatePassword: updatePasswordRoute} = require('../Controllers/authController');
const {verifyToken} = require('../middleware/auth');
router.route('/').post(verifyToken, updatePasswordRoute);

module.exports = router;