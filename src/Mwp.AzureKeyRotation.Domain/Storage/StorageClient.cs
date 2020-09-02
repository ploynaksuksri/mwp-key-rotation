using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Mwp.Constants;
using Mwp.KeyVault;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mwp.Storage
{
    public class StorageClient
    {
        protected IAzure Azure;

        public StorageClient(string keyVaultName)
        {
            var secretClient = new KeyVaultClient(keyVaultName);

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                secretClient.GetValue(SecretConsts.AzureClientId),
                secretClient.GetValue(SecretConsts.AzureClientSecret),
                secretClient.GetValue(SecretConsts.AzureTenantId),
                AzureEnvironment.AzureGlobalCloud);

            Azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();
        }

        public async Task<StorageAccessKey> RegenerateKey(string resourceGroupName, string accountName, string keyName = SharedResourceSecret.StorageKey1Name)
        {
            var storageAccount = await Azure.StorageAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);
            var regenratedKey = await storageAccount.RegenerateKeyAsync(keyName);
            if (regenratedKey == null)
                throw new Exception($"Failed to regenerate key for account {accountName}");

            return new StorageAccessKey()
            {
                AccountName = accountName,
                AccountKey = regenratedKey.FirstOrDefault(e => e.KeyName == SharedResourceSecret.StorageKey1Name)?.Value
            };
        }

        public async Task<IStorageAccount> CreateAsync(string resourceGroupName, string accountName, Region region)
        {
            var sdkRegion = region ?? Region.AsiaSouthEast;
            var storage = await Azure.StorageAccounts.Define(accountName)
                .WithRegion(sdkRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAccessFromAzureServices()
                .WithSku(StorageAccountSkuType.Standard_GRS)
                .WithTag("createdBy", "SDK")
                .CreateAsync();
            return storage;
        }

        public async Task<string> GetStorageAccessKey(IStorageAccount storage)
        {
            var keys = await storage.GetKeysAsync();

            var key1 = keys.FirstOrDefault(e => e.KeyName == "key1");
            return key1.Value;
        }

        public async Task DeleteAsync(string resourceGroupName, string accountName)
        {
            await Azure.StorageAccounts.DeleteByResourceGroupAsync(resourceGroupName, accountName);
        }
    }
}