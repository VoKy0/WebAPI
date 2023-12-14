const express = require('express');
const session = require('express-session');
const { RateLimiterMemory } = require('rate-limiter-flexible');
const cors = require("cors");
const {verifyRequest, verifyQuotaLimit, verifyRateLimit} = require("../middleware/serviceAuth");
const {redirectGetRequest, redirectPostRequest} = require("../Controllers/proxyControllers");
let port = 2205;
function createProxyServer(db, serviceMetadata) {
    console.log(serviceMetadata)
    const app = express();
    app.set('db', db);
    app.set('base_url', serviceMetadata.base_url);
    app.set('secret', serviceMetadata.private_key);
    app.use(cors());
    app.use(session({
        resave: false,
        saveUninitialized: true,
        secret: "abc"
        // secret: serviceMetadata.private_key
    }));
    app.use(express.json());
    app.use(express.urlencoded({extended: false}));
    // app.all('*', (req, res) => {
    //     handleRedirect(req, res);
    // });

    const router = express.Router();
    // router.route("*").post(verifyRequest, verifyQuotaLimit, verifyRateLimit, redirectPostRequest);
    // router.route("*").get(verifyRequest, verifyQuotaLimit, verifyRateLimit, redirectGetRequest);
    router.route("*").post(redirectPostRequest);
    router.route("*").get(redirectGetRequest);
    // serviceMetadata.endpoints.forEach(endpoint => {
    //     if (endpoint.method.toLowerCase() == "post") {
    //         router.route(endpoint.route).post(verifyRequest, verifyQuotaLimit, verifyRateLimit, redirectPostRequest);
    //     } else if (endpoint.method.toLowerCase() == "get") {
    //         router.route(endpoint.route).get(verifyRequest, verifyQuotaLimit, verifyRateLimit, redirectGetRequest);
    //     }
    // })
    app.use('/', router);

    app.listen(port++, 'localhost', () => {
        console.log(`Server is running on port ${port - 1}`);
    });
}

module.exports = {
    createProxyServer
}