using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class UsersWithoutKeysDTO
    {
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public Roles UserRole { get; set; }
        public Gender Gender { get; set; }
    }
}
