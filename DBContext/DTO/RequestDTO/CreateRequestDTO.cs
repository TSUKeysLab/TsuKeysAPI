using System.Text.Json.Serialization;

namespace tsuKeysAPIProject.DBContext.DTO.RequestDTO
{
    public class CreateRequestDTO
    {
        public string ClassroomNumber { get; set; }
        public Guid TimeId { get; set; }
        public DateOnly DateOfBooking { get; set; }
    }
}