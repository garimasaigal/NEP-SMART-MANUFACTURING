using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SignalRFunctions
{
    public static class SignalRFunctions
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,[SignalRConnectionInfo(HubName = "dttelemetry")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("broadcast")]
        public static Task SendMessage([EventGridTrigger] EventGridEvent eventGridEvent,[SignalR(HubName = "dttelemetry")] IAsyncCollector<SignalRMessage> signalRMessages,ILogger log)
        {
            JObject eventGridData = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

            var data = eventGridData.SelectToken("data");

            var telemetryMessage = new Dictionary<object, object>();
            foreach (JProperty property in data.Children())
            {
                log.LogInformation(property.Name + " - " + property.Value);
                telemetryMessage.Add(property.Name, property.Value);
            }

            return signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "TelemetryMessage",
                Arguments = new[] { telemetryMessage }
            });
        }
    }
}