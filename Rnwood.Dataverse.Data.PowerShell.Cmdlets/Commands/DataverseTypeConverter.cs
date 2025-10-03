using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides utility methods for converting between PowerShell types and Dataverse SDK types.
    /// </summary>
    internal static class DataverseTypeConverter
    {
        /// <summary>
        /// Converts various object types to an EntityReference.
        /// Accepts EntityReference, PSObject with Id and TableName properties, Guid (with tableName), or string Guid (with tableName).
        /// </summary>
        public static EntityReference ToEntityReference(object value, string tableName = null, string parameterName = "parameter")
        {
            if (value is EntityReference entityRef)
            {
                return entityRef;
            }
            else if (value is PSObject psObj)
            {
                var idProp = psObj.Properties["Id"];
                var tableNameProp = psObj.Properties["TableName"] ?? psObj.Properties["LogicalName"];

                if (idProp != null && tableNameProp != null)
                {
                    return new EntityReference((string)tableNameProp.Value, (Guid)idProp.Value);
                }
            }
            else if (value is Guid guid)
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a Guid");
                }
                return new EntityReference(tableName, guid);
            }
            else if (value is string strValue && Guid.TryParse(strValue, out Guid parsedGuid))
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a string Guid");
                }
                return new EntityReference(tableName, parsedGuid);
            }

            throw new ArgumentException($"Unable to convert {parameterName} to EntityReference. Expected EntityReference, PSObject with Id and TableName properties, or Guid with TableName parameter.");
        }

        /// <summary>
        /// Converts various object types to an OptionSetValue.
        /// Accepts OptionSetValue, integer, or string that can be parsed as integer.
        /// </summary>
        public static OptionSetValue ToOptionSetValue(object value, string parameterName = "parameter")
        {
            if (value is OptionSetValue osv)
            {
                return osv;
            }
            else if (value is int intValue)
            {
                return new OptionSetValue(intValue);
            }
            else if (value is string strValue && int.TryParse(strValue, out int parsedInt))
            {
                return new OptionSetValue(parsedInt);
            }

            throw new ArgumentException($"Unable to convert {parameterName} to OptionSetValue. Expected OptionSetValue or integer value.");
        }
    }
}
