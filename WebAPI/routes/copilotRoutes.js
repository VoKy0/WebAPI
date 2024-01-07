const express = require('express');
const router = express.Router();
const {getSimilarAPIs} = require('../controllers/copilotController')
const {verifyToken} = require('../middleware/auth');

router.route('/get-similar-apis').post(verifyToken, getSimilarAPIs);

module.exports = router;