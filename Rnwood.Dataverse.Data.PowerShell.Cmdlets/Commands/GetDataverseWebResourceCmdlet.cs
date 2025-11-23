using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves web resources from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseWebResource")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseWebResourceCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_QUERY = "Query";
        private const string PARAMSET_ID = "Id";

        /// <summary>
        /// Gets or sets the ID of the web resource to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, HelpMessage = "ID of the web resource to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name or name pattern of the web resource to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Name or name pattern of the web resource. Supports wildcards (* and ?)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the web resource type to filter by.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Web resource type to filter by")]
        public WebResourceType? WebResourceType { get; set; }

        /// <summary>
        /// Gets or sets the display name or pattern to filter by. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Display name or pattern to filter by. Supports wildcards (* and ?)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter to only unmanaged web resources.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "If set, filters to only unmanaged web resources")]
        public SwitchParameter Unmanaged { get; set; }

        /// <summary>
        /// Gets or sets the file path to save the web resource content.
        /// </summary>
        [Parameter(HelpMessage = "File path to save the web resource content. If not specified, content is returned as a property.")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the folder path to save multiple web resource files.
        /// </summary>
        [Parameter(HelpMessage = "Folder path to save multiple web resource files. File names are based on the web resource name.")]
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the content property in the results.
        /// </summary>
        [Parameter(HelpMessage = "If set, includes the content property in the results. By default, content is excluded.")]
        public SwitchParameter IncludeContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to decode the content from base64.
        /// </summary>
        [Parameter(HelpMessage = "If set, decodes the content from base64 and returns as byte array. Implies -IncludeContent.")]
        public SwitchParameter DecodeContent { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("webresource");
            
            // Exclude content by default unless IncludeContent or DecodeContent is specified
            // DecodeContent implies IncludeContent
            bool includeContent = IncludeContent || DecodeContent;
            
            if (includeContent)
            {
                query.ColumnSet = new ColumnSet(true);
            }
            else
            {
                // Get all columns except content
                query.ColumnSet = new ColumnSet(
                    "webresourceid",
                    "name",
                    "displayname",
                    "webresourcetype",
                    "description",
                    "ismanaged",
                    "iscustomizable",
                    "canbedeleted",
                    "languagecode",
                    "dependencyxml",
                    "introducedversion",
                    "isenabledformobileclient",
                    "ishidden",
                    "createdon",
                    "modifiedon",
                    "createdby",
                    "modifiedby",
                    "organizationid",
                    "versionnumber",
                    "solutionid",
                    "componentstate",
                    "overwritetime"
                );
            }

            // Apply filters based on parameter set
            switch (ParameterSetName)
            {
                case PARAMSET_ID:
                    query.Criteria.AddCondition("webresourceid", ConditionOperator.Equal, Id);
                    break;

                case PARAMSET_QUERY:
                    if (!string.IsNullOrEmpty(Name))
                    {
                        // Check if Name contains wildcards
                        if (Name.Contains("*") || Name.Contains("?"))
                        {
                            // Convert wildcards to SQL LIKE pattern
                            string likePattern = Name.Replace("*", "%").Replace("?", "_");
                            query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                        }
                        else
                        {
                            query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                        }
                    }

                    if (WebResourceType.HasValue)
                    {
                        query.Criteria.AddCondition("webresourcetype", ConditionOperator.Equal, (int)WebResourceType.Value);
                    }

                    if (!string.IsNullOrEmpty(DisplayName))
                    {
                        // Check if DisplayName contains wildcards
                        if (DisplayName.Contains("*") || DisplayName.Contains("?"))
                        {
                            // Convert wildcards to SQL LIKE pattern
                            string likePattern = DisplayName.Replace("*", "%").Replace("?", "_");
                            query.Criteria.AddCondition("displayname", ConditionOperator.Like, likePattern);
                        }
                        else
                        {
                            query.Criteria.AddCondition("displayname", ConditionOperator.Equal, DisplayName);
                        }
                    }

                    if (Unmanaged)
                    {
                        query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    }
                    break;
            }

            // Use QueryHelpers to execute with automatic paging
            // Read unpublished by default since web resources are customizable entities
            var results = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, unpublished: true);

            // When saving to file/folder, we need content even if not requested in the output
            bool needContentForFileOperations = !string.IsNullOrEmpty(Path) || !string.IsNullOrEmpty(Folder);

            if (!string.IsNullOrEmpty(Folder))
            {
                // Save multiple web resources to folder
                if (!Directory.Exists(Folder))
                {
                    Directory.CreateDirectory(Folder);
                }

                foreach (var entity in results)
                {
                    // If content wasn't retrieved, fetch it now for saving
                    if (needContentForFileOperations && !entity.Contains("content"))
                    {
                        // Use RetrieveUnpublishedRequest to get unpublished content
                        var retrieveUnpublishedRequest = new RetrieveUnpublishedRequest
                        {
                            Target = new EntityReference("webresource", entity.Id),
                            ColumnSet = new ColumnSet("content")
                        };
                        var response = (RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                        entity["content"] = response.Entity.GetAttributeValue<string>("content");
                    }
                    SaveWebResourceToFolder(entity);
                }
            }
            else
            {
                // Stream results without buffering
                int count = 0;
                bool pathWarningShown = false;
                
                foreach (var entity in results)
                {
                    count++;
                    
                    // If content wasn't retrieved but needed for file operations, fetch it now
                    if (needContentForFileOperations && !entity.Contains("content"))
                    {
                        // Use RetrieveUnpublishedRequest to get unpublished content
                        var retrieveUnpublishedRequest = new RetrieveUnpublishedRequest
                        {
                            Target = new EntityReference("webresource", entity.Id),
                            ColumnSet = new ColumnSet("content")
                        };
                        var response = (RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                        entity["content"] = response.Entity.GetAttributeValue<string>("content");
                    }
                    
                    var psObject = ConvertEntityToPSObject(entity, includeContent);

                    // Save to file if Path is specified and we have exactly one result
                    if (!string.IsNullOrEmpty(Path))
                    {
                        if (count == 1)
                        {
                            SaveWebResourceToFile(entity, Path);
                        }
                        else if (count == 2 && !pathWarningShown)
                        {
                            WriteWarning($"Multiple web resources found. Use -Folder parameter to save multiple files. Skipping file save.");
                            pathWarningShown = true;
                        }
                    }

                    WriteObject(psObject);
                }
            }
        }

        private PSObject ConvertEntityToPSObject(Entity entity, bool includeContent)
        {
            var metadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, metadataFactory);
            
            // Determine which columns to include in the PSObject
            ColumnSet columnSet;
            if (includeContent)
            {
                columnSet = new ColumnSet(true);
            }
            else
            {
                // Use the same column list as the query (without content)
                columnSet = new ColumnSet(
                    "webresourceid",
                    "name",
                    "displayname",
                    "webresourcetype",
                    "description",
                    "ismanaged",
                    "iscustomizable",
                    "canbedeleted",
                    "languagecode",
                    "dependencyxml",
                    "introducedversion",
                    "isenabledformobileclient",
                    "ishidden",
                    "createdon",
                    "modifiedon",
                    "createdby",
                    "modifiedby",
                    "organizationid",
                    "versionnumber",
                    "solutionid",
                    "componentstate",
                    "overwritetime"
                );
            }
            
            var psObject = converter.ConvertToPSObject(entity, columnSet, _ => ValueType.Display);

            // Decode content if requested
            if (DecodeContent && entity.Contains("content"))
            {
                var contentBase64 = entity.GetAttributeValue<string>("content");
                if (!string.IsNullOrEmpty(contentBase64))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(contentBase64);
                        psObject.Properties.Remove("content");
                        psObject.Properties.Add(new PSNoteProperty("content", bytes));
                    }
                    catch (FormatException ex)
                    {
                        WriteError(new ErrorRecord(
                            ex,
                            "Base64DecodeError",
                            ErrorCategory.InvalidData,
                            entity.Id
                        ));
                    }
                }
            }

            return psObject;
        }

        private void SaveWebResourceToFile(Entity entity, string filePath)
        {
            if (!entity.Contains("content"))
            {
                WriteWarning($"Web resource does not contain content.");
                return;
            }

            var contentBase64 = entity.GetAttributeValue<string>("content");
            if (string.IsNullOrEmpty(contentBase64))
            {
                WriteWarning($"Web resource content is empty.");
                return;
            }

            try
            {
                var bytes = Convert.FromBase64String(contentBase64);
                File.WriteAllBytes(filePath, bytes);
                WriteVerbose($"Saved web resource to {filePath}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SaveFileFailed", ErrorCategory.WriteError, filePath));
            }
        }

        private void SaveWebResourceToFolder(Entity entity)
        {
            if (!entity.Contains("name") || !entity.Contains("content"))
            {
                WriteWarning($"Web resource missing name or content.");
                return;
            }

            var name = entity.GetAttributeValue<string>("name");
            
            // Sanitize filename by removing invalid characters
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var fileName = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            
            var filePath = System.IO.Path.Combine(Folder, fileName);

            SaveWebResourceToFile(entity, filePath);
        }
    }
}
