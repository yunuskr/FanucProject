using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admin { get; set; }
        // Şimdilik DbSet<> tanımlamıyoruz
    }
}
