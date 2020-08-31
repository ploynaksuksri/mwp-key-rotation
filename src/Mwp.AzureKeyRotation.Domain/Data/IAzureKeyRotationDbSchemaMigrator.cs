using System.Threading.Tasks;

namespace Mwp.AzureKeyRotation.Data
{
    public interface IAzureKeyRotationDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}