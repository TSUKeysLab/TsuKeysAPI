using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.Services.IServices.IKeyService
{
    public interface IKeyService
    {
        public Task CreateKey(CreateKeyDTO createKeyDTO, string token);
        public Task DeleteKey(string classroom, string token);
        public Task SendKeyRequest(KeyRequestsDTO keyRequestDTO, string token);
        public Task UpdateKeyRequestStatus(Guid requestId, string token, KeyRequestStatus status);
        public Task<List<KeyInfoDTO>> GetAllKeys(DateOnly? dateOfRequest, int? timeId, string Token, KeyGettingStatus gettingStatus);
        public Task<List<KeyRequestResponseDTO>> GetAllRequests(RequestUserStatus userStatus, string token);
        public Task ConfirmReceiptFromUser(Guid requestId, string token);
        public Task ConfirmReceiptFromDean(Guid requestId, string token);
        public Task<List<UsersWithoutKeysDTO>> GetUsersWithoutKeys(string token);
        public Task DeleteKeyRequest(string token, Guid requestId);
    }
}