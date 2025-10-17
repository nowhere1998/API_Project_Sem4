using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Area.Admin.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseStudentsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public CourseStudentsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/CourseStudents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseStudentDTO>>> GetCourseStudent([FromQuery] string? name, string? status)
        {
            var query = _context.CourseStudent.AsQueryable();
            //if (!string.IsNullOrWhiteSpace(name))
            //{
            //    query = query.Where(r =>
            //        r.Name.ToLower().Contains(name.ToLower().Trim())
            //    );
            //}
            var result = query
              .Include(cs => cs.Course)
              .Include(cs => cs.Student)
              .Select(cs => new CourseStudentDTO
              {
                  CourseStudentId = cs.CourseStudentId,
                  StudentId = cs.StudentId,
                  StudentName = cs.Student.Name,
                  CourseId = cs.CourseId,
                  CourseName = cs.Course.Name,
                  Status = cs.Status,
              });
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower().Trim().Equals("true"))
            {
                result = result.Where(cs => cs.Status == bool.Parse(status.ToLower().Trim()));
            }
            return await result.ToListAsync();
        }

        // GET: api/CourseStudents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseStudent>> GetCourseStudent(int id)
        {
            var courseStudent = await _context.CourseStudent.FindAsync(id);

            if (courseStudent == null)
            {
                return NotFound();
            }

            return courseStudent;
        }

        // PUT: api/CourseStudents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseStudent(int id, CourseStudent courseStudent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != courseStudent.CourseStudentId)
            {
                return BadRequest();
            }

            _context.Entry(courseStudent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseStudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CourseStudents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseStudent>> PostCourseStudent(CourseStudent courseStudent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.CourseStudent.Add(courseStudent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourseStudent", new { id = courseStudent.CourseStudentId }, courseStudent);
        }

        // DELETE: api/CourseStudents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseStudent(int id)
        {
            var courseStudent = await _context.CourseStudent.FindAsync(id);
            if (courseStudent == null)
            {
                return NotFound();
            }

            courseStudent.Status = false;
            _context.Entry(courseStudent).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseStudentExists(int id)
        {
            return _context.CourseStudent.Any(e => e.CourseStudentId == id);
        }
    }
}
