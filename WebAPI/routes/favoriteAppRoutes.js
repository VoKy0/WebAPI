const express = require('express');
const router = express.Router();
const {addFavoriteApp, deleteFavoriteApp, getFavoriteAppByDetails, getFavoriteCountByAppId} = require('../Controllers/favoriteAppsController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addFavoriteApp);
router.route('/delete').post(verifyToken, deleteFavoriteApp);
router.route('/get/user_id/app_id').get(verifyToken, getFavoriteAppByDetails);
router.route('/get-count/app_id').get(verifyToken, getFavoriteCountByAppId);

module.exports = router;