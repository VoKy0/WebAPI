const express = require('express');
const router = express.Router();
const {addInvoice, getInvoiceById} = require('../Controllers/invoicesController')
const {verifyToken} = require('../middleware/auth');

router.route('/create').post(verifyToken, addInvoice);
router.route('/get/id').get(verifyToken, getInvoiceById);

module.exports = router;