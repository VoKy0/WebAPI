const express = require('express');
const router = express.Router();
const {updateEndpoint, getHeadersByEndpointId, getQueriesByEndpointId, getBodiesByEndpointId, addEndpoint, deleteEndpoint, getEndpointsByServiceId} = require('../controllers/endpointsController')
const {verifyToken} = require('../middleware/auth');

router.route('/get/service_id').get(verifyToken, getEndpointsByServiceId);
router.route('/create').post(verifyToken, addEndpoint);
router.route('/delete').post(verifyToken, deleteEndpoint);
router.route('/update').post(verifyToken, updateEndpoint);
router.route('/headers/get/endpoint_id').get(verifyToken, getHeadersByEndpointId);
router.route('/queries/get/endpoint_id').get(verifyToken, getQueriesByEndpointId);
router.route('/bodies/get/endpoint_id').get(verifyToken, getBodiesByEndpointId);

module.exports = router;