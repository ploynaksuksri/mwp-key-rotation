using Microsoft.EntityFrameworkCore;

namespace Mwp
{
    public class AzureKeyRotationDbContextFactory
    {
        public static AzureKeyRotationDbContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AzureKeyRotationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new AzureKeyRotationDbContext(optionsBuilder.Options);
        }
    }
}