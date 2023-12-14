const express = require('express');
const router = express.Router();
const verifyToken = require('./verifyTokenRoutes')
const { addMessages, getMessages} = require('../Controllers/liveChatController')

router.route('/get').get(verifyToken, getMessages);
router.route('/add').post(verifyToken, addMessages);

module.exports = router;