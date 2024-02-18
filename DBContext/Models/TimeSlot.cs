namespace tsuKeysAPIProject.DBContext.Models
{
    public class TimeSlot
    {
        public Guid Id { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
