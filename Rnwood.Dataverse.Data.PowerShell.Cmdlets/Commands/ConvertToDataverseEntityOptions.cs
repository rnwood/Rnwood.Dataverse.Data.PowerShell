using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Options for converting PSObject to Dataverse Entity.
    /// </summary>
    public class ConvertToDataverseEntityOptions
    {
        /// <summary>
        /// Initializes a new instance of the ConvertToDataverseEntityOptions class.
        /// </summary>
        public ConvertToDataverseEntityOptions()
        {
            IgnoredPropertyName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ColumnOptions = new Dictionary<string,ConvertToDataverseEntityColumnOptions>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the set of property names to ignore during conversion.
        /// </summary>
        public HashSet<string> IgnoredPropertyName { get; private set; }

        /// <summary>
        /// Gets the column-specific options for conversion.
        /// </summary>
        public Dictionary<string, ConvertToDataverseEntityColumnOptions> ColumnOptions { get; private set;}
    }
}
