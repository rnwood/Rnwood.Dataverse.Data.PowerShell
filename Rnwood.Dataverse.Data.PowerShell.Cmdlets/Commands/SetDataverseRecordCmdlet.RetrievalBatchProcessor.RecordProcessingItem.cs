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
        internal class RecordProcessingItem
        {
            public PSObject InputObject { get; set; }
            public Entity Target { get; set; }
            public EntityMetadata EntityMetadata { get; set; }
            public Entity ExistingRecord { get; set; }
            public string TableName { get; internal set; }
            public Guid? CallerId { get; internal set; }
        }

        /// <summary>
        /// Represents a record scheduled for retry after a failure.
    }
}
