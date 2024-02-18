using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.Services.IServices.IKeyService
{
    public interface IKeyService
    {
        public Task CreateKey(CreateKeyDTO createKeyDTO, string token);
        public Task DeleteKey(string classroom, string token);
        public Task SendKeyRequest(KeyRequestDTO keyRequestDTO, string token);
        public Task UpdateKeyRequestStatus(string classroomNumber, string token, RequestStatus status);

        public Task GetAllKeys();

        public Task GetAllRequests();
        public Task ConfirmReceipt(string classroomNumber, string token);

        //TODO Сделать выдачу всех входящих заявок для конкретного пользователя
    }
}