using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.RolesDTO
{
    public class GetUserInformationResponseDTO
    {
        public string Name { get; set; }

        public string Lastname { get; set; }
        public Roles Role { get; set; }
    }
}
