using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;
using System.IO;

using System.Text.Json;

using Azure.Storage.Queues; // Namespace for Queue storage types
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Net.Http;

namespace TestULSLogs
{
    public static class StartSubscription
    {
        [FunctionName("StartSubscription")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("StartSubscription Started");
            string address = req.Query["address"];
            string authId = req.Query["authId"];
            string expiration = req.Query["expiration"];
            var subs = await Utilities.StartSubscription(address,authId, expiration);

            return new OkObjectResult(subs);
        }
    }
}
