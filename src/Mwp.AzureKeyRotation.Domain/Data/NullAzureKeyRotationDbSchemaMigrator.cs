using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Mwp.AzureKeyRotation.Data
{
    /* This is used if database provider does't define
     * IAzureKeyRotationDbSchemaMigrator implementation.
     */
    public class NullAzureKeyRotationDbSchemaMigrator : IAzureKeyRotationDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}