using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.Services.IServices.IRolesService
{
    public interface IRoleService
    {
        public Task grantRole(GrantRoleRequestDTO grantRole, string token);
        public Task<GetUsersPageDTO> getUsersInformation(string token, string? fullname, Roles? role, int size, int page);
    }
}
