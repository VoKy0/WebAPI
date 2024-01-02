using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace webapi_csharp.services
{
    public class validationServices
    {
        public static bool isEmailValid(string email) {
            string pattern = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|.("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
            return Regex.IsMatch(email.ToLower(), pattern);
        }

        public static (bool valid, string message) isUsernameValid(string username) {
            bool isValid = true;
            string message = "";

            if (username.length < 6 || username.length > 15) {
                isValid = false;
                message = "Username length must >= 6 and <= 15";
            }
            if (!Regex.IsMatch(username, "^[a-zA-Z0-9_\\.]*$")) {
                isValid = false;
                message = "Username must only contains a-z, A-Z, 0-9, _, and .";
            }

            return (isValid, message);
        } 

        public static (bool valid, string message) isPasswordValid(string password) {
            bool isValid = true;
            string message = "";

            var passwordValidator = new PasswordValidator();
            passwordValidator
                .HasMinimumLength(8);           // Minimum length 8
                .HasMaximumLength(25);          // Maximum length 25
                .HasUpperCase();                // Must have uppercase letters
                .HasLowerCase();                // Must have lowercase letters
                .HasDigit();                    // Must have at least 1 digits
                .HasNoSpace()                   // Should not have spaces

            var valiadteResults = passwordValidator.Validate(password)

            if (valiadteResults.Any()) {
                isValid = false;
                message = string.Join(". ", validateResult.Select(result => result.Replace("The string", "Password"))) + ".";
            }

            return (isValid, message);
        }

        public static (bool valid, string message) isOAuthSignUpDataValid(string username) {
            var validUsername = isUsernameValid(username);
            return (validUsername.valid, validUsername.message);
        }

        public static (bool valid, string message) isSignUpDataValid(string email, string password, string username) {
            var validUsername = isUsernameValid(username);
            if (!validUsername.valid) {
                return (false, "Please provide a valid username.");
            }

            var validEmail = isEmailValid(email);
            if (!validEmail) {
                return (false, "Please provide a valid email address.");
            }

            var validPassword = isPasswordValid(password);
            if (!validPassword.valid) {
                return (validPassword.valid, validPassword.message);
            }
        }
    }

    public class PasswordValidator
    {
        private readonly PasswordRules rules;

        public PasswordValidator()
        {
            rules = new PasswordRules();
        }

        public PasswordValidator HasMinimumLength(int length)
        {
            rules.MinLength = length;
            return this;
        }

        public PasswordValidator HasMaximumLength(int length)
        {
            rules.MaxLength = length;
            return this;
        }

        public PasswordValidator HasUpperCase()
        {
            rules.RequireUpperCase = true;
            return this;
        }

        public PasswordValidator HasLowerCase()
        {
            rules.RequireLowerCase = true;
            return this;
        }

        public PasswordValidator HasDigit()
        {
            rules.RequireDigit = true;
            return this;
        }

        public PasswordValidator HasNoSpace()
        {
            rules.RequireNoSpace = true;
            return this;
        }

        public List<string> Validate(string password)
        {
            var errors = new List<string>();

            if (rules.MinLength > 0 && password.Length < rules.MinLength)
            {
                errors.Add($"The string must have at least {rules.MinLength} characters.");
            }

            if (rules.MaxLength > 0 && password.Length > rules.MaxLength)
            {
                errors.Add($"The string must have at most {rules.MaxLength} characters.");
            }

            if (rules.RequireUpperCase && !password.Any(char.IsUpper))
            {
                errors.Add("The string must contain at least one uppercase letter.");
            }

            if (rules.RequireLowerCase && !password.Any(char.IsLower))
            {
                errors.Add("The string must contain at least one lowercase letter.");
            }

            if (rules.RequireDigit && !password.Any(char.IsDigit))
            {
                errors.Add("The string must contain at least one digit.");
            }

            if (rules.RequireNoSpace && password.Any(char.IsWhiteSpace))
            {
                errors.Add("The string must not contain any spaces.");
            }

            return errors;
        }

        private class PasswordRules
        {
            public int MinLength { get; set; }
            public int MaxLength { get; set; }
            public bool RequireUpperCase { get; set; }
            public bool RequireLowerCase { get; set; }
            public bool RequireDigit { get; set; }
            public bool RequireNoSpace { get; set; }
        }
    }
}