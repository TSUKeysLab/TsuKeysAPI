using tsuKeysAPIProject.DBContext.DTO.RolesDTO;

namespace tsuKeysAPIProject.DBContext.DTO.RequestDTO
{
    public class GetRequestsPageDTO
    {
        public IQueryable<GetAllRequestsDTO> Requests { get; set; }
        public PaginationDTO Pagination { get; set; }
    }
}
