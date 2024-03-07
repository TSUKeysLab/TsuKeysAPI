using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class UsersForTransferDTO
    {
        public IQueryable<GetUserInformationResponseDTO> Users { get; set; }
    }
}
