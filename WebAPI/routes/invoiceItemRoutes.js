const express = require('express');
const router = express.Router();
const {getInvoiceItemsByInvoiceId} = require('../Controllers/invoiceItemsController')
const {verifyToken} = require('../middleware/auth');

router.route('/get/invoice_id').get(verifyToken, getInvoiceItemsByInvoiceId);

module.exports = router;