const express = require('express');
const router = express.Router();
const {updatePlanRateLimit, getPlansRateLimitByPlanId} = require('../controllers/plansController')
const {verifyToken} = require('../middleware/auth');

router.route('/update').post(verifyToken, updatePlanRateLimit);
router.route('/get/plan_id').get(verifyToken, getPlansRateLimitByPlanId);
module.exports = router;