using Mwp.Secret;
using Mwp.SharedResource;
using Mwp.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
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
            newSecret.Properties.ExpiresOn = CalculateNextExpiresOn();
            await secretClient.UpdateExpiresOn(newSecret.Properties);

            var hostDBConnectionString = (await secretClient.GetAsync(SecretConsts.ConnectionStringsDefault)).Value;
            await UpdateKeyToDatabase(hostDBConnectionString, secretName, newAccessKey.ConnectionString);
        }

        public static async Task UpdateKeyToDatabase(string connectionString, string secretName, string keyToUpdate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var updateQuery = @$"Update mwp.TenantResources set ConnectionString = '{keyToUpdate}'
                              FROM mwp.TenantResources tr INNER JOIN mwp.SharedResources sr
                              ON tr.CloudServiceLocationId = sr.CloudServiceLocationId
                              AND tr.CloudServiceOptionId = sr.CloudServiceOptionId
                              WHERE sr.SecretName = '{secretName}'";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    var rows = await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #region private methods

        private static DateTimeOffset CalculateNextExpiresOn()
        {
            return DateTime.UtcNow.AddDays(SharedResourceSecret.SecretExpiresOnDays);
        }

        #endregion private methods
    }
}