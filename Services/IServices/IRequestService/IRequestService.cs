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

        public Task<GetRequestsPageDTO> getAllUsersRequests(List<RequestStatus> statuses, string token, int page, int size);

        public Task approveRequest(ApproveRequestDTO approveRequestDTO, string token);

        public Task rejectRequest(RejectRequestDTO rejectRequestDTO, string token);

        public Task deleteRequest(DeleteRequestDTO deleteRequestDTO, string token);
    }
}
