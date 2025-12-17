using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets a table's vector icon by downloading an icon from an online icon set and creating/updating a web resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseTableIcon", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseTableIconCmdlet : OrganizationServiceCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the logical name of the entity (table).
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the icon set to retrieve the icon from.
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Icon set to retrieve the icon from")]
        [ValidateSet("Iconoir")]
        public string IconSet { get; set; } = "Iconoir";

        /// <summary>
        /// Gets or sets the name of the icon to set.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "Name of the icon to set (e.g., 'user', 'settings')")]
        public string IconName { get; set; }

        /// <summary>
        /// Gets or sets the publisher prefix to use for the web resource name.
        /// </summary>
        [Parameter(HelpMessage = "Publisher prefix to use for the web resource name (defaults to active publisher's prefix)")]
        public string PublisherPrefix { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the entity and web resource after updating.
        /// </summary>
        [Parameter(HelpMessage = "Publish the entity and web resource after updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Gets or sets whether to return the updated entity metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the updated entity metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Get the publisher prefix if not specified
                if (string.IsNullOrWhiteSpace(PublisherPrefix))
                {
                    PublisherPrefix = GetActivePublisherPrefix();
                    WriteVerbose($"Using active publisher prefix: {PublisherPrefix}");
                }

                // Download the icon
                WriteVerbose($"Downloading icon '{IconName}' from {IconSet}");
                var iconContent = DownloadIconAsync().GetAwaiter().GetResult();

                // Create web resource name
                var webResourceName = $"{PublisherPrefix}_/icons/{IconName}.svg";
                WriteVerbose($"Web resource name: {webResourceName}");

                // Create or update the web resource
                var webResourceId = CreateOrUpdateWebResource(webResourceName, iconContent);
                WriteVerbose($"Web resource created/updated: {webResourceId}");

                // Update the entity metadata
                if (!ShouldProcess($"Entity '{EntityName}'", $"Set IconVectorName to '{webResourceName}'"))
                {
                    return;
                }

                UpdateEntityIcon(webResourceName);

                // Publish if requested
                if (Publish && ShouldProcess($"Entity '{EntityName}' and web resource '{webResourceName}'", "Publish"))
                {
                    PublishChanges(webResourceId);
                }

                // Return updated entity metadata if PassThru specified
                if (PassThru)
                {
                    var retrieveRequest = new RetrieveEntityRequest
                    {
                        LogicalName = EntityName,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };

                    var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
                    var result = ConvertEntityMetadataToPSObject(retrieveResponse.EntityMetadata);
                    WriteObject(result);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SetTableIconError",
                    ErrorCategory.InvalidOperation,
                    EntityName));
            }
        }

        private async Task<string> DownloadIconAsync()
        {
            string downloadUrl;

            if (IconSet == "Iconoir")
            {
                // Iconoir icons are at: https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular/{name}.svg
                downloadUrl = $"https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular/{IconName}.svg";
            }
            else
            {
                throw new NotSupportedException($"Icon set '{IconSet}' is not supported");
            }

            WriteVerbose($"Downloading from: {downloadUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            request.Headers.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new ArgumentException($"Icon '{IconName}' not found in {IconSet} icon set. Use Get-DataverseIconSetIcon to browse available icons.");
                }
                response.EnsureSuccessStatusCode();
            }

            var svgContent = await response.Content.ReadAsStringAsync();
            return svgContent;
        }

        private Guid CreateOrUpdateWebResource(string name, string content)
        {
            // Check if web resource exists
            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("webresourceid", "name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, name)
                    }
                }
            };

            var results = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose).ToList();
            var existingWebResource = results.FirstOrDefault();

            // Convert SVG content to base64
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var contentBase64 = Convert.ToBase64String(contentBytes);

            if (existingWebResource != null)
            {
                // Update existing web resource
                var webResourceId = existingWebResource.Id;
                WriteVerbose($"Updating existing web resource: {webResourceId}");

                var updateEntity = new Entity("webresource", webResourceId);
                updateEntity["content"] = contentBase64;

                Connection.Update(updateEntity);

                return webResourceId;
            }
            else
            {
                // Create new web resource
                WriteVerbose($"Creating new web resource: {name}");

                var newWebResource = new Entity("webresource");
                newWebResource["name"] = name;
                newWebResource["displayname"] = $"Icon: {IconName}";
                newWebResource["description"] = $"Vector icon from {IconSet} icon set";
                newWebResource["webresourcetype"] = new OptionSetValue(11); // 11 = SVG
                newWebResource["content"] = contentBase64;

                var webResourceId = Connection.Create(newWebResource);
                return webResourceId;
            }
        }

        private void UpdateEntityIcon(string webResourceName)
        {
            WriteVerbose($"Updating entity '{EntityName}' IconVectorName to '{webResourceName}'");

            // Retrieve existing entity metadata
            var retrieveRequest = new RetrieveEntityRequest
            {
                LogicalName = EntityName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
            var existingEntity = retrieveResponse.EntityMetadata;

            // Update entity metadata
            var entityToUpdate = new EntityMetadata
            {
                MetadataId = existingEntity.MetadataId,
                LogicalName = existingEntity.LogicalName,
                IconVectorName = webResourceName
            };

            var updateRequest = new UpdateEntityRequest
            {
                Entity = entityToUpdate,
                MergeLabels = true
            };

            Connection.Execute(updateRequest);

            WriteVerbose($"Entity icon updated successfully");

            // Invalidate cache for this entity
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
            }
        }

        private void PublishChanges(Guid webResourceId)
        {
            WriteVerbose($"Publishing entity '{EntityName}' and web resource");

            // Publish both entity and web resource
            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $@"<importexportxml>
                    <entities>
                        <entity>{EntityName}</entity>
                    </entities>
                    <webresources>
                        <webresource>{{{webResourceId}}}</webresource>
                    </webresources>
                </importexportxml>"
            };

            Connection.Execute(publishRequest);
            WriteVerbose($"Published successfully");

            // Wait for publish to complete
            PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose);
        }

        private string GetActivePublisherPrefix()
        {
            // Query for the active publisher
            // First get the organization's default publisher
            var whoAmIRequest = new OrganizationRequest("WhoAmI");
            var whoAmIResponse = Connection.Execute(whoAmIRequest);
            var organizationId = (Guid)whoAmIResponse["OrganizationId"];

            var retrieveOrgRequest = new RetrieveRequest
            {
                Target = new EntityReference("organization", organizationId),
                ColumnSet = new ColumnSet("defaultpublisherid")
            };

            var retrieveOrgResponse = (RetrieveResponse)Connection.Execute(retrieveOrgRequest);
            var organization = retrieveOrgResponse.Entity;

            if (organization.Contains("defaultpublisherid"))
            {
                var publisherId = organization.GetAttributeValue<EntityReference>("defaultpublisherid").Id;

                // Retrieve the publisher prefix
                var retrievePublisherRequest = new RetrieveRequest
                {
                    Target = new EntityReference("publisher", publisherId),
                    ColumnSet = new ColumnSet("customizationprefix")
                };

                var retrievePublisherResponse = (RetrieveResponse)Connection.Execute(retrievePublisherRequest);
                var publisher = retrievePublisherResponse.Entity;

                if (publisher.Contains("customizationprefix"))
                {
                    return publisher.GetAttributeValue<string>("customizationprefix");
                }
            }

            // Fallback to "new" if we can't determine the publisher prefix
            WriteWarning("Could not determine active publisher prefix, using 'new' as fallback");
            return "new";
        }

        private PSObject ConvertEntityMetadataToPSObject(EntityMetadata metadata)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("LogicalName", metadata.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", metadata.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", metadata.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("IconVectorName", metadata.IconVectorName));
            result.Properties.Add(new PSNoteProperty("MetadataId", metadata.MetadataId));
            return result;
        }
    }
}
