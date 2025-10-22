using Azure;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    internal partial class RetrievalBatchProcessor
    {
        internal class RetryRecord
        {
            public PSObject InputObject { get; set; }
            public int RetriesRemaining { get; set; }
            public DateTime NextRetryTime { get; set; }
            public Exception LastError { get; set; }
            public bool RetryInProgress { get; set; }
            public string TableName { get; internal set; }
            public Guid? CallerId { get; internal set; }
        }

        private readonly List<RecordProcessingItem> _retrievalBatchQueue;
        private readonly List<RetryRecord> _pendingRetries;
    }
}
