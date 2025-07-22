using FanucRelease.Models;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<KaynakDongusu> KaynakDonguleri { get; set; }
        public DbSet<MakineDurus> MakineDuruslari { get; set; }
        public DbSet<TrafoBilgisi> TrafoBilgileri { get; set; }
        public DbSet<KaynakParametre> KaynakParametreleri { get; set; }
    }
}
