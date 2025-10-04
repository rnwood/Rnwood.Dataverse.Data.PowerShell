using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Options for converting a specific column from PSObject to Dataverse.
    /// </summary>
    public class ConvertToDataverseEntityColumnOptions
    {
        /// <summary>
        /// Gets or sets the name of the lookup column to use for finding records.
        /// </summary>
        public string LookupColumn { get; set; }
    }
}