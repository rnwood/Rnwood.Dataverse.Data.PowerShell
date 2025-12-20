using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets an app module's icon by downloading an icon from an online icon set and creating/updating a web resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseAppmoduleIconFromSet", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseAppmoduleIconFromSetCmdlet : OrganizationServiceCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the ID of the app module.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "ById", HelpMessage = "ID of the app module")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByUniqueName", HelpMessage = "Unique name of the app module")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the icon set to retrieve the icon from.
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Icon set to retrieve the icon from")]
        [ValidateSet("FluentUI", "Iconoir", "Tabler")]
        public string IconSet { get; set; } = "FluentUI";

        /// <summary>
        /// Gets or sets the name of the icon to set.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "Name of the icon to set (e.g., 'user', 'settings')")]
        public string IconName { get; set; }

        /// <summary>
        /// Gets or sets the publisher prefix to use for the web resource name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Publisher prefix to use for the web resource name")]
        public string PublisherPrefix { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the app module and web resource after updating.
        /// </summary>
        [Parameter(HelpMessage = "Publish the app module and web resource after updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Gets or sets whether to return the updated app module.
        /// </summary>
        [Parameter(HelpMessage = "Return the updated app module")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Get the app module ID
                Guid appModuleId = GetAppModuleId();

                // Download the icon
                WriteVerbose($"Downloading icon '{IconName}' from {IconSet}");
                var iconContent = DownloadIconAsync().GetAwaiter().GetResult();

                // Create web resource name
                var webResourceName = $"{PublisherPrefix}_/icons/{IconSet.Replace(" ", "_")}/{IconName.ToLower().Replace(" ", "_")}.svg";
                WriteVerbose($"Web resource name: {webResourceName}");

                // Create or update the web resource
                var webResourceId = CreateOrUpdateWebResource(webResourceName, iconContent);
                WriteVerbose($"Web resource created/updated: {webResourceId}");

                // Update the app module
                if (!ShouldProcess($"App module with ID '{appModuleId}'", $"Set webresourceid to '{webResourceId}'"))
                {
                    return;
                }

                UpdateAppModuleIcon(appModuleId, webResourceId);

                // Publish if requested
                if (Publish && ShouldProcess($"App module with ID '{appModuleId}' and web resource '{webResourceName}'", "Publish"))
                {
                    PublishChanges(appModuleId, webResourceId);
                }

                // Return updated app module if PassThru specified
                if (PassThru)
                {
                    var appModule = Connection.Retrieve("appmodule", appModuleId, new ColumnSet(true));
                    var result = ConvertAppModuleToPSObject(appModule);
                    WriteObject(result);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SetAppModuleIconError",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }

        private Guid GetAppModuleId()
        {
            if (ParameterSetName == "ById")
            {
                return Id;
            }

            // Get by UniqueName
            WriteVerbose($"Looking up app module by UniqueName: {UniqueName}");

            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("appmoduleid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);

            var results = Connection.RetrieveMultiple(query);
            
            if (results.Entities.Count == 0)
            {
                // Try to retrieve unpublished version
                var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                {
                    Query = query
                };
                var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)Connection.Execute(retrieveUnpublishedMultipleRequest);
                results = unpublishedResponse.EntityCollection;
            }

            if (results.Entities.Count == 0)
            {
                throw new ArgumentException($"App module with UniqueName '{UniqueName}' not found");
            }

            if (results.Entities.Count > 1)
            {
                throw new ArgumentException($"Multiple app modules found with UniqueName '{UniqueName}'");
            }

            return results.Entities[0].Id;
        }

        private async Task<string> DownloadIconAsync()
        {
            // Use shared helper to get consistent download URL across cmdlets
            var downloadUrl = IconSetUrlHelper.GetIconDownloadUrl(IconSet, IconName);

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

        private void UpdateAppModuleIcon(Guid appModuleId, Guid webResourceId)
        {
            WriteVerbose($"Updating app module '{appModuleId}' webresourceid to '{webResourceId}'");

            // Retrieve existing app module to check current value
            Entity existingAppModule;
            try
            {
                existingAppModule = Connection.Retrieve("appmodule", appModuleId, new ColumnSet("webresourceid"));
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (QueryHelpers.IsNotFoundException(ex))
                {
                    // Try to retrieve unpublished version
                    var retrieveUnpublishedRequest = new RetrieveUnpublishedRequest
                    {
                        Target = new EntityReference("appmodule", appModuleId),
                        ColumnSet = new ColumnSet("webresourceid")
                    };
                    var response = (RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                    existingAppModule = response.Entity;
                }
                else
                {
                    throw;
                }
            }

            // Update app module
            var updateEntity = new Entity("appmodule", appModuleId);
            updateEntity["webresourceid"] = webResourceId;

            Connection.Update(updateEntity);

            WriteVerbose($"App module icon updated successfully");
        }

        private void PublishChanges(Guid appModuleId, Guid webResourceId)
        {
            WriteVerbose($"Publishing app module '{appModuleId}' and web resource '{webResourceId}'");

            // Publish both app module and web resource
            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $@"<importexportxml>
                    <appmodules>
                        <appmodule>{{{appModuleId}}}</appmodule>
                    </appmodules>
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

        private PSObject ConvertAppModuleToPSObject(Entity appModule)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("Id", appModule.Id));
            result.Properties.Add(new PSNoteProperty("UniqueName", appModule.GetAttributeValue<string>("uniquename")));
            result.Properties.Add(new PSNoteProperty("Name", appModule.GetAttributeValue<string>("name")));
            result.Properties.Add(new PSNoteProperty("WebResourceId", appModule.GetAttributeValue<Guid?>("webresourceid")));
            return result;
        }
    }
}
