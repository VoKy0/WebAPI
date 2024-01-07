const express = require('express');
const router = express.Router();
const {getTopRequestsAPI } = require('../controllers/globalAnalyticsController');
const {verifyToken} = require('../middleware/auth');

router.route('/get/top-requests-api/limit').get(verifyToken, getTopRequestsAPI);


module.exports = router;