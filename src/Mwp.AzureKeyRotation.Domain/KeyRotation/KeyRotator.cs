using Microsoft.Extensions.Logging;
using Mwp.Constants;
using Mwp.Repository;
using Mwp.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using MwpKeyVaultClient = Mwp.KeyVault.KeyVaultClient;

namespace Mwp.KeyRotation
{
    public class KeyRotator
    {
        private readonly ILogger _logger;
        private readonly MwpKeyVaultClient _secretClient;
        private readonly StorageClient _storageClient;

        public KeyRotator(ILogger logger, string keyVaultName) :
            this(keyVaultName)
        {
            _logger = logger;
        }

        public KeyRotator(string keyVaultName)
        {
            _secretClient = new MwpKeyVaultClient(keyVaultName);
            _storageClient = new StorageClient(keyVaultName);
        }

        public async Task<string> RotateKeyAsync(string secretName)
        {
            var currentSecret = await _secretClient.GetAsync(secretName);
            var secretValue = JObject.Parse(currentSecret.Value);
            var resourceGroupName = (string)secretValue[SharedResourceSecret.ResourceGroupProperty];
            var resourceName = (string)secretValue[SharedResourceSecret.ResourceNameProperty];

            _logger.LogInformation($"Regenerating access key for {resourceName} in resource group {resourceGroupName}");
            var newAccessKey = await _storageClient.RegenerateKey(resourceGroupName, resourceName);
            secretValue[SharedResourceSecret.KeyProperty] = newAccessKey.ConnectionString;

            _logger.LogInformation($"Updating new access key to secret: {secretName}");
            var newSecret = await _secretClient.SetAsync(secretName, secretValue.ToString());
            newSecret.Properties.ExpiresOn = DateTime.UtcNow.AddDays(SharedResourceSecret.ValidityPeriodDays);
            await _secretClient.UpdateProperties(newSecret.Properties);
            return newAccessKey.ConnectionString;
        }

        public async Task UpdateKeyToDatabase(string hostDbConnectionString, string secretName, string keyToUpdate)
        {
            _logger.LogInformation($"Updating connection string to database for secret: {secretName}");
            try
            {
                await using var dbContext = AzureKeyRotationDbContextFactory.Create(hostDbConnectionString);
                var sharedResource = await new SharedResourceRepository(dbContext).GetBySecretName(secretName);
                await new TenantResourceRepository(dbContext).UpdateConnectionString(
                    sharedResource.CloudServiceLocationId,
                    sharedResource.CloudServiceOptionId, keyToUpdate);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Failed to update connection string to database for secret: {secretName}");
            }
        }

        public async Task<string> GetHostDbConnectionString()
        {
            return (await _secretClient.GetAsync(SecretConsts.ConnectionStringsDefault)).Value;
        }
    }
}