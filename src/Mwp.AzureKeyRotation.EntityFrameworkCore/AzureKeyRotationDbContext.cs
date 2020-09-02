using Microsoft.EntityFrameworkCore;
using Mwp.Models;

namespace Mwp
{
    public class AzureKeyRotationDbContext : DbContext
    {
        public DbSet<SharedResource> SharedResources { get; set; }
        public DbSet<TenantResource> TenantResources { get; set; }

        public AzureKeyRotationDbContext(DbContextOptions<AzureKeyRotationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("mwp");
        }
    }
}