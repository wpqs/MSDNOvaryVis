using Microsoft.EntityFrameworkCore;
using OvaryVisWebApp.Models;

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
