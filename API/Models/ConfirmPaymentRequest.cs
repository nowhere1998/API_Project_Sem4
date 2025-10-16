namespace API.Models
{
    public class ConfirmPaymentRequest
    {
        public int StudentId { get; set; }    // thêm
        public int ExamId { get; set; }
        public string method { get; set; } = "";
        public bool success { get; set; }
    }
}