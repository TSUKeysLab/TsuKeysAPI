using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.RolesDTO
{
    public class GetUserInformationResponseDTO
    {
        
        public string Fullname { get; set; }

        public string Email { get; set; }
        public Roles Role { get; set; }
    }
}
