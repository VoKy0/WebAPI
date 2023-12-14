const express = require('express');
const router = express.Router();
const {reGenerateAccessToken, signOut} = require('../Controllers/authController')
const {verifyToken} = require('../middleware/auth');
router.route('/').post(reGenerateAccessToken);
router.route('/').delete(verifyToken, signOut);
module.exports = router;