using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIProgressTrackerApp.DTO.TaskDTO
{
    public class GetTaskTitle
    {
         public string? User_id { get; set; }
    }
   
    public class ModifyTaskTitle
    {
         public int Id { get; set; }
         public string? Task_title { get; set; }
         public DateTime? Date_created { get; set; }
         public string? User_id { get; set; }
    } 
    public class DeleteTaskTitle
    {
         public int Id { get; set; }
         public string? User_id { get; set; }
    }

    public class ModifyTaskContent
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public int? Task_order  { get; set; }
        public string? Task_details { get; set; }
        public DateTime? Date_created { get; set; }
         public string? User_id { get; set; }
    }
    public class DeleteTaskContent
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
         public string? User_id { get; set; }

    }
       public class ModifySubTask
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public int? Content_id { get; set; }
        public int? Subtask_order  { get; set; }
        public string? Subtask { get; set; }
        public DateTime? Date_created { get; set; }
        public string? User_id { get; set; }
    }
    public class DeleteSubTask
     {
        public int Id { get; set; }
        public int? Content_id { get; set; }
        public string? User_id { get; set; }

    }
    public class UpdateOrder
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public int? Task_order { get; set; }
        public string? Task_details { get; set; }
        public DateTime? Date_created { get; set; }
        public int? Status { get; set; }
        public DateTime? Date_started { get; set; }
        public DateTime? Status_modified { get; set; }
        public string? User_id { get; set; }
    }
  
    public class UpdateStatus
    {
        public int? Id {get; set;}
        public int? Title_id { get; set; }
        public int? Status { get; set; }
        public DateTime? Status_modified { get; set; }
        public string? User_id { get; set; }
    }
    public class GetTaskContent
    {
        
        public int? Title_id { get; set; }
        public DateTime? Date_created { get; set; }
        public string? User_id { get; set; }
    }

    public class AddSubContent
    {
        public int Id { get; set; }
        public int? TaskContents_id { get; set; }
        public string? Subtask { get; set; }
        public DateTime? Date_created { get; set; }
        public string? User_id { get; set; }
    }
    public class DeleteSubContent
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public string? User_id { get; set; }

    }
     public class UpdateSubtaskOrder
    {
        public int Id { get; set; }
        public int? Content_id { get; set; }
        public int? Subtask_order { get; set; } 
        public string? Subtask { get; set; }
        public string? User_id { get; set; }
    }
  
    public class UpdateSubtaskStatus
    {
        public int? Id {get; set;}
        public int? Content_id { get; set; }
        public int? Status { get; set; }
        public DateTime? Status_modified { get; set; }
        public string? User_id { get; set; }
    }

}