using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIProgressTrackerApp.DTO.NotesDTO
{
    public class GetNotes
    {   
        public int? Title_id { get; set; }
        public DateOnly? Date_created {get; set;}
        public string? User_id { get; set; }

    }
}