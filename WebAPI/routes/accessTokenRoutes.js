const express = require('express');
const router = express.Router();
const {generateAccessToken} = require('../controllers/authController')
const {verifyToken} = require('../middleware/auth');
router.route('/').post(verifyToken, generateAccessToken);
module.exports = router;