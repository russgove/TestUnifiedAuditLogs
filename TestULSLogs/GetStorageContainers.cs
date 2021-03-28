using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;


using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Net.Http;
using System.Security.Claims;

namespace TestULSLogs
{

    public class WebHook
    {
        public string address { get; set; }
        public string authId { get; set; }
        public string expiration { get; set; }
    }

    public class AuditItem
    {
        public string CreationTime { get; set; }
        public string Id { get; set; }
        public string Operation { get; set; }

        public string OrganizationId { get; set; }
        public int RecordType { get; set; }
        public int UserType { get; set; }
        public string UserKey { get; set; }
        public int Version { get; set; }
        public string Workload { get; set; }
        public string ClientIP { get; set; }
        public string ObjectId { get; set; }

        public string UserId { get; set; }
        public string CorrelationId { get; set; }
        public bool CustomUniqueId { get; set; }
        public string EventSource { get; set; }
        public string ItemType { get; set; }
        public string ListId { get; set; }
        public string ListItemUniqueId { get; set; }
        public string Site { get; set; }
        public string UserAgent { get; set; }
        public string WebId { get; set; }
        public string SourceFileExtension  { get; set; }
        public string SiteUrl { get; set; }
        public string SourceFileName { get; set; }
        public string SourceRelativeUrl { get; set; }

        public bool HighPriorityMediaProcessing { get; set; }

        public bool DoNotDistributeEvent { get; set; }
        public bool FromApp { get; set; }
        public bool IsDocLib { get; set; }
      


    }


    public class CallbackItem
    {
        public string clientId { get; set; }
        public string contentCreated { get; set; }
        public string contentExpiration { get; set; }

        public string contentId { get; set; }
        public string contentType { get; set; }
        public string contentUri { get; set; }
        public string tenantId { get; set; }
    }
    public static class GetStorageContainers
    {

        [FunctionName("StartSubscription")]
        public static async Task<IActionResult> StartSubscription(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
    ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                string token;
                token = await GetToken(config);
                HttpClient client = new HttpClient();
                var wh = new
                {
                    webhook = new WebHook()
                    {
                        address = "https://e3d4c33c0601.ngrok.io/api/Callback",
                        authId = "o365activityapinotification",
                        expiration = ""
                    }
                };

                var xx = JsonSerializer.Serialize(wh);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var Uri = new Uri($"{config["ResourceURL"]}/api/v1.0/{config["TenantId"]}/activity/feed/subscriptions/start?contentType=Audit.SharePoint&PublisherIdentifier={config["TenantId"]}`");
                var response = await client.PostAsync(Uri, new StringContent(JsonSerializer.Serialize(wh), Encoding.UTF8, "application/json"));
                Console.WriteLine(response.RequestMessage);
                Console.WriteLine(response.Content.ToString());


                Console.WriteLine(response);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }

            return new OkObjectResult("OK");
        }
        [FunctionName("ListAvailableContent")]
        public static async Task<IActionResult> ListAvailableContent(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                string token;
                token = await GetToken(config);
                string startTime = "2021-03-26";
                string endTime = "2021-03-27";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var Uri = new Uri($"{config["ResourceURL"]}/api/v1.0/{config["TenantId"]}/activity/feed/subscriptions/content?contentType=Audit.SharePoint&PublisherIdentifier={config["TenantId"]}&startTime={startTime}&endTime={endTime}");
                var response = await client.GetAsync(Uri);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine(response.RequestMessage);
                    Console.WriteLine(response.Content.ToString());
                    return new JsonResult(response.Content);
                }

                else
                {
                    return new BadRequestObjectResult(response.Content);
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }


        }


        [FunctionName("Callback")]
        public static IActionResult Callback(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {

            CallbackItem[] cbItems = GetCallbackItems(req);
            foreach (CallbackItem cbItem in cbItems)
            {
                ProcessCallbackItem(cbItem);
            }
            return new OkObjectResult("OK");
        }
        /// <summary>
        /// A callback item is a referenec to a url that will return a list of audititems.
        /// </summary>
        /// <param name="cbItem"></param>
        public static async void ProcessAuditItem(AuditItem auditItem)
        {
            // get the contents
            //if (auditItem.Operation == "FileDownloaded")
            //{
                Console.WriteLine($"User {auditItem.UserId} {auditItem.Operation} {auditItem.ItemType} {auditItem.SourceFileName}  in {auditItem.SourceRelativeUrl} on {auditItem.SiteUrl}");
            //}
        }
        /// <summary>
        /// A callback item is a referenec to a url that will return a list of audititems.
        /// </summary>
        /// <param name="cbItem"></param>
        public static async void ProcessCallbackItem(CallbackItem cbItem)
        {
            // get the contents
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string token;
            token = await GetToken(config);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                var response = await client.GetStringAsync(cbItem.contentUri);
            AuditItem[] auditItems = JsonSerializer.Deserialize<AuditItem[]>(response);
            foreach (var x in auditItems)
            {
                ProcessAuditItem(x);
            }
            
        }

        public static AuditItem[] GetAuditItems(HttpRequest request)
        {
            var body = GetRequestBody(request);
            return JsonSerializer.Deserialize<AuditItem[]>(body);

        }
        public static CallbackItem[] GetCallbackItems(HttpRequest request)
        {
            var body = GetRequestBody(request);
               return JsonSerializer.Deserialize<CallbackItem[]>(body);
               
              }
        public static string GetResponseBody(HttpRequest request)
        {
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEnd();
            return body;

        }
        public static string GetRequestBody(HttpRequest request)
        {
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEnd();
            return body;

        }
        private static async Task<string> GetToken(IConfigurationRoot config)
        {
            string token;
            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();
                requestParameters.Add("resource", config["ResourceURL"]);
                requestParameters.Add("client_id", "4588cb8c-24a8-43ab-bccb-15e7c3ae2030");
                requestParameters.Add("grant_type", "client_credentials");
                requestParameters.Add("client_secret", "B1Q~2VY50qSjS7O8-.0UK.gPGXm0I8kbpq");

                var url = $"https://login.microsoftonline.com/{config["TenantId"]}/oauth2/token";
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                var responseBytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responseBody = Encoding.UTF8.GetString(responseBytes);

                var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseBody);
                token = jsonObject.Value<string>("access_token");
            }

            return token;
        }
    }



}
