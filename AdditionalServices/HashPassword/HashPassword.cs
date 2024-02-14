namespace tsuKeysAPIProject.AdditionalServices.HashPassword
{
    public static class HashPassword
    {
        public static string HashingPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
