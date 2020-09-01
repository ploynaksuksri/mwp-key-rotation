using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Mwp.KeyRotation;
using Mwp.SharedResource;
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
            var secretName = "test-key-rotation";
            var storageAccount = await _storageClient.CreateAsync(DefaultResourceGroup, storageName, Region.AsiaSouthEast);
            var currentAccessKey = await _storageClient.GetStorageAccessKey(storageAccount);
            var secretObject = CreateSharedResourceObject(currentAccessKey, storageName, DefaultSubscriptionId,
                DefaultResourceGroup);
            var secret = await _secretClient.SetAsync(secretName, secretObject.ToString());

            // Act
            await KeyRotator.RotateKeyAsync(DefaultKeyVault, secretName, secret.Properties.Version);

            // Assert
            var newAccessKey = await _storageClient.GetStorageAccessKey(storageAccount);
            newAccessKey.ShouldNotBe(currentAccessKey);

            var updatedSecret = await _secretClient.GetAsync(secretName);
            var updatedSecretObject = JObject.Parse(updatedSecret.Value);
            ((string)updatedSecretObject[SharedResourceSecret.KeyProperty]).ShouldContain($"AccountKey={newAccessKey}");
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