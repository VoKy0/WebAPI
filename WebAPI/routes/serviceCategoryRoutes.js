const express = require('express');
const router = express.Router();
const {getServiceCategories, getServiceCategoryById} = require('../Controllers/serviceCategoriesController')
const {verifyToken} = require('../middleware/auth');
router.route('/get').get(verifyToken, getServiceCategories);
router.route('/get/id').get(verifyToken, getServiceCategoryById);


module.exports = router;