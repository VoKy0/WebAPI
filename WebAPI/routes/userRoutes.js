const express = require('express');
const router = express.Router();
const {updateUser, getUserById, getUserByUsername, getUsers} = require('../controllers/usersController')
const {verifyToken} = require('../middleware/auth');
router.route('/get').get(verifyToken, getUsers);
router.route('/get/id').get(verifyToken, getUserById);
router.route('/get/username').get(verifyToken, getUserByUsername);
router.route('/update').post(verifyToken, updateUser);
module.exports = router;