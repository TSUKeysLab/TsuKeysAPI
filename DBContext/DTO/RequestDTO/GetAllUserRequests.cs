using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.RequestDTO
{
    public class GetAllUserRequests
    {
        public Guid Id { get; set; }

        public DateOnly DateOfBooking { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public RequestStatus Status { get; set; }

        public Roles ownerRole { get; set; }


        public string ClassroomNumber { get; set; }
    }
}
