using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public struct DataverseEntityReference
    {
        public DataverseEntityReference(string tableName, Guid id) : this()
        {
            TableName = tableName;
            Id = id;
        }

        public DataverseEntityReference(EntityReference entityReference) : this(entityReference.LogicalName, entityReference.Id)
        {
        }

        public Guid Id
        { get; set; }

        public string TableName { get; set; }

        public static implicit operator Guid(DataverseEntityReference value)
        {
            return value.Id;
        }

        public EntityReference ToEntityReference()
        {
            return new EntityReference(TableName, Id);
        }

        public override string ToString()
        {
            return "TableName=" + TableName + "; Id=" + Id;
        }
    }
}