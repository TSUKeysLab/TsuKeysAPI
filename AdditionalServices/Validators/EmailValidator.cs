using System;
using System.Text.RegularExpressions;

namespace tsuKeysAPIProject.AdditionalServices.Validators
{
    public class EmailValidator
    {
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, emailRegex);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
