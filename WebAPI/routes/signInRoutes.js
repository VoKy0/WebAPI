const express = require('express');
const router = express.Router();
const {signIn, OAuthLogin} = require('../controllers/authController');

router.route('/').post(signIn);
router.route('/oauth').post(OAuthLogin);

module.exports = router;