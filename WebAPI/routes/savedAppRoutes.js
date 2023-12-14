const express = require('express');
const router = express.Router();
const {addSavedApp, deleteSavedApp, getSavedAppsByUserId, getSavedAppByDetails} = require('../Controllers/savedAppsController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addSavedApp);
router.route('/delete').post(verifyToken, deleteSavedApp);
router.route('/get/user_id').get(verifyToken, getSavedAppsByUserId);
router.route('/get/user_id/app_id').get(verifyToken, getSavedAppByDetails);

module.exports = router;