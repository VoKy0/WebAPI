const express = require('express')
const passport = require('passport')
const router = express.Router()
const {generateVerifyAccountToken} = require("../Controllers/authController");
const User = require("../models/User");
const Auth = require("../models/Auth");

router.get('/google', passport.authenticate('google', { scope: ['profile','email'] }))

router.get(
    '/google/callback',
    passport.authenticate('google', { failureRedirect: 'http://localhost:3000/user/login' }),
    (req, res) => {
        const user = req.user;
        const db = req.app.get('db');
        generateVerifyAccountToken(user.email).then(async (verifyToken) => {
            const userId = user.id;
            if (user.username !== null) {
                // login
                const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, verifyToken, userId);
                if (isVerifyTokenUpdated === true) {
                    res.redirect(`http://localhost:3000/user/login/google?token=${verifyToken}`);
                }
            } else {
                // signup
                const isAccountExists = await Auth.findByUserId(db, userId);
                let isVerifyTokenCreated;
                if (isAccountExists) {
                    isVerifyTokenCreated = await Auth.update(db, userId, verifyToken, null, null);
                } else {
                    isVerifyTokenCreated = await Auth.create(db, userId, verifyToken, null, null);
                }

                if (isVerifyTokenCreated === true) {
                    res.redirect(`http://localhost:3000/user/signup/google?token=${verifyToken}`);
                }
            }

        });

    }
)

router.get('/facebook', passport.authenticate('facebook'));

router.get('/facebook/callback',
    passport.authenticate('facebook', { failureRedirect: 'http://localhost:3000/user/login' }),
    function(req, res) {
    console.log("success")
        // Successful authentication, redirect or respond as needed
        res.redirect('http://localhost:3000/');
    });

router.get('/github', passport.authenticate('github', {scope: ['user:email']}));

router.get('/github/callback',
    passport.authenticate('github', { failureRedirect: 'http://localhost:3000/user/login' }),
    function(req, res) {
        // Successful authentication, redirect or respond as needed
        const user = req.user;
        const db = req.app.get('db');
        generateVerifyAccountToken(user.email).then(async (verifyToken) => {
            const userId = user.id;
            if (user.username !== null) {
                // login
                const isVerifyTokenUpdated = await Auth.updateVerifyToken(db, verifyToken, userId);
                if (isVerifyTokenUpdated === true) {
                    res.redirect(`http://localhost:3000/user/login/github?token=${verifyToken}`);
                }
            } else {
                // signup
                const isVerifyTokenCreated = await Auth.create(db, userId, verifyToken, null, null);
                if (isVerifyTokenCreated === true) {
                    res.redirect(`http://localhost:3000/user/signup/github?token=${verifyToken}`);
                }
            }

        });
    });

router.get('/logout', (req, res) => {
    req.logout()
    res.redirect('http://localhost:3000/users/login')
})

function ensureAuthenticated(req, res, next) {
    if (req.isAuthenticated()) {
        return next();
    }
    res.redirect('http://localhost:3000/users/login'); // Redirect to login if not authenticated
}

// Use the ensureAuthenticated middleware to protect routes
router.get(['/google', '/github', '/facebook'], ensureAuthenticated, (req, res) => {
    // Only accessible if authenticated
    console.log('abcasdf')
});


module.exports = router