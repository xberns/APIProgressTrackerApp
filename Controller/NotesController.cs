
using apiprogresstracker.Model.Notes;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;
using Microsoft.EntityFrameworkCore;
using APIProgressTrackerApp.DTO.NotesDTO;
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

        [HttpPost("PostNote")]
        public async Task<ActionResult<Notes>> PostNote(Notes notes)
        {
            if (notes == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var note = await _context.Notes
                        .Where(x => x.User_id == notes.User_id&& x.Title_id == notes.Title_id && x.Id == notes.Id)
                        .Select(x => x.Notes_content).FirstOrDefaultAsync();
                if (note != null)
                {
                    return StatusCode(409, "Already exist.");
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
        
        [HttpGet("GetNote")]
        public async Task<ActionResult> GetNote([FromQuery]GetNotes notes)
        {
            if (notes == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var note = await _context.Notes
                            .Where(x => x.User_id == notes.User_id
                                     && x.Title_id == notes.Title_id 
                                     && x.Date_created == notes.Date_created)
                            .Select(x => x.Notes_content).FirstOrDefaultAsync();

                 if (note == null)
                {
                    return Ok(new { note = "0", Message = "If existing dats is not showing, please contact the admin." });
                }
          
                return Ok(note);
            }
            catch
            {
                return StatusCode(500, "An error occurred while fetching.");
            }
        }

        [HttpPut("UpdateNote")]
        public async Task<ActionResult> UpdateNote(Notes notes)
        {
            if (notes == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var note = await _context.Notes.Where(x => x.User_id == notes.User_id && x.Title_id == notes.Title_id && x.Id == notes.Id).FirstOrDefaultAsync();

                if (note == null)
                {
                    return StatusCode(404, "Does not exist");
                }

                note.Notes_content = notes.Notes_content;

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data updated successfully.",
                        data = notes.Notes_content
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
        
        [HttpDelete("DeleteNote")]
        public async Task<ActionResult> DeleteNote([FromQuery]GetNotes notes)
        {
            if (notes == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var note = await _context.Notes
                            .Where(x => x.User_id == notes.User_id && x.Title_id == notes.Title_id && x.Date_created == notes.Date_created).FirstOrDefaultAsync();

                if (note == null)
                {
                    return StatusCode(404, "Does not exist");
                }

               _context.Notes.Remove(note);

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data deleted successfully.",
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving.");

            }catch (DbUpdateConcurrencyException)
            {
                // Optional: handle concurrency issues
                return StatusCode(500, "A concurrency error occurred.");
            }
        }
    }
}