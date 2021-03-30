using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using System.IO;

using System.Text.Json;

using Azure.Storage.Queues; // Namespace for Queue storage types
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Net.Http;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Web;

namespace TestULSLogs
{
    public static class Utilities
    {
        public static async void addSiteToCapture(string siteUrl,string siteId, string eventsToCapture, string captureToListUrl)
        {
            var config = GetConfig();
            var storageAccount = CloudStorageAccount.Parse(config["StorageAccountConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(config["SitesToCaptureTable"]);

            var entity = new Model.SiteToCaptureEntity(HttpUtility.UrlEncode(siteUrl), siteId,eventsToCapture, captureToListUrl);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            Model.SiteToCaptureEntity insertedCustomer = result.Result as Model.SiteToCaptureEntity;
        }
        public static  Model.SiteToCaptureEntity retrieveSiteToCapture(string siteUrl)
        {
            var config = GetConfig();
            var storageAccount = CloudStorageAccount.Parse(config["StorageAccountConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(config["SitesToCaptureTable"]);

            TableOperation retrieveOperation = TableOperation.Retrieve<Model.SiteToCaptureEntity>("",siteUrl);
            TableResult result = table.Execute(retrieveOperation);
            Model.SiteToCaptureEntity siteToCapture = result.Result as Model.SiteToCaptureEntity;
            return siteToCapture;
        }
        public static async void deleteSiteToCapture(Model.SiteToCaptureEntity siteToCapture)
        {
            var config = GetConfig();
            var storageAccount = CloudStorageAccount.Parse(config["StorageAccountConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(config["SitesToCaptureTable"]);

                      TableOperation insertOrMergeOperation = TableOperation.Delete(siteToCapture);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            Model.SiteToCaptureEntity insertedCustomer = result.Result as Model.SiteToCaptureEntity;
        }
        public static IEnumerable<Model.SiteToCaptureEntity>  getSitesToCapture()
        {
            var config = GetConfig();
            var storageAccount = CloudStorageAccount.Parse(config["StorageAccountConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(config["SitesToCaptureTable"]);
            var entities = table.ExecuteQuery(new TableQuery<Model.SiteToCaptureEntity>());
            return entities;
        }
        public static async void ProcessCallbackItem(Model.CallbackItem cbItem)
        {
            var config = GetConfig();
            var client = await GetHttpClient(config);

            var response = await client.GetStringAsync(cbItem.contentUri);
            Model.AuditItem[] auditItems = JsonSerializer.Deserialize<Model.AuditItem[]>(response);
            var sitesToCapture = getSitesToCapture();
            var  siteDictionary = new Dictionary<string, string>();
            foreach(var siteToCapture in sitesToCapture)
            {
                siteDictionary.Add(siteToCapture.SiteId, siteToCapture.EventsToCapture);
            }
            QueueClient queueClient = new QueueClient(config["StorageAccountConnectionString"], config["AuditQueueName"]);
            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();

            foreach (Model.AuditItem auditItem in auditItems)
            {
                if (auditItem.Site != null && siteDictionary.ContainsKey(auditItem.Site) && (siteDictionary[auditItem.Site].Contains(auditItem.Operation) || siteDictionary[auditItem.Site]=="*"))
                {
                    if (queueClient.Exists())
                    {
                        queueClient.SendMessage(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((string)JsonSerializer.Serialize(auditItem))));
                    }
                }
            }
        }
        public static async Task<string> ListSubscriptions()
        {
            var config = GetConfig();
            var client = await GetHttpClient(config);
            var Uri = new Uri($"{config["ResourceURL"]}/api/v1.0/{config["TenantId"]}/activity/feed/subscriptions/list?PublisherIdentifier={config["TenantId"]}");
            var availableContent = await client.GetStringAsync(Uri);
            return availableContent;

        }
        public static async Task<string> ListAvailableContents()
        {

            var config = GetConfig();
            var client = await GetHttpClient(config);
            string startTime = "2021-03-28";
            string endTime = "2021-03-29";
            var Uri = new Uri($"{config["ResourceURL"]}/api/v1.0/{config["TenantId"]}/activity/feed/subscriptions/content?contentType=Audit.SharePoint&PublisherIdentifier={config["TenantId"]}&startTime={startTime}&endTime={endTime}");
            var availableContent = await client.GetStringAsync(Uri);
            return availableContent;

        }
        public static async Task<string> StartSubscription()
        {
            var config = GetConfig();
            var client = await GetHttpClient(config);
            var webhook = new
            {
                webhook = new Model.WebHook()
                {
                    address = "https://1b61829ba29f.ngrok.io/api/Callback", //TODO: make this come from Http request (from a management webpart?)
                    authId = "o365activityapinotification",//TODO: make this come from Http request
                    expiration = ""
                }
            };

            var xx = JsonSerializer.Serialize(webhook);

            var Uri = new Uri($"{config["ResourceURL"]}/api/v1.0/{config["TenantId"]}/activity/feed/subscriptions/start?contentType=Audit.SharePoint&PublisherIdentifier={config["TenantId"]}`");
            var response = await client.PostAsync(Uri, new StringContent(JsonSerializer.Serialize(webhook), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            Console.WriteLine(response.Content);
            Console.WriteLine(response.Content.ToString());


            Console.WriteLine(response);
            return ":";

        }
        #region queueops
        public static void EnqueueCallbackItems(Model.CallbackItem[] cbItems, string storageAccountConnectionString, string callbackQueueName)
        {
            QueueClient queueClient = new QueueClient(storageAccountConnectionString, callbackQueueName);
            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();
            foreach (Model.CallbackItem cbItem in cbItems)
            {
                if (queueClient.Exists())
                {
                    // Send a message to the queue
                    queueClient.SendMessage(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((string)JsonSerializer.Serialize(cbItem))));
                }
            }
        }

        #endregion queueops

        #region Helpers
        public static IConfigurationRoot GetConfig()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();
            return config;
        }

        public static async Task<HttpClient> GetHttpClient(IConfigurationRoot config)
        {
            string token = await Utilities.GetToken(config);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
            return client;

        }
        public static async Task<string> GetToken(IConfigurationRoot config)
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
        #endregion Helpers
    }
}
