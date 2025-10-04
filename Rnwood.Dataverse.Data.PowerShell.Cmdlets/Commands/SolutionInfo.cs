using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell
{
	/// <summary>
	/// Contains information about a Dataverse solution.
	/// </summary>
    public class SolutionInfo
    {
		/// <summary>
		/// Gets or sets the version of the solution.
		/// </summary>
        public Version Version { get; set; }
		/// <summary>
		/// Gets or sets the name of the solution.
		/// </summary>
        public string Name { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether the solution is managed.
		/// </summary>
        public bool IsManaged { get; set; }
		/// <summary>
		/// Gets or sets the description of the solution.
		/// </summary>
        public string Description { get; set; }
		/// <summary>
		/// Gets or sets the unique identifier of the solution.
		/// </summary>
        public Guid Id { get; set; }
    }
}