using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiprogresstracker.Model.Tasks
{
    public class TaskTitle
    {
        public int Id { get; set; }
        public string? Task_title { get; set; }
        public DateTime Date_created { get; set; }
    }

    public class TaskContents
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public string? Task_name { get; set; }
        public DateTime? Date_created { get; set; }
        public int? Status { get; set; }
        public DateTime? Date_started { get; set; }
        public DateTime? Status_modified { get; set; }
    }
    public class TaskSubContents
    {
        public int Id { get; set; }
        public int? Contents_id { get; set; }
        public string? Subtask { get; set; }
        public DateTime? Date_created { get; set; }
        public int? Status { get; set; }
        public DateTime? Date_started { get; set; }
        public DateTime? Status_modified { get; set; }
    }
}