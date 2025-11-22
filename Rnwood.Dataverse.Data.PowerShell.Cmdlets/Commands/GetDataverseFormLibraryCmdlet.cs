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
    /// Retrieves script libraries from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormLibrary")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormLibraryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the library name to filter results.
        /// </summary>
        [Parameter(HelpMessage = "Name of the library to retrieve (web resource name)")]
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets the library unique ID to retrieve a specific library.
        /// </summary>
        [Parameter(HelpMessage = "Unique ID of a specific library to retrieve")]
        public Guid? LibraryUniqueId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, formElement) = FormXmlHelper.ParseFormXml(form);

            // Get formLibraries element
            XElement formLibrariesElement = formElement.Element("formLibraries");
            
            if (formLibrariesElement == null || !formLibrariesElement.Elements("Library").Any())
            {
                // No libraries found - don't throw error, just return nothing
                WriteVerbose($"No script libraries found in form '{FormId}'");
                return;
            }

            bool foundAny = false;
            foreach (var library in formLibrariesElement.Elements("Library"))
            {
                string name = library.Attribute("name")?.Value;
                string uniqueIdStr = library.Attribute("libraryUniqueId")?.Value;
                
                // Parse unique ID
                Guid? uniqueId = null;
                if (!string.IsNullOrEmpty(uniqueIdStr))
                {
                    // Handle both {guid} and guid formats
                    uniqueIdStr = uniqueIdStr.Trim('{', '}');
                    if (Guid.TryParse(uniqueIdStr, out Guid parsedId))
                    {
                        uniqueId = parsedId;
                    }
                }

                // Apply filters
                if (!string.IsNullOrEmpty(LibraryName) && !string.Equals(name, LibraryName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (LibraryUniqueId.HasValue && uniqueId != LibraryUniqueId.Value)
                    continue;

                foundAny = true;

                PSObject libraryObj = new PSObject();
                libraryObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                libraryObj.Properties.Add(new PSNoteProperty("Name", name));
                libraryObj.Properties.Add(new PSNoteProperty("LibraryUniqueId", uniqueId));

                WriteObject(libraryObj);
            }

            // If a specific library was requested and not found, throw an error
            if (!foundAny && (!string.IsNullOrEmpty(LibraryName) || LibraryUniqueId.HasValue))
            {
                string identifier = !string.IsNullOrEmpty(LibraryName) 
                    ? $"Library '{LibraryName}'" 
                    : $"Library with unique ID '{LibraryUniqueId}'";
                throw new InvalidOperationException($"{identifier} not found in form");
            }
        }
    }
}
