using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public abstract class DataverseCmdlet : PSCmdlet
    {
        public abstract DataverseConnection Connection { get; set; }


    }
}