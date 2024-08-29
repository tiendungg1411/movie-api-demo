using Data.DataContext;

namespace MovieAPI.Common
{
    public class AuthenticateUser
    {
        private readonly MovieDbContext _context;
        public AuthenticateUser(MovieDbContext context)
        {
            _context = context;
        }

        public string PasswordEncryption(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
}
