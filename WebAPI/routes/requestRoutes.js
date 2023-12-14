const express = require('express');
const router = express.Router();
const {addRequest, getRequestAnalyticsByServiceId} = require('../Controllers/requestsController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addRequest);
router.route('/get-analytics/service_id').get(verifyToken, getRequestAnalyticsByServiceId);

module.exports = router;