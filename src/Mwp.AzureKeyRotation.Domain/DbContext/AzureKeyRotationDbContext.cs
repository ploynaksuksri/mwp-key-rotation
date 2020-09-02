using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Mwp.DbContext
{
    /* This is your actual DbContext used on runtime.
     * It includes only your entities.
     * It does not include entities of the used modules, because each module has already
     * its own DbContext class. If you want to share some database tables with the used modules,
     * just create a structure like done for AppUser.
     *
     * Don't use this DbContext for database migrations since it does not contain tables of the
     * used modules (as explained above). See AzureKeyRotationMigrationsDbContext for migrations.
     */

    public class AzureKeyRotationDbContext : AbpDbContext<AzureKeyRotationDbContext>
    {
        public DbSet<SharedResource> SharedResources { get; set; }
        public DbSet<TenantResource> TenantResources { get; set; }

        /* Add DbSet properties for your Aggregate Roots / Entities here.
         * Also map them inside AzureKeyRotationDbContextModelCreatingExtensions.ConfigureAzureKeyRotation
         */

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