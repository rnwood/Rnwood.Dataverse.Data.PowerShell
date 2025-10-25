using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell
{
	public class SolutionInfo : SolutionInfoBase
	{
        public Guid? Id { get; set; }
    }

	public class SolutionFileInfo : SolutionInfoBase
	{

	}

    /// <summary>
    /// Contains information about a Dataverse solution.
    /// </summary>
    public class SolutionInfoBase
    {
		/// <summary>
		/// Gets or sets the version of the solution.
		/// </summary>
        public Version Version { get; set; }
		/// <summary>
		/// Gets or sets the unique name of the solution.
		/// </summary>
        public string UniqueName { get; set; }
		/// <summary>
		/// Gets or sets the friendly name of the solution.
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

		/// <summary>
		/// Gets or sets the publisher name.
		/// </summary>
        public string PublisherName { get; set; }
		/// <summary>
		/// Gets or sets the publisher unique name.
		/// </summary>
        public string PublisherUniqueName { get; set; }
		/// <summary>
		/// Gets or sets the publisher customization prefix.
		/// </summary>
        public string PublisherPrefix { get; set; }
    }
}