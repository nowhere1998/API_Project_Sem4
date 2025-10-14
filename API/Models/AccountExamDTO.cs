namespace API.Models
{
    public class AccountExamDto
    {
        public int AccountExamId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public float Score { get; set; }
        public bool IsPass { get; set; }
        public int Record { get; set; }
        public string RoomName { get; set; } = string.Empty;

    }
}
