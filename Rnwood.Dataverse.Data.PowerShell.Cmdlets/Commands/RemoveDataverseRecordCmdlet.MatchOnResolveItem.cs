using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Represents an InputObject queued for MatchOn resolution in RemoveDataverseRecordCmdlet.
    /// </summary>
    internal class MatchOnResolveItem
    {
        public PSObject InputObject { get; set; }
        public Entity InputEntity { get; set; }
        public List<Guid> ResolvedIds { get; set; }
    }
}
