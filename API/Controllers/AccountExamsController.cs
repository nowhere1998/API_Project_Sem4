using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountExamsController : ControllerBase
	{
		private readonly FlutterContext _context;

		public AccountExamsController(FlutterContext context)
		{
			_context = context;
		}
        [HttpGet("byStudent/{studentId}")]
        public async Task<ActionResult> GetAccountExamsByStudent(int studentId)
        {
            var result = await _context.AccountExams
                .Where(ae => ae.StudentId == studentId && ae.Status)
                .Include(ae => ae.Exam)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Select(ae => new {
                    ae.AccountExamId,
                    ae.ExamId,
                    ExamName = ae.Exam.Name,
                    ae.StudentId,
                    StudentName = ae.Student.Name,
                    ae.Score,
                    ae.Record,
                    ae.IsPass,
                    ae.Status,
                    RoomName = ae.Student.Room.Name,
                    CourseName = ae.CourseSubject.Course.Name,
                    SubjectName = ae.CourseSubject.Subject.Name
                })
                .ToListAsync();

            if (!result.Any())
                return NotFound($"Không tìm thấy điểm thi cho sinh viên ID: {studentId}");

            return Ok(result);
        }






    }
}
