using System;
using System.Collections.Generic;
using System.Text;

namespace TestULSLogs
{
    using Microsoft.Azure.Cosmos.Table; // for azure table storage
    public class Model
    {
        public class SiteToCaptureEntity : TableEntity
        {
            public SiteToCaptureEntity()
            {
            }

            public SiteToCaptureEntity(string siteUrl,string siteId ,string eventsToCapture, string captureToSiteId, string captureToListId)
            {
                this.PartitionKey = "";
                this.RowKey = siteId;
                this.SiteUrl = siteUrl;
                this.SiteId = siteId;
                this.EventsToCapture = eventsToCapture;
                this.CaptureToSiteId = captureToSiteId;
                this.CaptureToListId = captureToListId;
            }
            public string SiteUrl  { get; set; }
            public string SiteId { get; set; }
            public string CaptureToSiteId { get; set; }
            public string CaptureToListId { get; set; }
            public string EventsToCapture { get; set; }
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
        public class WebHook
        {
            public string address { get; set; }
            public string authId { get; set; }
            public string expiration { get; set; }
        }
        /// <summary>
        /// This represents the details of an Audit item
        /// </summary>
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
            public string SourceFileExtension { get; set; }
            public string SiteUrl { get; set; }
            public string SourceFileName { get; set; }
            public string SourceRelativeUrl { get; set; }

            public bool HighPriorityMediaProcessing { get; set; }

            public bool DoNotDistributeEvent { get; set; }
            public bool FromApp { get; set; }
            public bool IsDocLib { get; set; }



        }
    }
}
