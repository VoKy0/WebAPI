const express = require('express');
const router = express.Router();
const {testEndpoint} = require('../Controllers/testEndpointController')
const {verifyToken} = require('../middleware/auth');

router.route('/').post(verifyToken, testEndpoint);

module.exports = router;