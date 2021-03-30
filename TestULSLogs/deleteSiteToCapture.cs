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
    public static class DeleteSiteToCapture
    {
        [FunctionName("DeleteSiteToCapture")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("DeleteSiteToCapture Started");

            string siteId = req.Query["siteId"];
            if (String.IsNullOrEmpty(siteId)){
                return new BadRequestResult();
            }
            var siteToCapture=Utilities.retrieveSiteToCapture(siteId);
            if (siteToCapture == null)
            {
                return new NotFoundResult();
            }
            Utilities.deleteSiteToCapture(siteToCapture);
                return new OkObjectResult("OK");
        }
    }
}
