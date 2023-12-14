const express = require('express');
const router = express.Router();
const {getCommentsByDiscussionId, addComment} = require('../Controllers/commentsController')
const {verifyToken} = require('../middleware/auth');
router.route('/create').post(verifyToken, addComment);
router.route('/get/discussion_id').get(verifyToken, getCommentsByDiscussionId);

module.exports = router;