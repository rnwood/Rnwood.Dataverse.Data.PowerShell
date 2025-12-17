using MarkMpn.Sql4Cds.Engine.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Shared helper methods for query execution, formatting, and batch processing utilities.
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
        public static IEnumerable<Entity> ExecuteQueryWithPaging(QueryBase query, IOrganizationService connection, Action<string> writeVerbose, bool unpublished = false)
        {
            return ExecuteQueryWithPaging(query, connection, writeVerbose, null, null, unpublished);
        }

        /// <summary>
        /// Executes a query with automatic paging, verbose output, and cancellation support.
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <param name="connection">The organization service connection</param>
        /// <param name="writeVerbose">Action to write verbose messages</param>
        /// <param name="isStopping">Function to check if the cmdlet is stopping</param>
        /// <param name="cancellationToken">Cancellation token to check during IO operations</param>
        /// <param name="unpublished">Whether to retrievepublished entities</param>
        /// <returns>Enumerable of entities from all pages</returns>
        public static IEnumerable<Entity> ExecuteQueryWithPaging(QueryBase query, IOrganizationService connection, Action<string> writeVerbose, Func<bool> isStopping, System.Threading.CancellationToken? cancellationToken, bool unpublished = false)
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

                OrganizationRequest request = !unpublished ? new RetrieveMultipleRequest() { Query = qe } : (OrganizationRequest)new RetrieveUnpublishedMultipleRequest() { Query = qe };

                OrganizationResponse response;
                EntityCollection entityCollection;
                int pageNum = 0;

                do
                {
                    // Check for cancellation before fetching next page
                    if ((isStopping != null && isStopping()) || (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested))
                    {
                        writeVerbose($"Query execution cancelled after {pageNum} page(s)");
                        yield break;
                    }

                    pageNum++;
                    writeVerbose($"Retrieving page {pageNum}...");
                    response = connection.Execute(request);
                    entityCollection = GetEntityCollectionFromResponse(response);
                    writeVerbose($"Page {pageNum} returned {entityCollection.Entities.Count} records");

                    pageInfo.PageNumber++;
                    pageInfo.PagingCookie = entityCollection.PagingCookie;

                    foreach (Entity entity in entityCollection.Entities)
                    {
                        // Check for cancellation during record iteration
                        if ((isStopping != null && isStopping()) || (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested))
                        {
                            writeVerbose($"Query execution cancelled after {pageNum} page(s)");
                            yield break;
                        }
                        
                        yield return entity;
                    }

                } while (entityCollection.MoreRecords);
                
                writeVerbose($"Query complete. Retrieved {pageNum} page(s)");
            }
            else
            {
                // When TopCount is set (e.g., from FetchXML), execute without PageInfo
                string topCountStr = query is QueryExpression qe2 ? qe2.TopCount?.ToString() : (query is QueryByAttribute qba2 ? qba2.TopCount?.ToString() : "null");
                writeVerbose($"Executing query with TopCount={topCountStr}");
                OrganizationRequest request = !unpublished ? new RetrieveMultipleRequest() { Query = query } : (OrganizationRequest)new RetrieveUnpublishedMultipleRequest() { Query = query };

                OrganizationResponse response = connection.Execute(request);
                EntityCollection entityCollection = GetEntityCollectionFromResponse(response);
                writeVerbose($"Query returned {entityCollection.Entities.Count} records");

                foreach (Entity entity in entityCollection.Entities)
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

        /// <summary>
        /// Applies bypass business logic execution parameters to a request.
        /// </summary>
        /// <param name="request">The organization request to apply bypass parameters to</param>
        /// <param name="bypassBusinessLogicExecution">Types of business logic to bypass</param>
        /// <param name="bypassBusinessLogicExecutionStepIds">IDs of specific steps to bypass</param>
        public static void ApplyBypassBusinessLogicExecution(
            OrganizationRequest request,
            CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] bypassBusinessLogicExecution,
            Guid[] bypassBusinessLogicExecutionStepIds)
        {
            if (bypassBusinessLogicExecution?.Length > 0)
            {
                request.Parameters["BypassBusinessLogicExecution"] = string.Join(",", bypassBusinessLogicExecution.Select(o => o.ToString()));
            }
            else
            {
                request.Parameters.Remove("BypassBusinessLogicExecution");
            }

            if (bypassBusinessLogicExecutionStepIds?.Length > 0)
            {
                request.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", bypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
            }
            else
            {
                request.Parameters.Remove("BypassBusinessLogicExecutionStepIds");
            }
        }

        /// <summary>
        /// Appends fault details to a string builder, including inner faults recursively.
        /// </summary>
        /// <param name="fault">The organization service fault</param>
        /// <param name="output">The string builder to append to</param>
        public static void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
        {
            output.AppendLine("OrganizationServiceFault " + fault.ErrorCode + ": " + fault.Message);
            output.AppendLine(fault.TraceText);

            if (fault.InnerFault != null)
            {
                output.AppendLine("---");
                AppendFaultDetails(fault.InnerFault, output);
            }
        }

        /// <summary>
        /// Gets a formatted summary of all columns in an entity.
        /// </summary>
        public static string GetColumnSummary(Entity entity, DataverseEntityConverter converter, bool useEllipsis = true)
        {
            PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(entity.Attributes.Select(a => a.Key).ToArray()), a => ValueType.Raw);
            return string.Join("\n", psObject.Properties.Select(a => a.Name + " = " + (useEllipsis ? Ellipsis(GetValueSummary(a.Value).ToString()) : GetValueSummary(a.Value).ToString())));
        }

        /// <summary>
        /// Truncates a string value to 100 characters with ellipsis.
        /// </summary>
        public static string Ellipsis(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length <= 100)
            {
                return value;
            }

            return value.Substring(0, 100) + "...";
        }

        /// <summary>
        /// Gets a summary of values, handling collections.
        /// </summary>
        public static object GetValueSummary(object value)
        {
            if (!(value is string) && value is IEnumerable enumerable)
            {
                return "[" + string.Join(", ", enumerable.Cast<object>().Select(i => GetValueSummary(i))) + "]";
            }

            return value ?? "<null>";
        }

        /// <summary>
        /// Extracts the EntityCollection from a query response, handling both published and unpublished response types.
        /// </summary>
        /// <param name="response">The organization response from a query request</param>
        /// <returns>The EntityCollection containing the query results</returns>
        private static EntityCollection GetEntityCollectionFromResponse(OrganizationResponse response)
        {
            if (response is RetrieveMultipleResponse rmr)
                return rmr.EntityCollection;
            if (response is RetrieveUnpublishedMultipleResponse rumr)
                return rumr.EntityCollection;
            throw new InvalidOperationException($"Unexpected response type: {response.GetType().Name}");
        }

        /// <summary>
        /// Determines if a FaultException indicates that a record or entity was not found.
        /// This method handles various error codes and messages used by different versions of 
        /// FakeXrmEasy and real Dataverse environments.
        /// </summary>
        /// <param name="ex">The FaultException to check</param>
        /// <returns>True if the exception indicates the entity/record was not found, false otherwise</returns>
        public static bool IsNotFoundException(FaultException<OrganizationServiceFault> ex)
        {
            if (ex == null)
                return false;

            // Check for standard error codes:
            // -2147220969 (0x80040217): Record not found (Dataverse standard)
            // -2146233088 (0x80131500): Object does not exist (common in FakeXrmEasy)
            if (ex.Detail != null && (ex.Detail.ErrorCode == -2147220969 || ex.Detail.ErrorCode == -2146233088))
                return true;

            // FakeXrmEasy sometimes sets HResult instead of Detail.ErrorCode
            if (ex.HResult == -2146233088)
                return true;

            // Check message for various "not found" patterns (case-insensitive)
            if (ex.Message != null)
            {
                string lowerMessage = ex.Message.ToLower();
                if (lowerMessage.Contains("does not exist") || 
                    lowerMessage.Contains("not found") ||
                    lowerMessage.Contains("doesn't exist"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a generic FaultException indicates that a record or entity was not found.
        /// This overload handles non-generic FaultException which may be thrown by some versions of FakeXrmEasy.
        /// </summary>
        /// <param name="ex">The FaultException to check</param>
        /// <returns>True if the exception indicates the entity/record was not found, false otherwise</returns>
        public static bool IsNotFoundException(FaultException ex)
        {
            if (ex == null)
                return false;

            // Check HResult for object does not exist error
            if (ex.HResult == -2146233088 || ex.HResult == -2147220969)
                return true;

            // Check message for various "not found" patterns (case-insensitive)
            if (ex.Message != null)
            {
                string lowerMessage = ex.Message.ToLower();
                if (lowerMessage.Contains("does not exist") || 
                    lowerMessage.Contains("not found") ||
                    lowerMessage.Contains("doesn't exist"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if an OrganizationServiceFault indicates that a record or entity was not found.
        /// Used when working with faults from ExecuteMultipleResponse or other batch operations.
        /// </summary>
        /// <param name="fault">The OrganizationServiceFault to check</param>
        /// <returns>True if the fault indicates the entity/record was not found, false otherwise</returns>
        public static bool IsNotFoundException(OrganizationServiceFault fault)
        {
            if (fault == null)
                return false;

            // Check for standard error codes
            if (fault.ErrorCode == -2147220969 || fault.ErrorCode == -2146233088)
                return true;

            // Check message for various "not found" patterns (case-insensitive)
            if (fault.Message != null)
            {
                string lowerMessage = fault.Message.ToLower();
                if (lowerMessage.Contains("does not exist") || 
                    lowerMessage.Contains("not found") ||
                    lowerMessage.Contains("doesn't exist"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Executes a query for both unpublished and published records, deduplicating results by ID.
        /// Unpublished records take precedence over published records when both exist for the same ID.
        /// Deduplication is based on entity ID (Guid), not entity type or logical name.
        /// This method is designed for single-entity queries where all results share the same entity type.
        /// </summary>
        /// <param name="query">The query to execute (should target a single entity type)</param>
        /// <param name="connection">The organization service connection</param>
        /// <param name="writeVerbose">Action to write verbose messages</param>
        /// <returns>Enumerable of deduplicated entities, with unpublished records preferred</returns>
        public static IEnumerable<Entity> ExecuteQueryWithPublishedAndUnpublished(QueryBase query, IOrganizationService connection, Action<string> writeVerbose)
        {
            // Track IDs we've seen in unpublished results for deduplication
            var unpublishedIds = new HashSet<Guid>();
            int unpublishedCount = 0;

            // First yield unpublished records - these take precedence
            // Build the ID set as we stream through the results to minimize memory usage
            foreach (var entity in ExecuteQueryWithPaging(query, connection, writeVerbose, unpublished: true))
            {
                unpublishedIds.Add(entity.Id);
                unpublishedCount++;
                yield return entity;
            }
            
            writeVerbose($"Retrieved {unpublishedCount} unpublished record(s)");

            // Now get published records and filter out duplicates
            int publishedCount = 0;
            int filteredCount = 0;
            
            foreach (var entity in ExecuteQueryWithPaging(query, connection, writeVerbose, unpublished: false))
            {
                publishedCount++;
                // Only return if this ID wasn't in unpublished results
                if (!unpublishedIds.Contains(entity.Id))
                {
                    yield return entity;
                }
                else
                {
                    filteredCount++;
                }
            }

            writeVerbose($"Retrieved {publishedCount} published record(s), filtered {filteredCount} duplicate(s)");
        }
    }
}
