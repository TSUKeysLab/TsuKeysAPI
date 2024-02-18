using System.ComponentModel.DataAnnotations;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class KeyRequest
    {
        public string KeyOwner { get; set; }
        public string KeyRecipient { get; set; }
        [Key]
        public string ClassroomNumber { get; set; }
        public RequestStatus Status { get; set; }
    }
}
