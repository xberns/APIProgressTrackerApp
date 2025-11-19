using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace apiprogresstracker.Model.Tasks
{
    public class TaskTitle
    {
        public int Id { get; set; }
        public string? Task_title { get; set; }
        public string? Task_description { get; set; }
        public DateTime? Date_created { get; set; }
        public string? User_id { get; set; }

        public ICollection<TaskContents>? TaskContents { get; set; }
    }

    public class TaskContents
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
        public TaskTitle? TaskTitle { get; set; }
        public ICollection<TaskSubContents>? TaskSubContents { get; set; }
    }
    public class TaskSubContents
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public int? Content_id { get; set; }
        public int? Subtask_order { get; set; }
        public string? Subtask { get; set; }
        public DateTime? Date_created { get; set; }
        public int? Status { get; set; }
        public DateTime? Date_started { get; set; }
        public DateTime? Status_modified { get; set; }
        public string? User_id { get; set; }
        public TaskContents? TaskContents { get; set; }
    }
}