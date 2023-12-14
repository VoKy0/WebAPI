const express = require('express');
const router = express.Router();
const {getApps, addApp, getAppsByVendorId} = require('../Controllers/appsController')
const {verifyToken} = require('../middleware/auth');

router.route('/get').get(verifyToken, getApps);
router.route('/create').post(verifyToken, addApp);
router.route('/get/vendor_id').get(verifyToken, getAppsByVendorId);

module.exports = router;