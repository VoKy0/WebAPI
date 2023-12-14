const {isSignUpDataValid, isOAuthSignUpDataValid, isPasswordValid} = require('../services/validationServices');
const {verifyRefreshToken, generateTokens} = require('../services/JWTServices')
const Auth = require('../Models/Auth');
const User = require('../Models/User');
const config = require("config");
const jwt = require('jsonwebtoken');
const fs = require('fs')
const speakeasy = require('speakeasy')
const nodemailer = require('nodemailer')
const twilio = require('twilio')(config.get('PRIVATE_INFORM.TWILIO.ACCOUNT_SID'), config.get('PRIVATE_INFORM.TWILIO.AUTH_TOKEN'))

const generateAccessToken = async (req, res) => {
    const db = req.app.get('db');
    let userInfo = req.body.user_info;
    const { accessToken, refreshToken } = await generateTokens(userInfo);
    userInfo["token"] = accessToken;
    const isTokenUpdated = await Auth.updateRefreshToken(db, refreshToken, userInfo["id"]);
    if (isTokenUpdated) {
        res.status(200).json({"success": true, "message": "Generate tokens successfully.", "data": [userInfo]})
    } else {
        res.status(200).json({"success": false, "message": "DB: Update refresh token failed.", "data": []})
    }
}

const signIn = (req, res) => {
    const identifier = req.body.identifier;
    const password = req.body.password;

    const db = req.app.get('db');
    db.raw('SELECT fn_sign_in(:identifier, :password) as user_info', { identifier, password }).then(async result => {
        const { accessToken, refreshToken } = await generateTokens(result.rows[0].user_info);
        result.rows[0].user_info["token"] = accessToken;
        const isTokenUpdated = await Auth.updateRefreshToken(db, refreshToken, result.rows[0].user_info["id"]);
        if (isTokenUpdated) {
            res.status(200).json({"success": true, "message": "Signed in successfully.", "data": result.rows})
        } else {
            throw new Error("DB: Update refresh token failed");
        }

    }).catch(error => {
        console.log(error.message);
        res.status(200).json({"success": false, "message": "Invalid email address or password", "data": []});
    });
}
const signUp = (req, res) => {
    const first_name = req.body.first_name;
    const last_name = req.body.last_name;
    const email = req.body.email;
    const password = req.body.password;
    const username = req.body.username;
    isSignUpDataValid(email, password, username).then(isDataValid => {
        if (isDataValid.valid) {
            const db = req.app.get('db');
            db.raw('Call prc_sign_up(:first_name, :last_name, :email, :password, :username)', { first_name, last_name, email, password, username }).then(() => {
                res.status(200).json({"success": true, "message": "Signed up successfully.", "data": []})
            }).catch(error => {
                console.log(error);
                res.status(200).json({"success": false, "message": "Email is already existed", "data": []});
            })

        } else {
            console.log(isDataValid)
            res.status(200).json({"success": false, "message": isDataValid.message, "data": []});
        }
    });
}

const signOut = (req, res) => {
    const db = req.app.get('db');
    try {
        const isTokenUpdated = Auth.updateRefreshToken(db, null, req.query.user_id);
        if (isTokenUpdated) {
            res.status(200).json({"success": true, "message": "Signed out successfully.", "data": []})
        } else {
            throw new Error("DB: Update refresh token failed");
        }
    } catch(err) {
        console.log(err);
        res.status(500).json({ error: true, message: "Internal Server Error: " + err.message });
    }
}
const generateVerifyAccountToken = (email) => {
    const payload = {_email: email, _current: new Date()};
    const accessToken = jwt.sign(
        payload,
        config.get("AUTH.JWT.ACCESS_TOKEN.PRIVATE_KEY"),
        {expiresIn: config.get("AUTH.JWT.ACCESS_TOKEN.VERIFY_EMAIL_EXPIRES_IN")}
    );
    return Promise.resolve(accessToken);
}
const confirmEmailOnSignUp = (req, res) => {
    const db = req.app.get('db');
    const mailer = req.app.get('mailer');
    generateVerifyAccountToken(req.body.email).then(async (verifyToken) => {
        const userInfo = await User.findByEmail(db, req.body.email);
        const userId = userInfo.id;
        const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, verifyToken, userId);
        if (isVerifyTokenUpdated === true) {
            const formEmailValidation = fs.readFileSync('assets/html/formEmailVerification.html', 'utf8');
            const recipient_email = req.body.email
            let content = formEmailValidation.replaceAll('{{reset_link}}', `${config.get('PROTOCOL')}://${config.get("CLIENT_HOST")}/user/auth/verify?token=${verifyToken}`);
            content = content.replaceAll('{{user_name}}', userInfo.username);
            content = content.replaceAll('{{user_email}}', userInfo.email);
            let transporter = nodemailer.createTransport({
                service: "gmail",
                auth: {
                    user: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS'),
                    pass: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.PASSWORD'),
                },
            });
            const mail_configs = {
                from: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS'),
                to: recipient_email,
                subject: "Email Confirm",
                html: content,
            };


            transporter.sendMail(mail_configs, function (error, info) {
                if (error) {
                    console.log(error);
                    res.status(200).json({"success": false, "message": "Email sent failed.", "data": []});
                }
                res.status(200).json({"success": true, "message": "Email sent successfully.", "data": []});
            });
        } else {
            res.status(200).json({"success": false, "message": "Update verify token failed", "data": []});
        }
    }).catch(error => {
        res.status(200).json({"success": false, "message": "Generate verify token failed", "data": []});
    })
}

