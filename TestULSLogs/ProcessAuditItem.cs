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
    public static class ProcessAuditItem
    {
        [FunctionName("ProcessAuditItem")]
        public static void Run([QueueTrigger("auditstoragequeue", Connection = "StorageAccountConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"ProcessAuditItem Started");
            Model.AuditItem auditItem = JsonSerializer.Deserialize<Model.AuditItem>(myQueueItem);
            Console.WriteLine($"User {auditItem.UserId} {auditItem.Operation} {auditItem.ItemType} {auditItem.SourceFileName}  in {auditItem.SourceRelativeUrl} on {auditItem.SiteUrl}");
        }
    }
}
