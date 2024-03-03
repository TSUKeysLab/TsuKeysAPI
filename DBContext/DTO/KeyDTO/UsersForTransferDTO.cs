using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class UsersForTransferDTO
    {
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public Roles Role { get; set; }

    }
}
