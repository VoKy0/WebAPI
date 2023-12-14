const jwt = require('jsonwebtoken');
const config = require("config");
const Auth = require("../Models/Auth");

async function generateTokens (user) {
    try {
        const payload = {_id: user.id, roles: user.role};
        const accessToken = jwt.sign(
            payload,
            config.get("AUTH.JWT.ACCESS_TOKEN.PRIVATE_KEY"),
            {expiresIn: config.get("AUTH.JWT.ACCESS_TOKEN.EXPIRES_IN")}
        );
        const refreshToken = jwt.sign(
            payload,
            config.get("AUTH.JWT.REFRESH_TOKEN.PRIVATE_KEY"),
            { expiresIn: config.get("AUTH.JWT.REFRESH_TOKEN.EXPIRES_IN")}
        );
        return Promise.resolve({accessToken, refreshToken});
    } catch(err) {
        return Promise.reject(err);
    }
}
async function verifyRefreshToken(refreshToken) {
    const privateKey = config.get("AUTH.JWT.REFRESH_TOKEN.PRIVATE_KEY");
    return new Promise((resolve, reject) => {
        jwt.verify(refreshToken, privateKey, (err, tokenDetails) => {
            if (err) {
                return reject({ error: true, message: "Invalid refresh token"});
            } else {
                return resolve({
                    tokenDetails,
                    error: false,
                    message: "Valid refresh token"
                })
            }
        })
    })
}
module.exports = {
    generateTokens,
    verifyRefreshToken
}