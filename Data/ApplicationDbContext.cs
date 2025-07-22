using Microsoft.EntityFrameworkCore;

namespace FanucProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Şimdilik DbSet<> tanımlamıyoruz
    }
}
