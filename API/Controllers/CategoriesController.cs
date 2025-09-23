//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using API.Models;

//namespace API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CategoriesController : ControllerBase
//    {
//        private readonly FlutterContext _context;

//        public CategoriesController(FlutterContext context)
//        {
//            _context = context;
//        }

//        // GET: api/Categories
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(string name = "", int currentPage = 1)
//        {
//            int pageSize = 6;
//            if (string.IsNullOrWhiteSpace(name))
//            {
//                return await _context.Categories
//                    .OrderByDescending(c => c.CategoryId)
//                    .Skip((currentPage - 1) * pageSize)
//                    .Take(pageSize)
//                    .ToListAsync();
//            }
//            return await _context.Categories
//                    .OrderByDescending(c => c.CategoryId)
//                    .Where(c => c.CategoryName.ToLower().Contains(name.Trim().ToLower()))
//                    .Skip((currentPage - 1) * pageSize)
//                    .Take(pageSize)
//                    .ToListAsync();
//        }

//        [HttpGet("getAll")]
//        public async Task<ActionResult<IEnumerable<Category>>> GetAll(string name = "")
//        {
//            if (!string.IsNullOrWhiteSpace(name))
//            {
//                return await _context.Categories
//                    .Where(c => c.CategoryName.ToLower().Contains(name.Trim().ToLower()))
//                    .ToListAsync();
//            }
//            return await _context.Categories.ToListAsync();
//        }

//        // GET: api/Categories/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<Category>> GetCategory(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);

//            if (category == null)
//            {
//                return Ok("Warning: No item!");
//            }

//            return category;
//        }

//        // PUT: api/Categories/5
//        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutCategory(int id, Category category)
//        {
//            if (id != category.CategoryId)
//            {
//                return BadRequest();
//            }

//            var checkUniqueName = _context.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower() && c.CategoryId != category.CategoryId);
//            if (checkUniqueName != null)
//            {
//                return Ok("Warning: Category name is already exist!");
//            }
//            category.CategoryName = category.CategoryName.Trim();
//            _context.Entry(category).State = EntityState.Modified;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!CategoryExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            return Ok("Update Category success!");
//        }

//        // POST: api/Categories
//        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//        [HttpPost]
//        public async Task<ActionResult<Category>> PostCategory(Category category)
//        {
//            if (_context.Categories == null)
//            {
//                return Ok("Warning: Entity set 'RESTINAContext.Categories'  is null.");
//            }
//            if (string.IsNullOrWhiteSpace(category.CategoryName))
//            {
//                return Ok("Warning: Category name can not empty!");
//            }
//            if (_context.Categories.FirstOrDefault(c => c.CategoryName.ToLower().Equals(category.CategoryName.ToLower())) != null)
//            {
//                return Ok("Warning: Category name is already exist!");
//            }
//            category.CategoryName = category.CategoryName.Trim();
//            _context.Categories.Add(category);
//            await _context.SaveChangesAsync();

//            return Ok("Create new category success!");
//        }

//        // DELETE: api/Categories/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteCategory(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);
//            if (category == null)
//            {
//                return NotFound();
//            }

//            _context.Categories.Remove(category);
//            await _context.SaveChangesAsync();

//            return Ok("Delete category success!");
//        }

//        private bool CategoryExists(int id)
//        {
//            return _context.Categories.Any(e => e.CategoryId == id);
//        }
//    }
//}
