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
        
        [HttpGet("GetTaskTitle")]
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
        [HttpPost("SaveTaskTitle")]
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
                if (get.Count == 0)
                {
                    return Ok(new { task = "0", Message = "If existing dats is not showing, please contact the admin." });
                }
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
                if ( get.Task_details == datas.Task_details)
                {
                    return StatusCode(200, "No update necessary; data was identical");
                }
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

        [HttpDelete("DeleteTasksContent")]
        public async Task<IActionResult> DeleteTasksContent([FromBody] DeleteTaskContent data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try

            {
                if (data == null)
                {
                    return BadRequest("Parameter is null.");
                }
                var del = await _context.TaskContents.Where(x => x.Title_id == data.Title_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (del == null)
                {
                    return NotFound();
                }
                _context.TaskContents.Remove(del);
                var saved = await _context.SaveChangesAsync();
                if (saved <= 0)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to delete data.");
                }

                var get = await _context.TaskContents
                         .Where(x => x.Title_id == data.Title_id)
                         .OrderBy(x => x.Task_order).ToListAsync();
                var nonidentical = 0;

                for (int i = 0; i < get.Count; i++)
                {
                    if (get[i].Task_order != i)
                    {
                        nonidentical = 1;
                    }
                    get[i].Task_order = i;

                }
                if (nonidentical == 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Deleted successfully. No reorder needed"
                    });
                }
                var savedd = await _context.SaveChangesAsync();

                if (saved > 0 && savedd > 0)
                {
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Data deleted and reordered successfully."
                    });
                }
                await transaction.RollbackAsync();
                if (savedd <= 0)
                {
                    return StatusCode(500, "Failed to reorder data.");
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("UpdateTasksOrder")]
        public async Task<ActionResult> UpdateTasksOrder([FromBody] List<UpdateOrder> data)
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
        
       [HttpGet("GetSubTask")]
        public async Task<IActionResult> GetSubTask(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }
                var get = await _context.TaskSubContents
                          .Where(x => x.Content_id == id)
                          .OrderBy(x => x.Subtask_order).ToListAsync();
                if (get.Count == 0)
                {
                    return Ok(new { subtask = "0", Message = "If existing dats is not showing, please contact the admin." });
                }
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
        [HttpPost("AddNewSubTask")]
        public async Task<ActionResult> AddNewSubTask(ModifySubTask datas)
        {
            try
            {
                if (datas == null )
                {
                    return BadRequest("Parameter is null or empty.");
                }

             
                    var subtasks = new TaskSubContents
                    {
                        Content_id = datas.Content_id,
                        Subtask_order = datas.Subtask_order,
                        Subtask = datas.Subtask,
                        Date_created = datas.Date_created,
                        Status = 0,
                        Status_modified = datas.Date_created

                    };
                
                await _context.TaskSubContents.AddAsync(subtasks);
                
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = subtasks
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving the data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("ModSubTask")]
        public async Task<ActionResult> ModSubTask(ModifySubTask datas)
        {
            try
            {
                if (datas == null )
                {
                    return BadRequest("Parameter is null or empty.");
                }
                 var get = await _context.TaskSubContents.Where(x => x.Content_id == datas.Content_id && x.Id == datas.Id).FirstOrDefaultAsync();
                if ( get.Subtask == datas.Subtask)
                {
                    return StatusCode(200, "No update necessary; data was identical");
                }
                get.Subtask = datas.Subtask;
             
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully." + datas.Subtask
                    });
                }

                return StatusCode(500, "An unknown error occurred while saving the data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
         [HttpDelete("DeleteSubTask")]
        public async Task<IActionResult> DeleteSubTask([FromBody] DeleteSubTask data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try

            {
                if (data == null)
                {
                    return BadRequest("Parameter is null.");
                }
                 var del = await _context.TaskSubContents.Where(x => x.Content_id == data.Content_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (del == null)
                {
                    return NotFound();
                }
               _context.TaskSubContents.Remove(del);
                  var saved =  await _context.SaveChangesAsync();
                if (saved <= 0)
                {
                     await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to delete data.");
                }

                var get = await _context.TaskSubContents
                         .Where(x => x.Content_id == data.Content_id)
                         .OrderBy(x => x.Subtask_order).ToListAsync();
                var nonidentical = 0;

                for (int i = 0; i < get.Count; i++)
                {
                    if (get[i].Subtask_order != i)
                    {
                        nonidentical = 1;
                    }
                    get[i].Subtask_order = i;
                   
                }
                if (nonidentical == 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Deleted successfully. No reorder needed"
                    });
                }
                var savedd = await _context.SaveChangesAsync();

                if (saved > 0 && savedd > 0)
                {
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Data deleted and reordered successfully."
                    });
                }
                await transaction.RollbackAsync();
                 if (savedd <= 0)
                    {
                    return StatusCode(500, "Failed to reorder data.");
                    }
                    return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                 await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

         [HttpPut("UpdateSubTasksOrder")]
        public async Task<ActionResult> UpdateSubTasksOrder([FromBody] List<UpdateSubtaskOrder> data)
        {
            try
            {

                var get = await _context.TaskSubContents.Where(x => x.Content_id == data[0].Content_id).ToListAsync();
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
                                get[o].Subtask_order = i;

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

         [HttpPut("UpdateSubTaskStatus")]
        public async Task<ActionResult> UpdateSubTaskStatus(UpdateSubtaskStatus data)
        {
            try
            {

                var get = await _context.TaskSubContents.Where(x => x.Content_id == data.Content_id && x.Id == data.Id).FirstOrDefaultAsync();
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