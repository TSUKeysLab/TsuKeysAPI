using System.ComponentModel.DataAnnotations;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class KeyRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string KeyOwner { get; set; }
        public string KeyRecipient { get; set; }
        public string ClassroomNumber { get; set; }
        public KeyRequestStatus Status { get; set; }
    }
}
