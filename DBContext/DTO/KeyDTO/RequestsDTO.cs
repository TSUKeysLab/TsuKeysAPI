using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class RequestsDTO
    {
        public string KeyOwner { get; set; }
        public string KeyRecipient { get; set; }

        public string ClassroomNumber { get; set; }
        public RequestStatus Status { get; set; }
    }
}
