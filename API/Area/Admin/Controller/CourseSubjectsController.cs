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
    public class CourseSubjectsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public CourseSubjectsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/CourseSubjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSubject>>> GetCourseSubject([FromQuery] string? name, string? status)
        {
            var query = _context.CourseSubject.AsQueryable();
            //if (!string.IsNullOrWhiteSpace(name))
            //{
            //    query = query.Where(r =>
            //        r.Name.ToLower().Contains(name.ToLower().Trim())
            //    );
            //}
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(cs => cs.Status==bool.Parse(status.ToLower().Trim()));
            }
            return await query.ToListAsync();
        }

        // GET: api/CourseSubjects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseSubject>> GetCourseSubject(int id)
        {
            var courseSubject = await _context.CourseSubject.FindAsync(id);

            if (courseSubject == null)
            {
                return NotFound();
            }

            return courseSubject;
        }

        // PUT: api/CourseSubjects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseSubject(int id, CourseSubject courseSubject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != courseSubject.CourseSubjectId)
            {
                return BadRequest();
            }

            _context.Entry(courseSubject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseSubjectExists(id))
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

        // POST: api/CourseSubjects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseSubject>> PostCourseSubject(CourseSubject courseSubject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.CourseSubject.Add(courseSubject);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourseSubject", new { id = courseSubject.CourseSubjectId }, courseSubject);
        }

        // DELETE: api/CourseSubjects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSubject(int id)
        {
            var courseSubject = await _context.CourseSubject.FindAsync(id);
            if (courseSubject == null)
            {
                return NotFound();
            }

            courseSubject.Status = false;
            _context.Entry(courseSubject).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseSubjectExists(int id)
        {
            return _context.CourseSubject.Any(e => e.CourseSubjectId == id);
        }
    }
}
