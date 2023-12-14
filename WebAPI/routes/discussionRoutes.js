const express = require('express');
const router = express.Router();
const {getDiscussionById, getDiscussionsByServiceId,addDiscussion} = require('../Controllers/discussionsController')
const {verifyToken} = require('../middleware/auth');
router.route('/create').post(verifyToken, addDiscussion);
router.route('/get/service_id').get(verifyToken, getDiscussionsByServiceId);
router.route('/get/id').get(verifyToken, getDiscussionById);

module.exports = router;