namespace testbook.Method
{
    public static class PasswordSecurity
    {
        public static string HashPasswords(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password, string HashPasswords)
        {
            return BCrypt.Net.BCrypt.Verify(password, HashPasswords);
        }

    }
}
