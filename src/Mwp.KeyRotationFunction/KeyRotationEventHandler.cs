// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Mwp.KeyRotation;
using Newtonsoft.Json.Linq;

namespace Mwp
{
    public static class KeyRotationEventHandler
    {
        [FunctionName("KeyRotationEventHandler")]
        public static async void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            var objectInput = JObject.Parse(eventGridEvent.Data.ToString());
            var keyVaultName = (string)objectInput["VaultName"];
            var secretName = (string)objectInput["ObjectName"];

            log.LogInformation($"Rotate Access key");
            var keyRotator = new KeyRotator(log, keyVaultName);
            var newConnectionString = await keyRotator.RotateKeyAsync(secretName);
            var hostDbConnectionString = await keyRotator.GetHostDbConnectionString();
            keyRotator.UpdateKeyToDatabase(hostDbConnectionString, secretName, newConnectionString).GetAwaiter().GetResult();
        }
    }
}