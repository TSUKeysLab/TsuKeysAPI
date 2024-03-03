using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class UserKeysDTO
    {
        public string ClassroomNumber { get; set; }
        public TimeOnly TimeToEndUsage { get; set; }
    }
}
