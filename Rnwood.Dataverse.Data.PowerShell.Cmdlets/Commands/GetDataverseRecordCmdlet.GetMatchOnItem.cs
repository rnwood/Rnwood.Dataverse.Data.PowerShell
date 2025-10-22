using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public partial class GetDataverseRecordCmdlet
    {
        /// <summary>
        /// Represents an InputObject queued for MatchOn resolution in GetDataverseRecordCmdlet.
        /// </summary>
        internal class GetMatchOnItem
        {
            public PSObject InputObject { get; set; }
            public Entity InputEntity { get; set; }
            public List<Entity> MatchedRecords { get; set; }
            public string[] MatchedOnColumns { get; set; }
        }
    }
}
