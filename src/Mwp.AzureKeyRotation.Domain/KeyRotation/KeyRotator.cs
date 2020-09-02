using Microsoft.EntityFrameworkCore;
using Mwp.DbContext;
using Mwp.Secret;
using Mwp.SharedResource;
using Mwp.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using MwpKeyVaultClient = Mwp.KeyVault.KeyVaultClient;

namespace Mwp.KeyRotation
{
    public class KeyRotator
    {
        public static async Task RotateKeyAsync(string keyVaultName, string secretName)
        {
            var secretClient = new MwpKeyVaultClient(keyVaultName);
            var storageClient = new StorageClient(keyVaultName);

            var currentSecret = await secretClient.GetAsync(secretName);
            var secretValue = JObject.Parse(currentSecret.Value);
            var resourceGroupName = (string)secretValue[SharedResourceSecret.ResourceGroupProperty];
            var resourceName = (string)secretValue[SharedResourceSecret.ResourceNameProperty];

            var newAccessKey = await storageClient.RegenerateKey(resourceGroupName, resourceName);
            secretValue[SharedResourceSecret.KeyProperty] = newAccessKey.ConnectionString;

            var newSecret = await secretClient.SetAsync(secretName, secretValue.ToString());
            newSecret.Properties.ExpiresOn = DateTime.UtcNow.AddDays(SharedResourceSecret.SecretExpiresOnDays);
            await secretClient.UpdateExpiresOn(newSecret.Properties);

            var hostDbConnectionString = (await secretClient.GetAsync(SecretConsts.ConnectionStringsDefault)).Value;

            await UpdateKeyToDatabase(hostDbConnectionString, secretName, newAccessKey.ConnectionString);
        }

        public static async Task UpdateKeyToDatabase(string connectionString, string secretName, string keyToUpdate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AzureKeyRotationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            await using var dbContext = new AzureKeyRotationDbContext(optionsBuilder.Options);
            var sharedResource = await dbContext.SharedResources.FirstOrDefaultAsync(e => e.SecretName == secretName);
            var tenantResources = dbContext.TenantResources.Where(e => e.CloudServiceLocationId == sharedResource.CloudServiceLocationId
                                                                       && e.CloudServiceOptionId == sharedResource.CloudServiceOptionId);
            foreach (var resource in tenantResources)
            {
                resource.ConnectionString = keyToUpdate;
            }

            dbContext.TenantResources.UpdateRange(tenantResources);
            await dbContext.SaveChangesAsync();
        }
    }
}