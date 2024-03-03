using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class KeyRequestResponseDTO
    {
        public Guid RequestId { get; set; }
        public string? KeyOwnerFullName { get; set; }
        public string? KeyOwnerEmail { get; set; }
        public string? KeyRecipientFullName { get; set; }
        public string? KeyRecipientEmail { get; set; }
        public string ClassroomNumber { get; set; }
        public TimeOnly EndOfRequest {  get; set; }
        public KeyRequestStatus Status { get; set; }
    }
}
