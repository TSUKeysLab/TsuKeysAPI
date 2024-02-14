using System.ComponentModel.DataAnnotations;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.UserDTO
{
    public class RegisterRequestDTO
    {
        public string FullName { get; set; }
        public DateOnly BirthDate { get; set; }

        public Gender Gender { get; set; }

        [EmailAddress(ErrorMessage = "Неправильный формат email")]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
