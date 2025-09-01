using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;
using apiprogresstracker.Model.Tasks;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<TaskTitle>> SaveTitle(TaskTitle datas)
        {
            try

            {
                if (datas == null)
                {
                    return BadRequest("Parameter is null.");
                }

                await _context.TaskTitle.AddAsync(datas);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = datas
                    });
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTitle(string datas)
        {
            try
            {
                if (datas == null)
                {
                    return BadRequest();
                }
                var get = await _context.TaskTitle.Where(x => x.User == datas).ToListAsync();

                if (get.Count > 0)
                {
                    return Ok(get);
                }
                return StatusCode(500, "Error occured during transaction");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("SaveTaskContent")]
        public async Task<ActionResult<TaskTitle>> SaveTaskContent(TaskContents datas)
        {
            try

            {
                if (datas == null)
                {
                    return BadRequest("Parameter is null.");
                }

                await _context.TaskContents.AddAsync(datas);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = datas
                    });
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetTaskContent")]
        public async Task<IActionResult> GetTaskContent(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }
                var get = await _context.TaskContents.Where(x => x.Title_id == id).ToListAsync();

                if (get.Count > 0)
                {
                    return Ok(get);
                }
                return StatusCode(500, "Error occured during transaction");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("SaveTaskSubContent")]
        public async Task<ActionResult<TaskTitle>> SaveTaskSubContent(TaskContents datas)
        {
            try

            {
                if (datas == null)
                {
                    return BadRequest("Parameter is null.");
                }

                await _context.TaskContents.AddAsync(datas);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = datas
                    });
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetTaskSubContent")]
        public async Task<IActionResult> GetTaskSubContent(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }
                var get = await _context.TaskSubContents.Where(x => x.Contents_id == id).ToListAsync();

                if (get.Count > 0)
                {
                    return Ok(get);
                }
                return StatusCode(500, "Error occured during transaction");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        

    }
}