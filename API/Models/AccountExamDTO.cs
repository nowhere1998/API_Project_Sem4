﻿namespace API.Models
{
    public class AccountExamDto
    {
        public int AccountExamId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int CourseSubjectId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Subject { get; set; }
        public int SubjectId { get; set; }
        public int CourseId { get; set; }
        public string? Course { get; set; }
        public float Score { get; set; }
        public bool IsPass { get; set; }
        public int Record { get; set; }
        public string RoomName { get; set; } = string.Empty;

        public bool Status { get; set; }
    }
}