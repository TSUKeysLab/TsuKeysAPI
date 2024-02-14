using System.ComponentModel.DataAnnotations;
using System.Reflection;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public DateOnly BirthDate { get; set; }

        public Gender Gender { get; set; }

        [EmailAddress(ErrorMessage = "Неправильный формат email")]
        public string Email { get; set; }

        public Roles role { get; set; }

        public string Password { get; set; }
    }
}