const OAuthSignUp = async(req, res) => {
    const isDataValid = isOAuthSignUpDataValid(req.body.username);
    if (isDataValid.valid) {
        const db = req.app.get('db');
        const isUsernameExists = await User.findByUsername(db, req.body.username);
        if (isUsernameExists) {
            res.status(200).json({
                "success": false,
                "message": "The username already exists. Please choose another username.",
                "data": []
            })
        } else {
            try {
                db("users").where({id: req.body.id}).update({
                    username: req.body.username
                }).returning('*').then(async row => {
                    const userInfo = row[0];
                    const {accessToken, refreshToken} = await generateTokens(userInfo);
                    userInfo["token"] = accessToken;
                    const isTokenUpdated = await Auth.updateRefreshToken(db, refreshToken, userInfo.id);
                    if (isTokenUpdated) {
                        const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, null, userInfo.id);
                        if (isVerifyTokenUpdated === true) {
                            res.status(200).json({
                                "success": true,
                                "message": "Data successfully updated to the database.",
                                "data": [userInfo]
                            })
                        } else {
                            res.status(200).json({"success": false, "message": "Update token failed.", "data": []});
                        }

                    } else {
                        res.status(200).json({
                            "success": false,
                            "message": "Token update failed",
                            "data": row
                        })
                    }
                })
            } catch (error) {
                res.status(200).json({
                    "success": false,
                    "message": error.message,
                    "data": []
                })
            }
        }
    } else {
        res.status(200).json({"success": false,
            "message": isDataValid.message,
            "data": []})
    }
}
const OAuthLogin = async(req, res) => {
    const token = req.body.token;
    const db = req.app.get('db');
    if (!token) {
        res.status(200).json({"success": false, "message": "Verify failed.", "data": []});
    } else {
        let userId = (await Auth.findUserByVerifyToken(db, token));
        if (userId != null) {
            userId = userId.user_id;
            const userInfo = await User.findById(db, userId);
            console.log('loginoauth: ', userInfo)
            const { accessToken, refreshToken } = await generateTokens(userInfo);
            userInfo["token"] = accessToken;
            const isTokenUpdated = await Auth.updateRefreshToken(db, refreshToken, userInfo["id"]);
            if (isTokenUpdated) {
                res.status(200).json({"success": true, "message": "OAuth login successfully.", "data": [userInfo]})
            } else {
                res.status(200).json({"success": false, "message": "DB: Update refresh token failed.", "data": []})
            }
        } else {
            res.status(200).json({"success": false, "message": "Verify failed.", "data": []});
        }
    }
}
const manualVerifyToken = async (req, res) => {
    const token = req.body.token;
    const db = req.app.get('db');
    if (!token) {
        res.status(200).json({"success": false, "message": "Verify failed.", "data": []});
    }
    try {
        const decoded = jwt.verify(token, config.get('AUTH.JWT.ACCESS_TOKEN.PRIVATE_KEY'));
        let userId = (await Auth.findUserByVerifyToken(db, token));
        if (userId != null) {
            userId = userId.user_id;
            res.status(200).json({"success": true, "message": "Verify successfully.", "data": [{id: userId}]});
        } else {
            res.status(200).json({"success": false, "message": "Verify failed.", "data": []});
        }
    } catch (err) {
        console.log("invalid token: ", err)
        res.status(200).json({"success": false, "message": "Verify failed.", "data": []});
    }
}

