const express = require('express');
const router = express.Router();
const {addSavedService, deleteSavedService, getSavedServiceByUserId, getSavedServiceByDetails} = require('../Controllers/savedServicesController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addSavedService);
router.route('/delete').post(verifyToken, deleteSavedService);
router.route('/get/user_id').get(verifyToken, getSavedServiceByUserId);
router.route('/get/user_id/service_id').get(verifyToken, getSavedServiceByDetails);

module.exports = router;