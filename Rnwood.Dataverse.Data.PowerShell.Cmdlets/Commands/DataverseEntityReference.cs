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
	/// Represents a reference to a Dataverse entity.
	/// </summary>
    public struct DataverseEntityReference
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DataverseEntityReference"/> struct.
		/// </summary>
		/// <param name="tableName">The logical name of the table.</param>
		/// <param name="id">The unique identifier of the record.</param>
        public DataverseEntityReference(string tableName, Guid id) : this()
        {
            TableName = tableName;
            Id = id;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataverseEntityReference"/> struct from an EntityReference.
		/// </summary>
		/// <param name="entityReference">The entity reference to convert from.</param>
        public DataverseEntityReference(EntityReference entityReference) : this(entityReference.LogicalName, entityReference.Id)
        {
        }

		/// <summary>
		/// Gets or sets the unique identifier of the record.
		/// </summary>
        public Guid Id
        { get; set; }

		/// <summary>
		/// Gets or sets the logical name of the table.
		/// </summary>
        public string TableName { get; set; }


        public string Type => "EntityReference";

        /// <summary>
        /// Implicitly converts a DataverseEntityReference to a Guid.
        /// </summary>
        /// <param name="value">The entity reference to convert.</param>
        public static implicit operator Guid(DataverseEntityReference value)
        {
            return value.Id;
        }

		/// <summary>
		/// Converts this instance to an EntityReference.
		/// </summary>
		/// <returns>An EntityReference representing this instance.</returns>
        public EntityReference ToEntityReference()
        {
            return new EntityReference(TableName, Id);
        }

		/// <summary>
		/// Returns a string representation of this instance.
		/// </summary>
		/// <returns>A string containing the table name and ID.</returns>
        public override string ToString()
        {
            return "TableName=" + TableName + "; Id=" + Id;
        }
    }
}