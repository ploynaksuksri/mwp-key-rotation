using Mwp.SharedResource;
using Mwp.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using MwpKeyVaultClient = Mwp.KeyVault.KeyVaultClient;

namespace Mwp.KeyRotation
{
    public class KeyRotator
    {
        public static async Task RotateKeyAsync(string keyVaultName, string secretName, string secretVersion)
        {
            var secretClient = new MwpKeyVaultClient(keyVaultName);
            var storageClient = new StorageClient(keyVaultName);

            var currentSecret = await secretClient.GetAsync(secretName);
            var secretValue = JObject.Parse(currentSecret.Value);
            var resourceGroupName = (string)secretValue[SharedResourceSecret.ResourceGroupProperty];
            var resourceName = (string)secretValue[SharedResourceSecret.ResourceNameProperty];

            var newAccessKey = await storageClient.RegenerateKey(resourceGroupName, resourceName);
            secretValue[SharedResourceSecret.KeyProperty] = newAccessKey.ToString();

            var newSecret = await secretClient.SetAsync(secretName, secretValue.ToString());
            newSecret.Properties.ExpiresOn = CalculateNextExpiresOn();
            await secretClient.UpdateExpiresOn(newSecret.Properties);
        }

        #region private methods

        private static DateTimeOffset CalculateNextExpiresOn()
        {
            return DateTime.UtcNow.AddDays(SharedResourceSecret.SecretExpiresOnDays);
        }

        #endregion private methods
    }
}