using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class KeyRequestResponseDTO
    {
        public string? KeyOwnerFullName { get; set; }
        public string? KeyOwnerEmail { get; set; }
        public string? KeyRecipientFullName { get; set; }
        public string? KeyRecipientEmail { get; set; }
        public string ClassroomNumber { get; set; }
        public RequestStatus Status { get; set; }
    }
}
