using System.ComponentModel.DataAnnotations;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class KeyInfoDTO
    {
        public Guid OwnerId { get; set; }
        public DateTime? BookTime { get; set; }
        public string ClassroomNumber { get; set; }
    }
}
