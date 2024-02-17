using System.ComponentModel.DataAnnotations;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class KeyRequest
    {
        public string KeyOwner { get; set; }
        public string KeyRecipient { get; set; }
        [Key]
        public string ClassroomNumber { get; set; }
        public DateTime EndOfBooking { get; set; }
    }
}
