using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    internal class ConvertToDataverseEntityOptions
    {
        public ConvertToDataverseEntityOptions()
        {
            IgnoredPropertyName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ColumnOptions = new Dictionary<string,ConvertToDataverseEntityColumnOptions>(StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> IgnoredPropertyName { get; private set; }

        public Dictionary<string, ConvertToDataverseEntityColumnOptions> ColumnOptions { get; private set;}
    }
}
