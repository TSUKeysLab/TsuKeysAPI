namespace tsuKeysAPIProject.DBContext.DTO.KeyDTO
{
    public class KeyRequestDTO
    {
        public Guid OwnerId {  get; set; }
        public string KeyRecipient { get; set; }
        public string ClassroomNumber { get; set; }
    }
}
