using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apiprogresstracker.ApplicationDBContext;
using APIProgressTrackerApp.DTO.TaskDTO;
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
        public async Task<IActionResult> GetProgress([FromQuery] GetTaskContent data)
        {
            try{
                var task =  await _context.TaskContents
                            .GroupBy(x => x.Title_id == data.Title_id)
                            .Select(o => new
                            {
                                taskTotal = o.Count(),
                                taskNew = o.Count(x => x.Status == 0),
                                taskInProgress = o.Count(x => x.Status == 1),
                                taskCompleted = o.Count(x => x.Status == 2),
                                taskOnHold = o.Count(x => x.Status == 3),
                             }).FirstOrDefaultAsync();

            if (task == null)
            {
                 return Ok(new { data = "0", Message = "If existing dats is not showing, please contact the admin." });
            }

             var subtask = await _context.TaskSubContents
                            .GroupBy(x => x.Title_id == data.Title_id)
                            .Select(o => new
                            {
                                subtaskTotal = o.Count(),
                                subtaskNew = o.Count(x => x.Status == 0),
                                subtaskInProgress = o.Count(x => x.Status == 1),
                                subtaskCompleted = o.Count(x => x.Status == 2),
                                subtaskOnHold = o.Count(x => x.Status == 3),
                             }).FirstOrDefaultAsync();
          
            if (subtask == null)
            {
                return Ok( new {task, Message = "Loaded succesfully. No subtask"});
            }

            var total = new
            {
                taskTotal = task.taskTotal + subtask.subtaskTotal,
                taskNew = task.taskNew + subtask.subtaskNew,
                taskInProgress = task.taskInProgress + subtask.subtaskInProgress,
                taskCompleted = task.taskCompleted + subtask.subtaskCompleted,
                taskOnHold = task.taskOnHold + subtask.subtaskOnHold
            };
            return Ok(total);
            }
            catch
            {
                return StatusCode(500, "An error occurred while fetching.");
            }
        }
    }
}