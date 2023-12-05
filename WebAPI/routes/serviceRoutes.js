const express = require('express');
const router = express.Router();
const {updateService, getServices,getServiceById, getServicesByServiceCategoryId, getServicesByServiceCategoryName, getServicesByVendorId, addService} = require('../controllers/servicesController')
const {verifyToken} = require('../middleware/auth');

router.route('/get').get(verifyToken, getServices);
router.route('/create').post(verifyToken, addService);
router.route('/get/service_category_name').get(verifyToken, getServicesByServiceCategoryName);
router.route('/get/id').get(verifyToken, getServiceById);
router.route('/get/service_category_id').get(verifyToken, getServicesByServiceCategoryId);
router.route('/get/vendor_id').get(verifyToken, getServicesByVendorId);
router.route('/update').post(verifyToken, updateService)

module.exports = router;