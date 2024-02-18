namespace tsuKeysAPIProject.DBContext.DTO.RequestDTO
{
    public class GetRequestsPageDTO
    {
        public IQueryable Requests { get; set; }
        public PaginationDTO Pagination { get; set; }
    }
}
