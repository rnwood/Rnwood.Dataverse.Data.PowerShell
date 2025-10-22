using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Shared helper methods for query execution and formatting.
    /// </summary>
    internal static class QueryHelpers
    {
        /// <summary>
        /// Executes a query with automatic paging and verbose output.
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <param name="connection">The organization service connection</param>
        /// <param name="writeVerbose">Action to write verbose messages</param>
        /// <returns>Enumerable of entities from all pages</returns>
        public static IEnumerable<Entity> ExecuteQueryWithPaging(QueryBase query, IOrganizationService connection, Action<string> writeVerbose)
        {
            writeVerbose($"Executing query: {QueryToVerboseString(query)}");

            // Only set PageInfo if TopCount is not already set (e.g., from FetchXML top attribute)
            // because Dataverse doesn't allow both TopCount and PageInfo to be set simultaneously
            if (query is QueryExpression qe && !qe.TopCount.HasValue)
            {
                PagingInfo pageInfo = new PagingInfo()
                {
                    PageNumber = 1,
                    Count = 1000
                };

                qe.PageInfo = pageInfo;

                RetrieveMultipleRequest request = new RetrieveMultipleRequest()
                {
                    Query = qe
                };

                RetrieveMultipleResponse response;
                int pageNum = 0;

                do
                {
                    pageNum++;
                    writeVerbose($"Retrieving page {pageNum}...");
                    response = (RetrieveMultipleResponse)connection.Execute(request);
                    writeVerbose($"Page {pageNum} returned {response.EntityCollection.Entities.Count} records");

                    pageInfo.PageNumber++;
                    pageInfo.PagingCookie = response.EntityCollection.PagingCookie;

                    foreach (Entity entity in response.EntityCollection.Entities)
                    {
                        yield return entity;
                    }

                } while (response.EntityCollection.MoreRecords);
                
                writeVerbose($"Query complete. Retrieved {pageNum} page(s)");
            }
            else
            {
                // When TopCount is set (e.g., from FetchXML), execute without PageInfo
                string topCountStr = query is QueryExpression qe2 ? qe2.TopCount?.ToString() : (query is QueryByAttribute qba2 ? qba2.TopCount?.ToString() : "null");
                writeVerbose($"Executing query with TopCount={topCountStr}");
                RetrieveMultipleRequest request = new RetrieveMultipleRequest()
                {
                    Query = query
                };

                RetrieveMultipleResponse response = (RetrieveMultipleResponse)connection.Execute(request);
                writeVerbose($"Query returned {response.EntityCollection.Entities.Count} records");

                foreach (Entity entity in response.EntityCollection.Entities)
                {
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// Converts a query to a verbose string representation.
        /// </summary>
        /// <param name="query">The query to format</param>
        /// <returns>String representation of the query</returns>
        public static string QueryToVerboseString(QueryBase query)
        {
            if (query is QueryExpression qe)
            {
                var sb = new StringBuilder();
                sb.Append($"QueryExpression(EntityName={qe.EntityName}");
                if (qe.TopCount.HasValue)
                    sb.Append($", TopCount={qe.TopCount}");
                if (qe.Criteria?.Conditions?.Count > 0)
                    sb.Append($", Conditions={qe.Criteria.Conditions.Count}");
                if (qe.ColumnSet != null)
                {
                    if (qe.ColumnSet.AllColumns)
                        sb.Append($", Columns=All");
                    else
                        sb.Append($", Columns={qe.ColumnSet.Columns.Count}");
                }
                sb.Append(")");
                
                // Add XML serialization for full query details
                try
                {
                    var serializer = new DataContractSerializer(typeof(QueryExpression));
                    using (var sw = new StringWriter())
                    using (var writer = new System.Xml.XmlTextWriter(sw))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented;
                        serializer.WriteObject(writer, qe);
                        sb.Append($"\nFull Query XML:\n{sw}");
                    }
                }
                catch (Exception ex)
                {
                    sb.Append($"\nQuery serialization error: {ex.Message}");
                }
                
                return sb.ToString();
            }
            else if (query is QueryByAttribute qba)
            {
                var sb = new StringBuilder();
                sb.Append($"QueryByAttribute(EntityName={qba.EntityName}, Attributes={qba.Attributes.Count}, Columns={qba.ColumnSet?.Columns?.Count ?? 0}");
                if (qba.TopCount.HasValue)
                    sb.Append($", TopCount={qba.TopCount}");
                sb.Append(")");
                
                // Add XML serialization for full query details
                try
                {
                    var serializer = new DataContractSerializer(typeof(QueryByAttribute));
                    using (var sw = new StringWriter())
                    using (var writer = new System.Xml.XmlTextWriter(sw))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented;
                        serializer.WriteObject(writer, qba);
                        sb.Append($"\nFull Query XML:\n{sw}");
                    }
                }
                catch (Exception ex)
                {
                    sb.Append($"\nQuery serialization error: {ex.Message}");
                }
                
                return sb.ToString();
            }
            return query.GetType().Name;
        }

        /// <summary>
        /// Compares two values with proper case-insensitive comparison for strings to match Dataverse query behavior.
        /// </summary>
        /// <param name="value1">First value to compare</param>
        /// <param name="value2">Second value to compare</param>
        /// <returns>True if values are equal according to Dataverse semantics</returns>
        public static bool AreValuesEqual(object value1, object value2)
        {
            // Handle nulls
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Use case-insensitive comparison for strings to match Dataverse behavior
            if (value1 is string str1 && value2 is string str2)
            {
                return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
            }

            // For other types, use standard equality
            return Equals(value1, value2);
        }
    }
}
