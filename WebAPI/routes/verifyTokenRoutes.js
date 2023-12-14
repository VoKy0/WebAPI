const express = require('express');
const router = express.Router();
const {manualVerifyToken} = require("../Controllers/authController");
router.route('/verify').post(manualVerifyToken);

module.exports = router;