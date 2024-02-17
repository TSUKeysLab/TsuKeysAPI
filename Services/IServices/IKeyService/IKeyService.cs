using tsuKeysAPIProject.DBContext.DTO.KeyDTO;

namespace tsuKeysAPIProject.Services.IServices.IKeyService
{
    public interface IKeyService
    {
        public Task CreateKey(CreateKeyDTO createKeyDTO, string token);
        public Task DeleteKey(string classroom);
        public Task SendKeyRequest(KeyRequestDTO keyRequestDTO);
        public Task AcceptKeyRequest(string classroomNumber);
        public Task DeclineKeyRequest(string classroomNumber);
        public Task GetAllKeys();

        public Task GetAllRequests();
        public Task ConfirmReceipt(string classroomNumber);

        //TODO Сделать выдачу всех входящих заявок для конкретного пользователя
    }
}