const confirmEmailOnResetPassword = (req, res) => {
    const db = req.app.get('db');
    generateVerifyAccountToken(req.body.email).then(async (verifyToken) => {
        const userInfo = await User.findByEmail(db, req.body.email);
        const userId = userInfo.id;
        const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, verifyToken, userId);
        if (isVerifyTokenUpdated === true) {
            const formEmailValidation = fs.readFileSync('assets/html/formEmailVerification.html', 'utf8');
            const recipient_email = req.body.email
            let content = formEmailValidation.replaceAll('{{reset_link}}', `https://platform.softzone.ai/user/reset-password?token=${verifyToken}`)
            content = content.replaceAll('{{user_name}}', userInfo.username);
            content = content.replaceAll('{{user_email}}', userInfo.email);
            let transporter = nodemailer.createTransport({
                service: "gmail",
                auth: {
                    user: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS'),
                    pass: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.PASSWORD'),
                },
            });
            const mail_configs = {
                from: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS'),
                to: recipient_email,
                subject: "Email Confirm",
                html: content,
            };


            transporter.sendMail(mail_configs, function (error, info) {
                if (error) {
                    console.log(error);
                    res.status(200).json({"success": false, "message": "Email sent failed.", "data": []});
                }

                res.status(200).json({"success": true, "message": "Email sent successfully.", "data": []});
            });
        } else {
            res.status(200).json({"success": false, "message": "Update verify token failed", "data": []});
        }
    }).catch(error => {
        res.status(200).json({"success": false, "message": "Generate verify token failed", "data": []});
    })

}

const confirmEmailOn2FASecurity = (req, res) => {
    const db = req.app.get('db');
    generateVerifyAccountToken(req.body.email).then(async (verifyToken) => {
        const userInfo = await User.findByEmail(db, req.body.email);
        const userId = userInfo.id;
        const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, verifyToken, userId);
        if (isVerifyTokenUpdated === true) {
            const formEmailValidation = fs.readFileSync('assets/html/formEmailVerification.html', 'utf8');
            const recipient_email = req.body.email
            let content = formEmailValidation.replaceAll('{{reset_link}}', `https://platform.softzone.ai/user/settings/security?token=${verifyToken}`)
            content = content.replaceAll('{{user_name}}', userInfo.username);
            content = content.replaceAll('{{user_email}}', userInfo.email);
            let transporter = nodemailer.createTransport({
                service: "gmail",
                auth: {
                    user: config.get("PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS"),
                    pass: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.PASSWORD'),
                },
            });
            const mail_configs = {
                from: config.get('PRIVATE_INFORM.EMAIL.GOOGLE.ADDRESS'),
                to: recipient_email,
                subject: "Email Confirm",
                html: content,
            };


            transporter.sendMail(mail_configs, function (error, info) {
                if (error) {
                    console.log(error);
                    res.status(200).json({"success": false, "message": "Email sent failed.", "data": []});
                }

                res.status(200).json({"success": true, "message": "Email sent successfully.", "data": []});
            });
        } else {
            res.status(200).json({"success": false, "message": "Update verify token failed", "data": []});
        }
    }).catch(error => {
        res.status(200).json({"success": false, "message": "Generate verify token failed", "data": []});
    })

}

const updatePassword = async (req, res) => {
    const db = req.app.get('db');
    const newPassword = req.body.new_password;
    const oldPassword = req.body.old_password;
    const userId = req.body.user_id;
    const truePassword = (await Auth.findPasswordByUserId(db, userId)).password;
    try {
        db.raw('SELECT compare_password(:truePassword, :oldPassword)', { truePassword, oldPassword }).then(async result => {
            if(result.rows[0].compare_password === true) {
                const passwordValidation = isPasswordValid(newPassword);
                if (passwordValidation.valid) {
                    const hashedNewPassword = (await db.raw(
                        `select crypt('${newPassword}', gen_salt('bf'))`
                    )).rows[0].crypt;

                    db("authentication").where({user_id: userId}).update({
                        password: hashedNewPassword
                    }).then(row => {
                        res.status(200).json({
                            "success": true,
                            "message": "Data successfully updated to the database.",
                            "data": row
                        })
                    }).catch(err => {
                        res.status(500).json({
                            success: false,
                            data: [],
                            message: err.message,
                        });
                    })
                } else {
                    res.status(200).json({
                        success: false,
                        data: [],
                        message: passwordValidation.message,
                    });
                }
            } else {
                res.status(200).json({
                    success: false,
                    data: [],
                    message: "Incorrect password",
                });
            }
        })
    } catch(err) {
        res.status(500).json({
            success: false,
            data: [],
            message: err.message,
        });
    }
}

const recoverPassword = async (req, res)=>{
    const newPassword = req.body.password;
    const token = req.body.token;
    const db = req.app.get('db');
    let userId = await Auth.findUserByVerifyToken(db, token);
    if (userId != null) {
        userId = userId.user_id;
        const userInfo = await User.findById(db, userId);
        const email = userInfo.email;
        const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, null, userId);
        if (isVerifyTokenUpdated) {
            try {
                db.raw('Call prc_reset_password(:email, :newPassword)', { email, newPassword }).then(() => {
                    res.status(200).json({"success": true, "message": "Password updated successfully.", "data": []})
                }).catch(error => {
                    res.status(200).json({"success": false, "message": error.message, "data": []});
                })
            } catch (err) {
                console.log('error here: ', err.message);
                res.status(200).json({"success": false, "message": err.message, "data": []})
            }
        } else {
            res.status(500).json({
                success: false,
                data: [],
                message: "Token verified failed",
            });
        }
    } else {
        res.status(200).json({
            success: false,
            data: [],
            message: "Token verified failed",
        });
    }

}

