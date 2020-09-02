using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;

namespace Mwp.KeyVault
{
    public class KeyVaultClient
    {
        private readonly SecretClient _client;

        public KeyVaultClient(string keyVaultName)
        {
            var kvUri = GetKeyVaultUri(keyVaultName);
            _client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
        }

        public static string GetKeyVaultUri(string keyVaultName)
        {
            return $"https://{keyVaultName}.vault.azure.net/";
        }

        public string GetValue(string name)
        {
            KeyVaultSecret secret = _client.GetSecret(TransformName(name));
            return secret.Value;
        }

        public async Task<KeyVaultSecret> GetAsync(string name)
        {
            return await _client.GetSecretAsync(TransformName(name));
        }

        public async Task<KeyVaultSecret> SetAsync(string name, string value)
        {
            return await _client.SetSecretAsync(name, value);
        }

        public async Task UpdateProperties(SecretProperties properties)
        {
            await _client.UpdateSecretPropertiesAsync(properties);
        }

        #region private methods

        private static string TransformName(string name)
        {
            return name.Replace(":", "--");
        }

        #endregion private methods
    }
}