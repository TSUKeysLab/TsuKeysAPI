using System;

namespace tsuKeysAPIProject.AdditionalServices.Validators
{
    public class DateOfBirthValidator
    {
        public static bool ValidateDateOfBirth(DateOnly dateOfBirth)
        {
            DateOnly now = DateOnly.FromDateTime(DateTime.UtcNow);
            int age = now.Year - dateOfBirth.Year;

            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            if (age < 13 || age > 100)
            {
                return false;
            }

            return true;
        }
    }
}
