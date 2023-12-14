const express = require('express');
const router = express.Router();
const {uploadImage, fetchImage, deleteImage} = require('../Controllers/cloudinaryController')
const {verifyToken} = require('../middleware/auth');

router.route('/get').get(verifyToken, fetchImage);
router.route('/create').post(verifyToken, uploadImage);
router.route('/delete').post(verifyToken, deleteImage);
module.exports = router;