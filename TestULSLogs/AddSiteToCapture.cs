using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TestULSLogs
{
    public static class AddSiteToCapture
    {
        [FunctionName("AddSiteToCapture")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AddSiteToCapture Started");

            string siteUrl = req.Query["siteUrl"];
            string siteId = req.Query["siteId"];
            string eventsToCapture = req.Query["eventsToCapture"];
            string captureToListUrl = req.Query["captureToListUrl"];

            if (String.IsNullOrEmpty(siteUrl) | String.IsNullOrEmpty(eventsToCapture) | String.IsNullOrEmpty(captureToListUrl))
            { return new BadRequestResult(); }

            Utilities.addSiteToCapture(siteUrl, siteId,eventsToCapture, captureToListUrl);

            return new OkObjectResult("OK");
        }
    }
}
