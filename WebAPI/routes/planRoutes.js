const express = require('express');
const router = express.Router();
const {getPlansByServiceId, updatePlan, addPlan, activatePlan} = require('../Controllers/plansController')
const {verifyToken} = require('../middleware/auth');

router.route('/get/service_id').get(verifyToken, getPlansByServiceId);
router.route('/create').post(verifyToken, addPlan);
router.route('/activate').post(verifyToken, activatePlan);
router.route('/update').post(verifyToken, updatePlan)

module.exports = router;