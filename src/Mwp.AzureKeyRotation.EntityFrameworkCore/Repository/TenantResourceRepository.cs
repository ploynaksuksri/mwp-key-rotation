using Mwp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Mwp.Repository
{
    public class TenantResourceRepository : BaseRepository<TenantResource>
    {
        public TenantResourceRepository(AzureKeyRotationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpdateConnectionString(int locationId, int optionId, string connectionString)
        {
            var resources = await GetAsync(e => e.CloudServiceLocationId == locationId && e.CloudServiceOptionId == optionId);
            resources.ToList().ForEach(e => e.ConnectionString = connectionString);
            await UpdateRange(resources, autoSave: true);
        }
    }
}