using tsuKeysAPIProject.DBContext.Models;

namespace tsuKeysAPIProject.DBContext.DTO.ScheduleDTO
{
    public class GetScheduleDTO
    {
        public Guid Id { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
