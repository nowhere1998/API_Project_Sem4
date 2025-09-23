using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class AccountExam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountExamId { get; set; }
        public int SubjectId { get; set; }
        public int ExamId { get; set; }
        public int AccountId { get; set; }
        public float Score { get; set; }
        public int Record {  get; set; }

        public bool IsPass { get; set; }
        public bool Status { get; set; }
    }
}
