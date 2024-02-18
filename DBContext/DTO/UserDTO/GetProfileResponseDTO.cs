using System.ComponentModel.DataAnnotations;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.UserDTO
{
    public class GetProfileResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Fullname { get; set; }
        public DateOnly BirthDate { get; set; }

        public Gender Gender { get; set; }

        public Roles Role { get; set; }

        public string Email { get; set; }
    }
}
