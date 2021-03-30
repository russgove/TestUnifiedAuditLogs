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
using System.Collections.Generic;
using System.Reflection;

namespace TestULSLogs
{
    /// <summary>
    /// This is an item we send to M365 to request a subscription be added to the audit logs
    /// </summary>
   

    /// <summary>
    /// This represents  the data sent back to  our callback when new Audit content is available. The callback
    /// recives an array of these.
    /// </summary>
    public static class ProcessCallback
    {
       

        [FunctionName("Callback")]
        public static IActionResult Callback(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            var body = GetRequestBody(req);
            //if its just a validationCode (which M365 sends when setting up a new callback), return ok
  
            try
            {
                var test = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                if (test.ContainsKey("validationCode"))
                {
                    return new OkObjectResult("OK");
                }
            }
            catch (Exception e) {
                // do nothing, iot wasn;t a validation callback
            }
           
            var config = Utilities.GetConfig();
            Model.CallbackItem[] cbItems = JsonSerializer.Deserialize<Model.CallbackItem[]>(body);
            Utilities.EnqueueCallbackItems(cbItems, config["StorageAccountConnectionString"], config["CallbackQueueName"]);
            return new OkObjectResult("OK");
        }
      
 
        public static string GetRequestBody(HttpRequest request)
        {
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEnd();
            return body;

        }
    
    }



}
