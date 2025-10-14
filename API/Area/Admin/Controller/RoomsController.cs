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
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public RoomsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms([FromQuery] string? name, string? status, string? roomId)
        {
            var query = _context.Rooms.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(r =>
                    r.Name.ToLower().Contains(name.ToLower().Trim())
                );
            }
            if (!string.IsNullOrWhiteSpace(roomId) && int.TryParse(roomId, out int roomIdValue))
            {
                query = query.Where(r => r.RoomId == roomIdValue);
            }
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower().Trim().Equals("true"))
            {
                query = query.Where(cs => cs.Status == bool.Parse(status.ToLower().Trim()));
            }
            return await query.ToListAsync();
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        // PUT: api/Rooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != room.RoomId)
            {
                return BadRequest();
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.RoomId }, room);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.Status = false;
            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
        [HttpGet("byRoom/{roomId}")]
        public async Task<ActionResult<object>> GetAccountsByRoom(int roomId)
        {
            // Lấy Room và các Account trong Room
            var room = await _context.Rooms
                .Include(r => r.Accounts) // load luôn Accounts
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.Status);

            if (room == null)
                return NotFound($"RoomId={roomId} không tồn tại hoặc không active.");

            // Lọc sinh viên active
            var students = room.Accounts
                .Where(a => a.Role == 0 && a.Status)
                .Select(a => new
                {
                    a.AccountId,
                    a.Name,
                    a.FullName,
                    a.Email,
                    a.Status
                })
                .ToList();

            return new
            {
                roomId = room.RoomId,
                roomName = room.Name,
                students = students
            };
        }

        [HttpGet("byAccount/{accountId}")]
        public async Task<ActionResult<object>> GetRoomByAccount(int accountId)
        {
            // Tìm Room mà account này thuộc vào
            var room = await _context.Rooms
                .Where(r => r.Accounts.Any(a => a.AccountId == accountId && a.Status))
                .Select(r => new
                {
                    r.RoomId,
                    r.Name,
                    r.Status,
                    Students = r.Accounts
                        .Where(a => a.Role == 0 && a.Status) // Chỉ lấy sinh viên active
                        .Select(a => new
                        {
                            a.AccountId,
                            a.Name,
                            a.FullName,
                            a.Email,
                            a.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            return room;
        }
    }
}
