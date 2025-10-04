using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Specifies the type of value to retrieve from Dataverse.
	/// </summary>
    public enum ValueType
    {
		/// <summary>
		/// Raw value (e.g., GUID for lookups, numeric value for choices).
		/// </summary>
        Raw,
		/// <summary>
		/// Display value (e.g., name for lookups, label for choices).
		/// </summary>
        Display
    }
}
