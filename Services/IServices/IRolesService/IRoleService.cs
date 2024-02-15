using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;

namespace tsuKeysAPIProject.Services.IServices.IRolesService
{
    public interface IRoleService
    {
        public Task grantRole(GrantRoleRequestDTO grantRole, string token);
        public Task<GetUserInformationResponseDTO> getUserInformation(GetUserInformationRequestDTO userInformation);
    }
}
