const express = require('express');
const router = express.Router();
const {generateCode} = require('../controllers/generateCodeController')
const {verifyToken} = require('../middleware/auth');

router.route('/get').get(verifyToken, generateCode);

module.exports = router;