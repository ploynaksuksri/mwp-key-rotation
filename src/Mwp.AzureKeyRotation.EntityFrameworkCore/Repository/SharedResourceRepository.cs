using Mwp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Mwp.Repository
{
    public class SharedResourceRepository : BaseRepository<SharedResource>
    {
        public SharedResourceRepository(AzureKeyRotationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<SharedResource> GetBySecretName(string secretName)
        {
            return (await GetAsync(e => e.SecretName == secretName)).FirstOrDefault();
        }
    }
}