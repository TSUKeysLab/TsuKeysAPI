using Microsoft.EntityFrameworkCore;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.AdditionalServices.UserInfoHelper
{
    public class UserInfoHelper
    {
        private readonly AppDBContext _db;
        
        public UserInfoHelper(AppDBContext db) {
            _db = db;
        }

        public async Task<Roles> GetUserRole(string email)
        {
            var userRole = await _db.Users
                .Where(u => u.Email == email)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();
            return userRole;
        }

    }
}
