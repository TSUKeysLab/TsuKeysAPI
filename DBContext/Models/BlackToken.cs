using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace tsuKeysAPIProject.DBContext.Models
{
    public class BlackToken
    {
        public Guid Id { get; set; }
        public string Blacktoken { get; set; }
    }
}
