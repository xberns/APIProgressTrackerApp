
using apiprogresstracker.Model.Notes;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                if (notes == null)
                {
                    return BadRequest("Parameter is null.");
                }

                await _context.Notes.AddAsync(notes);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = notes
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception: _logger.LogError(ex, "Error saving note");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetNote(DateTime date)
        {
            var formatdate = new DateOnly(date.Year, date.Month, date.Day);
            try
            {
                var note = await _context.Notes.Where(x => x.Date_Created == formatdate).Select(x => x.Notes_content).ToListAsync();

                if (note == null){
                    return NotFound();
                }
                    
                return Ok(note);
            }
            catch
            {
                return StatusCode(500, "An error occurred while fetching.");
            }
        }


    }
}