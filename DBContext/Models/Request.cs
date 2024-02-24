using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class Request
    {
        public Guid Id { get; set; }

        public int TimeId { get; set; }

        public DateOnly DateOfBooking { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
        
        public RequestStatus Status { get; set; }
        
        public string RequestOwner { get; set; }

        public Roles ownerRole { get; set; }

        public Guid OwnerId { get; set; }

        public DateTime DateOfSent { get; set; }

        public string ClassroomNumber { get; set; }


    }
}