const reGenerateAccessToken = async (req, res) => {
    const db = req.app.get('db');
    console.log('refreshtoken userid: ', req.body.user_id);
    const refreshToken = (await Auth.findTokenByUserId(db, req.body.user_id)).refresh_token;
    verifyRefreshToken(refreshToken)
        .then(({ tokenDetails }) => {
            const payload = { _id: tokenDetails._id, roles: tokenDetails.roles };
            const accessToken = jwt.sign(
                payload,
                config.get("AUTH.JWT.ACCESS_TOKEN.PRIVATE_KEY"),
                { expiresIn: config.get("AUTH.JWT.ACCESS_TOKEN.EXPIRES_IN") }
            );
            console.log('generated new access token')
            res.status(200).json({
                success: true,
                data: [{
                    token: accessToken
                }],
                message: "Access token created successfully",
            });
        })
        .catch((err) =>
            res.status(401).json({
                success: false,
                data: [],
                message: err
            })
        );
}

const sendSMSOtp = (req, res)=>{
    const { phoneNumber } = req.body;

    // Tạo secret và mã OTP
    const secret = speakeasy.generateSecret({ length: 20 }).base32;
    console.log('secret', secret)
    const token = speakeasy.totp({
        secret: secret, // nên lưu vào database hoặc backend
        encoding: 'base32',
        step: 60,
    });

    console.log('Generated Token:', token);

    // Gửi mã OTP qua SMS
    twilio.messages
        .create({
            body: `Your OTP code is: ${token}`,
            to: phoneNumber,
            from: config.get('PRIVATE_INFORM.PHONE'),
        })
        .then((message) => res.status(200).json({ success: true, secret, token, 'message.sid': message.sid }))
        .catch((error) => {
            console.error('Error sending OTP:', error);
            res.status(500).json({ success: false, error: error.message });
        });
}
const checkPhoneNumber = (req, res)=>{
   try{
       twilio.lookups.v2.phoneNumbers(req.body.phoneNumber)
           .fetch()
           .then(phone_number => {
               if(phone_number.valid === true){
                   res.status(200).json({ success: true, valid: true })
               }else{
                   res.status(200).json({ success: true, valid: false })
               }
           })
           .catch((error) => {
               console.error('Phone Number Error:', error);
               res.status(500).json({ success: false, error: error.message });
           });
   }
    catch (err) {
        res.status(500).json({
            success: false,
            message: err.message,
        });
    }
}
const verifyAccount = async (req, res) => {
    const db = req.app.get('db');
    const token = req.body.token;
    let userId = (await Auth.findUserByVerifyToken(db, token));
    if (userId != null) {
        userId = userId.user_id;
        const userInfo = await User.findById(db, userId);
        db('users').update({
            active: true,
        }).where('id', userId).then(async () => {
            const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, null, userId);
            if (isVerifyTokenUpdated) {
                res.status(200).json({
                    success: true,
                    data: [userInfo],
                    message: "Account verified successfully",
                });

            } else {
                res.status(200).json({
                    success: false,
                    data: [],
                    message: "Account verified failed",
                });
            }

        }).catch(error => {
            console.log(error)
            res.status(200).json({
                success: false,
                data: [userInfo],
                message: error.message,
            });
        })

    } else {
        res.status(200).json({
            success: false,
            data: [],
            message: "Account verified failed",
        });
    }


}

const verifiedOtp = (req, res)=>{
    const { secret, token } = req.body;
    console.log('secret', secret)
    console.log(token)
    const verified = speakeasy.totp.verify({
        secret: secret,
        encoding: 'base32',
        token: token,
        step: 60
    });

    if (verified) {
        console.log('Verified');
        res.status(200).json({success: true, message: 'OTP is valid'});
    } else {
        console.log('Not Verified');
        res.status(200).json({success: false, message: 'OTP is invalid'});
    }
}
module.exports = {
    signIn,
    signUp,
    signOut,
    recoverPassword,
    confirmEmail: confirmEmailOnResetPassword,
    reGenerateAccessToken,
    sendSMSOtp,
    verifiedOtp,
    updatePassword,
    confirmEmailOnSignUp,
    verifyAccount,
    setUsernameOnSignUp: OAuthSignUp,
    generateAccessToken,
    manualVerifyToken,
    generateVerifyAccountToken,
    OAuthLogin,
    confirmEmailOn2FASecurity,
    checkPhoneNumber
}