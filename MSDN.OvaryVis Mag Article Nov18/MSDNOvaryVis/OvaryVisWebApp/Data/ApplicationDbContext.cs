using Microsoft.EntityFrameworkCore;
using OvaryVisWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OvaryVisWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<OvaryVis> OvaryVis { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                 : base(options)
        { }

    }
}
