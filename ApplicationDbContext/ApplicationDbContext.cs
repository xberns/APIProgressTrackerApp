using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apiprogresstracker.Model.Notes;
using apiprogresstracker.Model.Calendar;
using apiprogresstracker.Model.Common;
using apiprogresstracker.Model.Tasks;
using Microsoft.EntityFrameworkCore;


namespace apiprogresstracker.ApplicationDBContext
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notes> Notes { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<TaskTitle> TaskTitle { get; set; }
        public DbSet<TaskContents> TaskContents { get; set; }
        public DbSet<TaskSubContents> TaskSubContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        
    }

}