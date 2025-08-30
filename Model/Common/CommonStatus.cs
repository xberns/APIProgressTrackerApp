using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiprogresstracker.Model.Common
{
    public class CommonStatus
    {
        public int Id { get; set; }
        public string? Status_name { get; set; }
        public int? Status_id { get; set; }
    }
}