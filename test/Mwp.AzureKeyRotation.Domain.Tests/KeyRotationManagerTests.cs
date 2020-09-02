using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Mwp.Constants;
using Mwp.KeyRotation;
using Mwp.Storage;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using MwpKeyVaultClient = Mwp.KeyVault.KeyVaultClient;

namespace Mwp.AzureKeyRotation
{
    public class KeyRotationManagerTests
    {
        public const string DefaultResourceGroup = "mwp-local-app";
        public const string DefaultSubscriptionId = "29c044a6-2a71-48fb-9cb0-b4fdbe7c878a";
        public const string DefaultKeyVault = "mwp-kv-local";
        public const string DefaultTestSecretName = "test-key-rotation";
        public const string MwpPrefix = "mwp";

        private readonly MwpKeyVaultClient _secretClient;
        private readonly StorageClient _storageClient;

        public KeyRotationManagerTests()
        {
            _secretClient = new MwpKeyVaultClient(DefaultKeyVault);
            _storageClient = new StorageClient(DefaultKeyVault);
        }

        [Fact]
        public async Task Should_Rotate_AccessKey_Storage()
        {
            var storageName = MwpPrefix + "rotation" + new Random().Next(100000, 300000);
            var storageAccount = await _storageClient.CreateAsync(DefaultResourceGroup, storageName, Region.AsiaSouthEast);
            var currentAccessKey = await _storageClient.GetStorageAccessKey(storageAccount);
            var secretObject = CreateSharedResourceObject(currentAccessKey, storageName, DefaultSubscriptionId, DefaultResourceGroup);
            await _secretClient.SetAsync(DefaultTestSecretName, secretObject.ToString());

            // Act
            var newConnectionString = await new KeyRotator(DefaultKeyVault).RotateKeyAsync(DefaultTestSecretName);

            // Assert - Access key is rotated
            var newAccessKey = await _storageClient.GetStorageAccessKey(storageAccount);
            newAccessKey.ShouldNotBe(currentAccessKey);

            // Assert - New Access key is updated to Key Vault with new expiration date
            var updatedSecret = await _secretClient.GetAsync(DefaultTestSecretName);
            var updatedSecretObject = JObject.Parse(updatedSecret.Value);
            ((string)updatedSecretObject[SharedResourceSecret.KeyProperty]).ShouldBe(newConnectionString);
            ((string)updatedSecretObject[SharedResourceSecret.ResourceNameProperty]).ShouldBe(storageName);
            updatedSecret.Properties.ExpiresOn?.ShouldBeGreaterThan(DateTimeOffset.UtcNow);

            // Remove storage
            await _storageClient.DeleteAsync(DefaultResourceGroup, storageName);
        }

        private JObject CreateSharedResourceObject(
            string key,
            string resourceName,
            string subscriptionId,
            string resourceGroupName)
        {
            JObject secretValue = new JObject(
                new JProperty(SharedResourceSecret.KeyProperty, key),
                new JProperty(SharedResourceSecret.ResourceNameProperty, resourceName),
                new JProperty(SharedResourceSecret.SubscriptionIdProperty, subscriptionId),
                new JProperty(SharedResourceSecret.ResourceGroupProperty, resourceGroupName));
            return secretValue;
        }
    }
}