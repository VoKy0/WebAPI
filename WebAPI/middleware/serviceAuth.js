const jwt = require("jsonwebtoken");
const {RedisServices} = require("../services/RedisServices");
const Plan = require("../models/Plan");

const redisProvider = new RedisServices()
redisProvider.connect();

const calculateTimePeriodInSecond = (interval) => {
    switch(interval) {
        case "daily": return 24 * 60 * 60;
        case "monthly": return 30 * 24 * 60 * 60;
        case "minutely": return 60;
        case "secondly": return 1;
        case "hourly": return 60 * 60;
        default: return 1;
    }
}

const verifyRateLimit = async (req, res, next) => {
    try {
        const currentTimestamp = Math.floor(Date.now() / 1000);
        const key = `rl:${req.headers["x-softzoneapi-key"]}:${currentTimestamp}`;
        const count = await redisProvider.getClient().incr(key);

        const limitInfo = await Plan.findPlanLimitDetailBySubscriptionKey(req.app.get('db'), key);

        // Set expiration for the rate limiting key
        redisProvider.getClient().expire(key, calculateTimePeriodInSecond(limitInfo.rl_interval));

        if (count > limitInfo.rl_value) {
            res.status(429).json({ error: 'Rate limit exceeded' });
        } else {
            return next(); // Continue with the request
        }
    } catch (error) {
        res.status(500).json({ error: 'Rate limiting error' });
    }
}

const verifyQuotaLimit = async (req, res, next) => {
    try {
        const currentTimestamp = Math.floor(Date.now() / 1000);
        const key = `rl:${req.headers["x-softzoneapi-key"]}:${currentTimestamp}`;
        const count = await redisProvider.getClient().incr(key);
        const limitInfo = await Plan.findPlanLimitDetailBySubscriptionKey(req.app.get('db'), key);
        console.log('limitinfo:', limitInfo);
        // Set expiration for the rate limiting key
        redisProvider.getClient().expire(key, calculateTimePeriodInSecond(limitInfo.ql_interval));
        console.log(limitInfo)
        if (count > limitInfo.rl_value) {
            if (limitInfo.ql_allow_overage) {

            } else {

            }
            res.status(429).json({ error: 'Quota limit exceeded' });
        } else {
            return next(); // Continue with the request
        }
    } catch (error) {
        console.log(error)
        res.status(500).json({ error: 'Quota limiting error' });
    }
}

const verifyRequest = (req, res, next) => {
    let softzoneToken = req.headers["x-softzoneapi-key"];
    console.log(softzoneToken);
    if (!softzoneToken) {
        return res.status(403).send("A token is required for authentication");
    } else if (softzoneToken.startsWith('Bearer ')) {
        softzoneToken = softzoneToken.substring(7);
    }
    try {
        const decoded = jwt.verify(softzoneToken, req.app.get('secret'));
        return next();
    } catch (err) {
        return res.status(401).send("Invalid Token");
    }
};

module.exports = {
    verifyRequest,
    verifyRateLimit: verifyRateLimit,
    verifyQuotaLimit: verifyQuotaLimit
}