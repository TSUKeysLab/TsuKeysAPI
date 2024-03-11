using System;
using System.Text.RegularExpressions;

namespace tsuKeysAPIProject.AdditionalServices.Validators
{
    public class fullNameValidator
    {
        public static bool ValidateFullName(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }
            try
            {
                string fullNameRegex = @"^.{2,30}$";
                return Regex.IsMatch(line, fullNameRegex);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
