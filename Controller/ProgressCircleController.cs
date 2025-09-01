using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiprogresstracker.ApplicationDBContext;
namespace APIProgressTrackerApp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressCircleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgressCircleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProgress(int id)
        {
            var totalTask = await _context.TaskContents.Where(x => x.Id == id).ToListAsync();

            return StatusCode(200);
        }
    }
}