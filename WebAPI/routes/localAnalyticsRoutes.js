const express = require('express');
const router = express.Router();
const { getTopUsersOnRequestCount, getTotalMonthRevenueByServiceId, getRequestCountWithinTimeRangeByServiceId, getAvgRequestLatencyWithinTimeRangeByServiceId,getCustomerCountWithinTimeRangeByServiceId, getAvgRevenueWithinTimeRangeByServiceId} = require('../Controllers/localAnalyticsController');
const {verifyToken} = require('../middleware/auth');

router.route('/get/customer-count/service_id/start_date/end_date').get(verifyToken, getCustomerCountWithinTimeRangeByServiceId);
router.route('/get/request-count/service_id/start_date/end_date').get(verifyToken, getRequestCountWithinTimeRangeByServiceId);
router.route('/get/avg-revenue/service_id/start_date/end_date').get(verifyToken, getAvgRevenueWithinTimeRangeByServiceId);
router.route('/get/top-users/user_id/service_id/limit').get(verifyToken, getTopUsersOnRequestCount);
router.route('/get/avg-request-latency/service_id/start_date/end_date').get(verifyToken, getAvgRequestLatencyWithinTimeRangeByServiceId );
router.route('/get/total-revenue/service_id/month/year').get(verifyToken, getTotalMonthRevenueByServiceId );

module.exports = router;