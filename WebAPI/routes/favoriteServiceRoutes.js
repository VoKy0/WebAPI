const express = require('express');
const router = express.Router();
const {addFavoriteService, deleteFavoriteService, getFavoriteServiceByDetails, getFavoriteCountByServiceId} = require('../Controllers/favoriteServicesController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addFavoriteService);
router.route('/delete').post(verifyToken, deleteFavoriteService);
router.route('/get/user_id/service_id').get(verifyToken, getFavoriteServiceByDetails);
router.route('/get-count/service_id').get(verifyToken, getFavoriteCountByServiceId);

module.exports = router;