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
        public async Task<IActionResult> GetTaskTitle([FromQuery]GetTaskTitle data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            try
            {
                var get = await _context.TaskTitle.Where(x => x.User_id == data.User_id).ToListAsync();

                if (get.Count > 0)
                {
                    return Ok(get);
                }
                if (get.Count == 0)
                {
                    return Ok (new
                             {  
                                data = "0",
                                message = "If existing data is not showing, please contact the admin."
                            });
                            
                }
                return StatusCode(500, "Error occured during transaction");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("AddNewTaskTitle")]
        public async Task<ActionResult<TaskTitle>> AddNewTaskTitle(ModifyTaskTitle data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            try
            {
                 var taskTitleDetails = new TaskTitle
                    {
                        Task_title = data.Task_title,
                        Task_description = data.Task_description,
                        User_id = data.User_id,
                        Date_created = data.Date_created,
                    };
                    
                await _context.TaskTitle.AddAsync(taskTitleDetails);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    return Ok(new
                    {
                        message = "Data saved successfully.",
                        data = taskTitleDetails
                    });
                }
                return StatusCode(500, "An unknown error occured while saving the data.");
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

         [HttpPut("ModTaskTitle")]
        public async Task<ActionResult> ModTaskTitle(ModifyTaskTitle data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            try
            {
                 var get = await _context.TaskTitle.Where(x => x.User_id == data.User_id && x.Id == data.Id).FirstOrDefaultAsync();
                 if (get == null)
                {
                    return StatusCode(404);
                }
                if ( get.Task_title == data.Task_title && get.Task_description == data.Task_description)
                {
                    return StatusCode(200, "No update necessary; data was identical");
                }
                get.Task_title = data.Task_title;
                get.Task_description = data.Task_description;
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

        [HttpDelete("DeleteTasksTitle")]
        public async Task<IActionResult> DeleteTasksTitle([FromQuery] DeleteTaskTitle data)
        {
            if (data == null)
            {
                return BadRequest();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string msg = "";
                if (data == null)
                {
                    return BadRequest("Parameter is null.");
                }
                var del = await _context.TaskTitle.Where(x => x.User_id == data.User_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (del == null)
                {
                    return NotFound();
                }
                _context.TaskTitle.Remove(del);
                var saved = await _context.SaveChangesAsync();
                
                if (saved <= 0)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to delete data. Tasktitle");
                }

                var delTask = await _context.TaskContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Id).ToListAsync();
                var savedd = 1;
                if (delTask == null || delTask.Count == 0)
                {
                    msg = "";
                }
                else
                {
                     _context.TaskContents.RemoveRange(delTask);
                    savedd = await _context.SaveChangesAsync();
                    if (savedd <= 0)
                    {
                        msg += " including its task contents";
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Failed to delete data., task contents");
                    }
                }
               
                var delSub = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Id).ToListAsync();
                var saveddd = 1;
                if (delSub == null || delSub.Count == 0 )
                {
                    msg = "";
                } else
                {
                _context.TaskSubContents.RemoveRange(delSub);
                savedd = await _context.SaveChangesAsync();

                    msg += " and subtasks";
                    if (savedd <= 0)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Failed to delete subtask data. sibtasl");
                    }
                }

                if (saved > 0 && savedd > 0 && saveddd > 0)
                {
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Project deleted succesfully" + msg + "."
                    });
                }
                await transaction.RollbackAsync();
                if (saveddd <= 0)
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
       
         [HttpGet("GetTaskContent")]
        public async Task<IActionResult> GetTaskContent([FromQuery]GetTaskContent data)
        {
            if (data.User_id == null)
            {
                return BadRequest();
            }
            try
            {
                var get = await _context.TaskContents
                          .Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id &&x.Date_created <= data.Date_created)
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
        public async Task<ActionResult> AddNewTaskContent(ModifyTaskContent data)
        {
            if (data == null)
            {
                return BadRequest("Parameter is null or empty.");
            }
            try
            {
                var taskContent = new TaskContents
                {
                    Title_id = data.Title_id,
                    Task_order = data.Task_order,
                    Task_details = data.Task_details,
                    Date_created = data.Date_created,
                    Status = 0,
                    Status_modified = data.Date_created,
                    User_id = data.User_id
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
        public async Task<ActionResult> ModTaskContent(ModifyTaskContent data)
         {
            if (data == null)
            {
                return BadRequest("Parameter is null or empty.");
            }
            try
            {
                var get = await _context.TaskContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id && x.Id == data.Id).FirstOrDefaultAsync();
                if ( get.Task_details == data.Task_details)
                {
                    return StatusCode(200, "No update necessary; data was identical");
                }
                get.Task_details = data.Task_details;
             
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
            if (data == null)
            {
                return BadRequest();
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string msg;
                if (data == null)
                {
                    return BadRequest("Parameter is null.");
                }
                var del = await _context.TaskContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (del == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                _context.TaskContents.Remove(del);
                var saved = await _context.SaveChangesAsync();
                if (saved <= 0)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to delete data.");
                }
                var delSub = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Content_id == data.Id).ToListAsync();
                var savedd = 1;
                
                if (delSub == null || delSub.Count == 0 )
                {
                    msg = "";
                } else
                {
                _context.TaskSubContents.RemoveRange(delSub);
                savedd = await _context.SaveChangesAsync();

                    msg = " Subtask data deleted succesfully.";
                    if (savedd <= 0)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Failed to delete subtask data.");
                    }
                }

                var get = await _context.TaskContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id).OrderBy(x => x.Task_order).ToListAsync();

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
                        message = "Deleted successfully. No reorder needed" + msg
                    });
                }
                var saveddd = await _context.SaveChangesAsync();

                if (saved > 0 && savedd > 0 && saveddd > 0)
                {
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        message = "Data deleted and reordered successfully." + msg
                    });
                }
                await transaction.RollbackAsync();
                if (saveddd <= 0)
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
            if (data == null)
            {
                return BadRequest();
            }
            try
            {
                var get = await _context.TaskContents.Where(x => x.User_id == data[0].User_id && x.Title_id == data[0].Title_id).ToListAsync();
                var getCount = get.Count;
                if (get == null || getCount == 0)
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
            if (data == null)
            {
                return BadRequest();
            }
            try
            {
                var get = await _context.TaskContents.Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (get == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                if (get.Date_started == null)
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

        [HttpGet("GetAllSubTask")]
        public async Task<IActionResult> GetTaskandSubTask([FromQuery]GetTaskContent data)
        {
            try
            {
                if (data.Title_id < 0 || data.User_id == null)
                {
                    return BadRequest();
                }
                var get = await _context.TaskSubContents
                         .Where(x => x.User_id == data.User_id && x.Title_id == data.Title_id && x.Date_created <= x.Date_created)
                         .OrderBy(x => x.Content_id)
                         .ThenBy( x => x.Subtask_order)
                         .ToListAsync();
                          
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
        public async Task<ActionResult> AddNewSubTask(ModifySubTask data)
        {
            if (data == null )
            {
                return BadRequest("Parameter is null or empty.");
            }
            try
            {
                var subtasks = new TaskSubContents
                {
                    Title_id = data.Title_id,
                    Content_id = data.Content_id,
                    Subtask_order = data.Subtask_order,
                    Subtask = data.Subtask,
                    Date_created = data.Date_created,
                    Status = 0,
                    Status_modified = data.Date_created,
                    User_id = data.User_id
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
        public async Task<ActionResult> ModSubTask(ModifySubTask data)
        {
            if (data == null )
            {
                return BadRequest("Parameter is null or empty.");
            }
            try
            {
                var get = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Content_id == data.Content_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (get == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                if ( get.Subtask == data.Subtask)
                {
                    return StatusCode(200, "No update necessary; data was identical");
                }
                get.Subtask = data.Subtask;
             
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
        
        [HttpDelete("DeleteSubTask")]
        public async Task<IActionResult> DeleteSubTask([FromBody] DeleteSubTask data)
        {
            if (data == null)
            {
                return BadRequest("Parameter is null.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var del = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Content_id == data.Content_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (del == null)
                {
                    return StatusCode(404, "Does not exist");
                }
               _context.TaskSubContents.Remove(del);

                var saved =  await _context.SaveChangesAsync();

                if (saved <= 0)
                {
                     await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to delete data.");
                }

                var get = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Content_id == data.Content_id).OrderBy(x => x.Subtask_order).ToListAsync();

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
            if (data == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var get = await _context.TaskSubContents.Where(x => x.User_id == data[0].User_id && x.Content_id == data[0].Content_id).ToListAsync();
                var getCount = get.Count;
                if (get == null)
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
            if (data == null)
            {
                return BadRequest("Parameter is null.");
            }
            try
            {
                var get = await _context.TaskSubContents.Where(x => x.User_id == data.User_id && x.Content_id == data.Content_id && x.Id == data.Id).FirstOrDefaultAsync();
                if (get == null)
                {
                    return StatusCode(404, "Does not exist");
                }
                if (get.Date_started == null)
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