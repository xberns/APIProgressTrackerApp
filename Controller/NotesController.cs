using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using apiprogresstracker.Model.Notes;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;

namespace apiprogresstracker.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Notes>> PostNotes(Notes notes)
        {
            await _context.Notes.AddAsync(notes);
            var saved = await _context.SaveChangesAsync();

            if (saved > 0)
            {
                return Ok();
            }
            return StatusCode(500, "An error occured while saving");
        }
    }
}