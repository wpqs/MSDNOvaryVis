using Microsoft.EntityFrameworkCore;

namespace OvaryVisFnApp
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<OvaryVis> OvaryVis { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                 : base(options)
        { }

    }
}
