using tsuKeysAPIProject.DBContext.DTO.RequestDTO;

namespace tsuKeysAPIProject.DBContext.DTO.RolesDTO
{
    public class GetUsersPageDTO
    {
        public IQueryable<GetUserInformationResponseDTO> Users { get; set; }
        public PaginationDTO Pagination { get; set; }
    }
}
