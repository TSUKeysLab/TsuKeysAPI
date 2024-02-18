using System.Net;
using tsuKeysAPIProject.DBContext.DTO.RequestDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.Services.IServices.IRequestService
{
    public interface IRequestService
    {
        public Task createRequest(CreateRequestDTO createRequestDTO, string token);
        public Task<GetRequestsPageDTO> getAllRequestsDTO(List<RequestStatus> statuses, string token, int page,int size, string? classroomNumber, RequestSorting sorting, Guid? timeId);
    }
}
