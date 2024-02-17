using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class Key
    {
        [Key]
        public String ClassroomNumber { get; set; }
        public String Owner { get; set; }
    }
}
