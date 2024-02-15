using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.RolesDTO
{
    public class GetUserInformationResponseDTO
    {
        public string FullName { get; set; }

        public Roles Role { get; set; }
    }
}
