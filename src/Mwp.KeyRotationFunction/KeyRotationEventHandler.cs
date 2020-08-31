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
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            var objectInput = JObject.Parse(eventGridEvent.Data.ToString());
            var keyVaultName = (string)objectInput["VaultName"];
            var secretName = (string)objectInput["ObjectName"];
            var secretVersion = (string)objectInput["Version"];

            KeyRotationManager.RotateKeyAsync(keyVaultName, secretName, secretVersion).GetAwaiter().GetResult();
        }
    }
}