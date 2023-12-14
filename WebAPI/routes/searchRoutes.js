const express = require('express');
const router = express.Router();
const {searchApps, searchServices} = require('../Controllers/searchController');
const {verifyToken} = require('../middleware/auth');

router.route('/apps').get(verifyToken, searchApps);
router.route('/services').get(verifyToken, searchServices);

module.exports = router;