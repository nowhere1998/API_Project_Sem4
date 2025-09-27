namespace API.Models
{
    public class ExamDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }

        public int AccountId { get; set; }
        public string AccountName { get; set; }

        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public int RoomId { get; set; }
        public string RoomName { get; set; }
    }
}
