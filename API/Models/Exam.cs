using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Exam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public int RoomId { get; set; }
        public int AccountId { get; set; }
        public DateTime ExamDay { get; set; } 
        public TimeSpan ExamTime { get; set; }
        public bool Status { get; set; }
        public float Fee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}
