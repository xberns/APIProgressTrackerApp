using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;
using apiprogresstracker.Model.Tasks;

namespace apiprogresstracker.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TaskTitle>> SaveTitle(TaskTitle title)
        {
            await _context.TaskTitle.AddAsync(title);
            var saved = await _context.SaveChangesAsync();

            if (saved > 0)
            {
                return Ok();
            }
            return StatusCode (500);
        }
    }
}