using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a script library from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormLibrary", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormLibraryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the library name (web resource name).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName", HelpMessage = "Name of the library to remove")]
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets the library unique ID.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByUniqueId", HelpMessage = "Unique ID of the library to remove")]
        public Guid LibraryUniqueId { get; set; }

        /// <summary>
        /// Gets or sets whether to skip publishing the form after removal.
        /// </summary>
        [Parameter(HelpMessage = "Skip publishing the form after removal")]
        public SwitchParameter SkipPublish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, formElement) = FormXmlHelper.ParseFormXml(form);

            // Get formLibraries element
            XElement formLibrariesElement = formElement.Element("formLibraries");
            
            if (formLibrariesElement == null || !formLibrariesElement.Elements("Library").Any())
            {
                string identifier = ParameterSetName == "ByName" 
                    ? $"Library '{LibraryName}'" 
                    : $"Library with unique ID '{LibraryUniqueId}'";
                throw new InvalidOperationException($"{identifier} not found - form has no libraries");
            }

            // Find the library to remove
            XElement libraryToRemove;
            if (ParameterSetName == "ByName")
            {
                libraryToRemove = formLibrariesElement.Elements("Library")
                    .FirstOrDefault(l => string.Equals(l.Attribute("name")?.Value, LibraryName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                libraryToRemove = formLibrariesElement.Elements("Library")
                    .FirstOrDefault(l =>
                    {
                        string uniqueIdStr = l.Attribute("libraryUniqueId")?.Value?.Trim('{', '}');
                        return Guid.TryParse(uniqueIdStr, out Guid id) && id == LibraryUniqueId;
                    });
            }

            if (libraryToRemove == null)
            {
                string identifier = ParameterSetName == "ByName" 
                    ? $"Library '{LibraryName}'" 
                    : $"Library with unique ID '{LibraryUniqueId}'";
                throw new InvalidOperationException($"{identifier} not found in form");
            }

            string libraryName = libraryToRemove.Attribute("name")?.Value;
            if (ShouldProcess($"Form '{FormId}'", $"Remove library '{libraryName}'"))
            {
                libraryToRemove.Remove();
                
                // If no more libraries, remove the formLibraries element
                if (!formLibrariesElement.Elements("Library").Any())
                {
                    formLibrariesElement.Remove();
                    WriteVerbose("Removed empty formLibraries element");
                }

                WriteVerbose($"Removed library '{libraryName}'");

                // Update the form
                FormXmlHelper.UpdateFormXml(Connection, FormId, doc);

                // Publish if requested
                if (!SkipPublish.IsPresent)
                {
                    string entityName = form.GetAttributeValue<string>("objecttypecode");
                    WriteVerbose($"Publishing entity '{entityName}'");
                    FormXmlHelper.PublishEntity(Connection, entityName);
                }
            }
        }
    }
}
