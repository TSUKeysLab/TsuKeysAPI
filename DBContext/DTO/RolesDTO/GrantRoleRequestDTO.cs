using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.RolesDTO
{
    public class GrantRoleRequestDTO
    {
        public Guid Id { get; set; }
        public Roles Role { get; set; }
    }
}
