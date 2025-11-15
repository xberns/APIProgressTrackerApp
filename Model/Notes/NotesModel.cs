using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiprogresstracker.Model.Notes
{
    public class Notes
    {
        public int Id { get; set; }
        public int? Title_id { get; set; }
        public string? Notes_content { get; set; }
        public DateOnly? Date_created { get; set; }
        public string? User_id { get; set; }
    }
}