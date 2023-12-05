const EmailValidator = require('deep-email-validator');
const PasswordValidator = require('password-validator');
const validator = require('validator')

async function isEmailValid(email) {
    return String(email)
        .toLowerCase()
        .match(
            /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        );
}

function isUsernameValid(username) {
    let isValid = {
        valid: true,
        message: ''
    }
    if (username.length < 6 || username.length > 15) {
        isValid.valid = false;
        isValid.message = "Username length must >= 6 and <= 15";
    }
    if (!validator.matches(username, "^[a-zA-Z0-9_\.]*$")) {
        isValid.valid = false;
        isValid.message = "Username must only contains a-z, A-Z, 0-9, _, and .";
    }
    return isValid;
}

function isPasswordValid(password) {
    let isValid = {
        valid: true,
        message: ''
    }

    const passwordValidator = new PasswordValidator();
    passwordValidator
        .is().min(8)                                    // Minimum length 8
        .is().max(25)                                  // Maximum length 25
        .has().uppercase()                              // Must have uppercase letters
        .has().lowercase()                              // Must have lowercase letters
        .has().digits()                                // Must have at least 2 digits
        .has().not().spaces()                           // Should not have spaces

    let validateResults = passwordValidator.validate(password, {details: true});
    isValid.valid = validateResults.length === 0;

    if (!isValid.valid) {
        for (let result of validateResults) {
            isValid.message += result.message.replace("The string", "Password") + '. ';
        }
        isValid.message = isValid.message.trim();
    }
    return isValid;

}
function isOAuthSignUpDataValid(username) {
    let isValid = {
        valid: true,
        message: ''
    }
    const validUsername = isUsernameValid(username);
    if (validUsername.valid === false) {
        isValid.valid = false;
        isValid.message = validUsername.message;
    }
    return isValid;
}
async function isSignUpDataValid(email, password, username) {
    let isValid = {
        valid: true,
        message: ''
    }
    const validUsername = isUsernameValid(username);
    if (validUsername.valid === false) {
        isValid.valid = false;
        isValid.message = "Please provide a valid username.";
    } else {
        const validEmail = isEmailValid(email);
        if (validEmail.valid === false) {
            isValid.valid = false;
            isValid.message = "Please provide a valid email address.";
        } else {
            const validPassword = isPasswordValid(password);
            isValid.valid = validPassword.valid;
            isValid.message = validPassword.message;
        }
    }

    return isValid;
}
module.exports = {
    isSignUpDataValid,
    isPasswordValid,
    isUsernameValid,
    isOAuthSignUpDataValid,
    isEmailValid
}