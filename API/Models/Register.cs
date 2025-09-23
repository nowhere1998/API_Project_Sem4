using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Register
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RegisterId { get; set; }
        public int AccountId { get; set; }
        public int ExamId { get; set; }
        public string? Email { get; set; }
        public bool Status { get; set; }
        public string payment {  get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
