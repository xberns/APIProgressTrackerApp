
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
            var note = await _context.Notes.Where(x => x.Date_created == notes.Date_created).Select(x => x.Notes_content).ToListAsync();
            if (note.Count > 0)
            {
                return StatusCode(409, "Already exist.");
            }
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
                var note = await _context.Notes.Where(x => x.Date_created == formatdate).Select(x => x.Notes_content).ToListAsync();

                if (note == null)
                {
                    return NotFound();
                }

                return Ok(note);
            }
            catch
            {
                return StatusCode(500, "An error occurred while fetching.");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateNote(Notes note)
        {
            try
            {

                var notess = await _context.Notes.Where(x => x.Date_created == note.Date_created).FirstOrDefaultAsync();


                if (notess == null)
                {
                    return StatusCode(404, "Does not exist");
                }

                notess.Notes_content = note.Notes_content;

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data updated successfully.",
                        data = note.Notes_content
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving.");

            }
            catch (DbUpdateConcurrencyException)
            {
                // Optional: handle concurrency issues
                return StatusCode(500, "A concurrency error occurred.");
            }
        }
        
        [HttpDelete]
        public async Task<ActionResult> DeleteNote(DateTime date)
        {
            try
            {
                var formatdate = new DateOnly(date.Year, date.Month, date.Day);

                var notess = await _context.Notes.Where(x => x.Date_created == formatdate).FirstOrDefaultAsync();


                if (notess == null)
                {
                    return StatusCode(404, "Does not exist");
                }

               _context.Notes.Remove(notess);

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data deleted successfully.",
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving.");

            }
            catch (DbUpdateConcurrencyException)
            {
                // Optional: handle concurrency issues
                return StatusCode(500, "A concurrency error occurred.");
            }
        }



    }
}