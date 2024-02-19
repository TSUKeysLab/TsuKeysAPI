using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class AvailableUsersResponseDTO
    {
        public string FullName { get; set; }
        public Roles Role { get; set; }
        public string UserEmail { get; set; }
    }
}
