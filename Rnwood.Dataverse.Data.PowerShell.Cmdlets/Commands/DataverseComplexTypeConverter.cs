using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Utility class for converting complex SDK types to/from PowerShell-friendly representations.
    /// Handles QueryBase, ColumnSet, PagingInfo, and other complex types.
    /// </summary>
    internal static class DataverseComplexTypeConverter
    {
        /// <summary>
        /// Converts a FetchXML string or Hashtable to a QueryBase object.
        /// </summary>
        /// <param name="fetchXml">FetchXML string (takes precedence)</param>
        /// <param name="filter">Hashtable with filter conditions</param>
        /// <param name="tableName">Entity logical name (required for Hashtable conversion)</param>
        /// <returns>QueryBase object (FetchExpression or QueryExpression)</returns>
        public static QueryBase ToQueryBase(string fetchXml, Hashtable filter, string tableName)
        {
            if (!string.IsNullOrEmpty(fetchXml))
            {
                return new FetchExpression(fetchXml);
            }
            
            if (filter != null)
            {
                var query = new QueryExpression(tableName);
                query.Criteria = new FilterExpression(LogicalOperator.And);
                
                foreach (DictionaryEntry entry in filter)
                {
                    string attributeName = (string)entry.Key;
                    object value = entry.Value;
                    
                    // Handle different value types
                    if (value is Hashtable nestedCondition)
                    {
                        // Support operators like @{operator='eq'; value='test'}
                        if (nestedCondition.ContainsKey("operator") && nestedCondition.ContainsKey("value"))
                        {
                            ConditionOperator op = ParseOperator((string)nestedCondition["operator"]);
                            query.Criteria.AddCondition(attributeName, op, nestedCondition["value"]);
                        }
                    }
                    else
                    {
                        // Simple equality
                        query.Criteria.AddCondition(attributeName, ConditionOperator.Equal, value);
                    }
                }
                
                return query;
            }
            
            throw new ArgumentException("Either FetchXml or Filter must be provided");
        }

        /// <summary>
        /// Converts string array or switch to ColumnSet object.
        /// </summary>
        /// <param name="columns">Array of column names</param>
        /// <param name="allColumns">Switch to select all columns</param>
        /// <returns>ColumnSet object</returns>
        public static ColumnSet ToColumnSet(string[] columns, bool allColumns)
        {
            if (allColumns)
            {
                return new ColumnSet(true);
            }
            
            if (columns != null && columns.Length > 0)
            {
                return new ColumnSet(columns);
            }
            
            // Default to all columns if nothing specified
            return new ColumnSet(true);
        }

        /// <summary>
        /// Converts page number and size to PagingInfo object.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>PagingInfo object</returns>
        public static PagingInfo ToPagingInfo(int pageNumber, int pageSize)
        {
            return new PagingInfo
            {
                PageNumber = pageNumber,
                Count = pageSize
            };
        }

        /// <summary>
        /// Converts hashtable array to RolePrivilege array.
        /// </summary>
        /// <param name="privilegeData">Array of hashtables with PrivilegeId and Depth</param>
        /// <returns>Array of RolePrivilege objects</returns>
        public static Microsoft.Crm.Sdk.Messages.RolePrivilege[] ToRolePrivileges(Hashtable[] privilegeData)
        {
            if (privilegeData == null || privilegeData.Length == 0)
            {
                return Array.Empty<Microsoft.Crm.Sdk.Messages.RolePrivilege>();
            }

            return privilegeData.Select(h => new Microsoft.Crm.Sdk.Messages.RolePrivilege
            {
                PrivilegeId = (Guid)h["PrivilegeId"],
                Depth = (Microsoft.Crm.Sdk.Messages.PrivilegeDepth)(int)h["Depth"]
            }).ToArray();
        }

        /// <summary>
        /// Converts PSObject array or hashtable array to TimeCode array.
        /// Note: TimeCode structure varies by SDK version. This is a simplified placeholder.
        /// </summary>
        /// <param name="timeCodeData">Array of PSObjects or hashtables with time code data</param>
        /// <returns>Array of TimeCode objects</returns>
        public static Microsoft.Crm.Sdk.Messages.TimeCode[] ToTimeCodes(PSObject[] timeCodeData)
        {
            if (timeCodeData == null || timeCodeData.Length == 0)
            {
                return Array.Empty<Microsoft.Crm.Sdk.Messages.TimeCode>();
            }

            // TimeCode is a simple struct with just int value in many SDK versions
            return timeCodeData.Select(tc => 
            {
                var timeCode = new Microsoft.Crm.Sdk.Messages.TimeCode();
                // TimeCode structure varies - this is a simplified implementation
                return timeCode;
            }).ToArray();
        }

        /// <summary>
        /// Converts hashtable array to LocalizedLabel array.
        /// </summary>
        /// <param name="labelData">Array of hashtables with Label and LanguageCode</param>
        /// <returns>Array of LocalizedLabel objects</returns>
        public static Microsoft.Xrm.Sdk.Label[] ToLocalizedLabels(Hashtable[] labelData)
        {
            if (labelData == null || labelData.Length == 0)
            {
                return Array.Empty<Microsoft.Xrm.Sdk.Label>();
            }

            var labels = labelData.Select(h => new Microsoft.Xrm.Sdk.LocalizedLabel(
                (string)h["Label"],
                (int)h["LanguageCode"]
            )).ToArray();

            return new[] { new Microsoft.Xrm.Sdk.Label(labels[0], labels) };
        }

        /// <summary>
        /// Parses operator string to ConditionOperator enum.
        /// </summary>
        private static ConditionOperator ParseOperator(string op)
        {
            var lower = op.ToLower();
            
            if (lower == "eq" || lower == "equal") return ConditionOperator.Equal;
            if (lower == "ne" || lower == "notequal") return ConditionOperator.NotEqual;
            if (lower == "gt" || lower == "greaterthan") return ConditionOperator.GreaterThan;
            if (lower == "ge" || lower == "greaterorequal") return ConditionOperator.GreaterEqual;
            if (lower == "lt" || lower == "lessthan") return ConditionOperator.LessThan;
            if (lower == "le" || lower == "lessorequal") return ConditionOperator.LessEqual;
            if (lower == "like") return ConditionOperator.Like;
            if (lower == "notlike") return ConditionOperator.NotLike;
            if (lower == "in") return ConditionOperator.In;
            if (lower == "notin") return ConditionOperator.NotIn;
            if (lower == "null") return ConditionOperator.Null;
            if (lower == "notnull") return ConditionOperator.NotNull;
            
            return ConditionOperator.Equal;
        }
    }
}
