const express = require('express');
const router = express.Router();
const {updatePlanQuotaLimit, getPlansQuotaLimitByPlanId} = require('../controllers/plansController')
const {verifyToken} = require('../middleware/auth');

router.route('/update').post(verifyToken, updatePlanQuotaLimit);
router.route('/get/plan_id').get(verifyToken, getPlansQuotaLimitByPlanId);
module.exports = router;