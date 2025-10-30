using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using apiprogresstracker.ApplicationDBContext;
using apiprogresstracker.Model.Tasks;
using APIProgressTrackerApp.DTO.TaskDTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;

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
        public async Task<IActionResult> GetTitle(GetTask datas)
        {
            try
            {
                if (datas == null)
                {
                    return BadRequest();
                }
                var get = await _context.TaskTitle.Where(x => x.User == datas.User).ToListAsync();

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
        public async Task<ActionResult<IEnumerable<TaskContents>>> SaveTaskContent(List<ModifyTaskContent> datas)
        {
            try
            {
                if (datas == null || datas.Count == 0)
                {
                    return BadRequest("Parameter is null or empty.");
                }

                var taskContentsList = new List<TaskContents>();

                foreach (var data in datas)
                {
                    var taskContent = new TaskContents
                    {
                        Title_id = data.Title_id,
                        Task_details = data.Task_details,
                        Date_created = data.Date_created,
                        Status = 0,
                        Status_modified = data.Date_created

                    };

                    taskContentsList.Add(taskContent);
                }

                await _context.TaskContents.AddRangeAsync(taskContentsList);
                
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = taskContentsList
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving the data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("AddNewTaskContent")]
        public async Task<ActionResult> AddNewTaskContent(ModifyTaskContent datas)
        {
            try
            {
                if (datas == null )
                {
                    return BadRequest("Parameter is null or empty.");
                }

             
                    var taskContent = new TaskContents
                    {
                        Title_id = datas.Title_id,
                        Task_order = datas.Task_order,
                        Task_details = datas.Task_details,
                        Date_created = datas.Date_created,
                        Status = 0,
                        Status_modified = datas.Date_created

                    };

                
                await _context.TaskContents.AddAsync(taskContent);
                
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = taskContent
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving the data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("ModTaskContent")]
        public async Task<ActionResult> ModTaskContent(ModifyTaskContent datas)
        {
            try
            {
                if (datas == null )
                {
                    return BadRequest("Parameter is null or empty.");
                }
                 var get = await _context.TaskContents.Where(x => x.Title_id == datas.Title_id && x.Id == datas.Id).FirstOrDefaultAsync();
               
                get.Task_details = datas.Task_details;
             
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully."
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving the data.");
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
                var get = await _context.TaskContents
                          .Where(x => x.Title_id == id)
                          .OrderBy(x => x.Task_order).ToListAsync();

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
        public async Task<ActionResult<TaskTitle>> SaveTaskSubContent(TaskContents data)
        {
            try

            {
                if (data == null)
                {
                    return BadRequest("Parameter is null.");
                }

                await _context.TaskContents.AddAsync(data);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully."
                    });
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

         [HttpPut("UpdateTasksOrder")]
        public async Task<ActionResult> UpdatTasksOrder([FromBody] List<UpdateOrder> data)
        {
            try
            {

                var get = await _context.TaskContents.Where(x => x.Title_id == data[0].Title_id).ToListAsync();
                var getCount = get.Count;
                if (data == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                else
                {
                    for (int o = 0; o < getCount; o++)
                    {
                        for (int i = 0; i < getCount; i++)
                        {
                            if (get[o].Id == data[i].Id)
                            {
                                get[o].Task_order = i;

                                break;
                            }
                            else
                            {
                                continue;
                            }

                        }
                    }

                }

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data updated successfully.",
                    });
                }
                 return StatusCode(500, "An unknown error occured while saving the data.");

            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "A concurrency error occurred.");
            }
        }

         [HttpPut("UpdateTaskContentStatus")]
        public async Task<ActionResult> UpdateTaskContentStatus(UpdateStatus data)
        {
            try
            {

                var get = await _context.TaskContents.Where(x => x.Title_id == data.Title_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (data == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                if (get.Date_started == get.Date_created)
                {
                    get.Date_started = data.Status_modified;
                }
                get.Status = data.Status;
                get.Status_modified = data.Status_modified;

                var saved = await _context.SaveChangesAsync();
                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data updated successfully.",
                    });
                }
                 return StatusCode(500, "An unknown error occured while saving the data.");

            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "A concurrency error occurred.");
            }
        }
    
    }
}