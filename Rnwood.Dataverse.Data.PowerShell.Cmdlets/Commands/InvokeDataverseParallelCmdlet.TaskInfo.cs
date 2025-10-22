using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public partial class InvokeDataverseParallelCmdlet
    {
        /// <summary>
        /// Helper class to track task information including chunk number.
        /// </summary>
        private class TaskInfo
        {
            public System.Management.Automation.PowerShell PowerShell { get; set; }
            public IAsyncResult AsyncResult { get; set; }
            public PSDataCollection<PSObject> OutputCollection { get; set; }
            public int ChunkNumber { get; set; }
            public int RecordCount { get; set; }
            public int LastOutputIndex { get; set; } = -1;
            public int LastErrorIndex { get; set; } = -1;
            public int LastVerboseIndex { get; set; } = -1;
            public int LastWarningIndex { get; set; } = -1;
            public int LastDebugIndex { get; set; } = -1;
            public int LastInformationIndex { get; set; } = -1;
            public EventHandler<DataAddedEventArgs> OutputHandler { get; set; }
            public EventHandler<DataAddedEventArgs> ErrorHandler { get; set; }
            public EventHandler<DataAddedEventArgs> VerboseHandler { get; set; }
            public EventHandler<DataAddedEventArgs> WarningHandler { get; set; }
            public EventHandler<DataAddedEventArgs> DebugHandler { get; set; }
            public EventHandler<DataAddedEventArgs> InformationHandler { get; set; }
        }
    }
}
