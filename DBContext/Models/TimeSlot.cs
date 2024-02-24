using System.ComponentModel.DataAnnotations;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class TimeSlot
    {
        [Key]
        public int SlotNumber { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
