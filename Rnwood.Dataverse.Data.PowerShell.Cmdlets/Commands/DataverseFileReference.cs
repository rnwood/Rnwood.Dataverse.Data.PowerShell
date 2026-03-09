using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Represents a reference to a Dataverse file attachment.
	/// </summary>
    public struct DataverseFileReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataverseFileReference"/> struct.
        /// </summary>
        /// <param name="id">The unique identifier of the record.</param>
        public DataverseFileReference(Guid id) : this()
        {
            Id = id;
        }


		/// <summary>
		/// Gets or sets the unique identifier of the record.
		/// </summary>
        public Guid Id
        { get; set; }


        public string Type => "FileReference";

		/// <summary>
		/// Implicitly converts a DataverseEntityReference to a Guid.
		/// </summary>
		/// <param name="value">The entity reference to convert.</param>
        public static implicit operator Guid(DataverseFileReference value)
        {
            return value.Id;
        }


		/// <summary>
		/// Returns a string representation of this instance.
		/// </summary>
		/// <returns>A string containing the table name and ID.</returns>
        public override string ToString()
        {
            return "Id=" + Id;
        }
    }
}