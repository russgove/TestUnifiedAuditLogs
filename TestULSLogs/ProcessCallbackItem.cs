
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TestULSLogs
{
    public static class ProcessCallbackItem
    {
 

        [FunctionName("ProcessCallbackItem")]
        public static async Task RunAsync([QueueTrigger("callbackstoragequeue", Connection = "StorageAccountConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"ProcessCallbackItem Started");
        
            Model.CallbackItem cbItem = JsonSerializer.Deserialize<Model.CallbackItem>(myQueueItem);
            Utilities.ProcessCallbackItem(cbItem);
        }
    }
}
