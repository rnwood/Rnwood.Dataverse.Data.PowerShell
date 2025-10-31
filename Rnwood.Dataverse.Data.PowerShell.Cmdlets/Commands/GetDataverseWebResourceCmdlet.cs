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
        private const string PARAMSET_NAME = "Name";

        /// <summary>
        /// Gets or sets the ID of the web resource to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, HelpMessage = "ID of the web resource to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the web resource to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NAME, Mandatory = true, HelpMessage = "Name of the web resource to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets filter values for querying web resources.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Hashtable to filter web resources. Keys may be column names like 'webresourcetype', 'name', etc.")]
        public Hashtable FilterValues { get; set; }

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
        /// Gets or sets a value indicating whether to decode the content from base64.
        /// </summary>
        [Parameter(HelpMessage = "If set, decodes the content from base64 and returns as byte array")]
        public SwitchParameter DecodeContent { get; set; }

        /// <summary>
        /// Gets or sets the columns to return.
        /// </summary>
        [Parameter(HelpMessage = "Array of column names to return. If not specified, returns all columns.")]
        [ArgumentCompleter(typeof(ColumnNamesArgumentCompleter))]
        public string[] Columns { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("webresource");
            query.ColumnSet = new ColumnSet(true);

            if (Columns != null && Columns.Length > 0)
            {
                query.ColumnSet = new ColumnSet(Columns);
                // Always include content, name, and webresourceid if we're writing to files
                if (!string.IsNullOrEmpty(Path) || !string.IsNullOrEmpty(Folder))
                {
                    if (!query.ColumnSet.Columns.Contains("content"))
                        query.ColumnSet.Columns.Add("content");
                    if (!query.ColumnSet.Columns.Contains("name"))
                        query.ColumnSet.Columns.Add("name");
                    if (!query.ColumnSet.Columns.Contains("webresourceid"))
                        query.ColumnSet.Columns.Add("webresourceid");
                }
            }

            // Apply filters based on parameter set
            switch (ParameterSetName)
            {
                case PARAMSET_ID:
                    query.Criteria.AddCondition("webresourceid", ConditionOperator.Equal, Id);
                    break;

                case PARAMSET_NAME:
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                    break;

                case PARAMSET_QUERY:
                    if (FilterValues != null)
                    {
                        FilterHelpers.ProcessHashFilterValues(query.Criteria, new Hashtable[] { FilterValues }, false);
                    }
                    break;
            }

            var results = Connection.RetrieveMultiple(query);

            if (!string.IsNullOrEmpty(Folder))
            {
                // Save multiple web resources to folder
                if (!Directory.Exists(Folder))
                {
                    Directory.CreateDirectory(Folder);
                }

                foreach (var entity in results.Entities)
                {
                    SaveWebResourceToFolder(entity);
                }
            }

            foreach (var entity in results.Entities)
            {
                var psObject = ConvertEntityToPSObject(entity);

                if (!string.IsNullOrEmpty(Path) && results.Entities.Count == 1)
                {
                    // Save single web resource to file
                    SaveWebResourceToFile(entity, Path);
                }

                WriteObject(psObject);
            }
        }

        private PSObject ConvertEntityToPSObject(Entity entity)
        {
            var metadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, metadataFactory);
            var psObject = converter.ConvertToPSObject(entity, new ColumnSet(true), _ => ValueType.Display);

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
                        WriteWarning($"Failed to decode content for web resource: {ex.Message}");
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
