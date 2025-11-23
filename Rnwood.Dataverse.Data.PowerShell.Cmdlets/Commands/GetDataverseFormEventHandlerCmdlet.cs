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
    /// Retrieves event handlers from a Dataverse form (form-level or control-level events).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormEventHandler")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormEventHandlerCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the event name to filter results (e.g., onload, onsave, onchange).
        /// </summary>
        [Parameter(HelpMessage = "Name of the event (e.g., onload, onsave, onchange)")]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for control-level events.
        /// </summary>
        [Parameter(HelpMessage = "Control ID for control-level events")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the tab name containing the control.
        /// </summary>
        [Parameter(HelpMessage = "Tab name containing the control (required when ControlId is specified)")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the section name containing the control.
        /// </summary>
        [Parameter(HelpMessage = "Section name containing the control (required when ControlId is specified)")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the handler unique ID to retrieve a specific handler.
        /// </summary>
        [Parameter(HelpMessage = "Unique ID of a specific handler to retrieve")]
        public Guid? HandlerUniqueId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate parameters
            if (!string.IsNullOrEmpty(ControlId) && (string.IsNullOrEmpty(TabName) || string.IsNullOrEmpty(SectionName)))
            {
                throw new ArgumentException("TabName and SectionName are required when ControlId is specified");
            }

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, formElement) = FormXmlHelper.ParseFormXml(form);

            bool foundAny = false;

            // Get control-level events if ControlId is specified
            if (!string.IsNullOrEmpty(ControlId))
            {
                foundAny = GetControlEvents(formElement);
            }
            else
            {
                // Get form-level events
                foundAny = GetFormEvents(formElement);
            }

            // If a specific handler unique ID was requested and not found, throw an error
            // But if just filtering by event name, return empty results instead of throwing
            if (!foundAny && HandlerUniqueId.HasValue)
            {
                string identifier = $"Handler with unique ID '{HandlerUniqueId}'";
                string location = !string.IsNullOrEmpty(ControlId) 
                    ? $"control '{ControlId}'" 
                    : "form";
                throw new InvalidOperationException($"{identifier} not found in {location}");
            }
            else if (!foundAny && !string.IsNullOrEmpty(EventName))
            {
                // Event name specified but not found - just return empty (don't throw)
                // This allows consumers to check if an event exists without catching exceptions
                WriteVerbose($"Event '{EventName}' not found - returning empty results");
            }
        }

        /// <summary>
        /// Gets form-level events.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        /// <returns>True if any events were found, false otherwise.</returns>
        private bool GetFormEvents(XElement formElement)
        {
            XElement eventsElement = formElement.Element("events");
            
            if (eventsElement == null || !eventsElement.Elements("event").Any())
            {
                WriteVerbose($"No form-level events found in form '{FormId}'");
                return false;
            }

            bool foundAny = false;
            foreach (var eventElement in eventsElement.Elements("event"))
            {
                string eventName = eventElement.Attribute("name")?.Value;
                
                // Apply event name filter
                if (!string.IsNullOrEmpty(EventName) && !string.Equals(eventName, EventName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Get handlers
                var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                foreach (var handlersElement in handlersElements)
                {
                    foreach (var handler in handlersElement.Elements("Handler"))
                    {
                        PSObject handlerObj = ParseHandler(handler, eventName, null, null, null);
                        if (handlerObj != null)
                        {
                            foundAny = true;
                            WriteObject(handlerObj);
                        }
                    }
                }
            }

            return foundAny;
        }

        /// <summary>
        /// Gets control-level events.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        /// <returns>True if any events were found, false otherwise.</returns>
        private bool GetControlEvents(XElement formElement)
        {
            // Find the control
            var (control, parentCell) = FormXmlHelper.FindControlById(formElement, TabName, SectionName, ControlId);
            
            if (control == null)
            {
                throw new InvalidOperationException($"Control '{ControlId}' not found in section '{SectionName}' of tab '{TabName}'");
            }

            XElement eventsElement = control.Element("events");
            
            if (eventsElement == null || !eventsElement.Elements("event").Any())
            {
                WriteVerbose($"No events found for control '{ControlId}'");
                return false;
            }

            bool foundAny = false;
            foreach (var eventElement in eventsElement.Elements("event"))
            {
                string eventName = eventElement.Attribute("name")?.Value;
                
                // Apply event name filter
                if (!string.IsNullOrEmpty(EventName) && !string.Equals(eventName, EventName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Get handlers
                var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                foreach (var handlersElement in handlersElements)
                {
                    foreach (var handler in handlersElement.Elements("Handler"))
                    {
                        PSObject handlerObj = ParseHandler(handler, eventName, ControlId, TabName, SectionName);
                        if (handlerObj != null)
                        {
                            foundAny = true;
                            WriteObject(handlerObj);
                        }
                    }
                }
            }

            return foundAny;
        }

        /// <summary>
        /// Parses a handler element into a PSObject.
        /// </summary>
        /// <param name="handler">The handler XML element.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="controlId">The control ID (null for form-level events).</param>
        /// <param name="tabName">The tab name (null for form-level events).</param>
        /// <param name="sectionName">The section name (null for form-level events).</param>
        /// <returns>A PSObject representing the handler, or null if filtered out.</returns>
        private PSObject ParseHandler(XElement handler, string eventName, string controlId, string tabName, string sectionName)
        {
            string functionName = handler.Attribute("functionName")?.Value;
            string libraryName = handler.Attribute("libraryName")?.Value;
            string uniqueIdStr = handler.Attribute("handlerUniqueId")?.Value;
            
            // Parse unique ID
            Guid? uniqueId = null;
            if (!string.IsNullOrEmpty(uniqueIdStr))
            {
                uniqueIdStr = uniqueIdStr.Trim('{', '}');
                if (Guid.TryParse(uniqueIdStr, out Guid parsedId))
                {
                    uniqueId = parsedId;
                }
            }

            // Apply handler unique ID filter
            if (HandlerUniqueId.HasValue && uniqueId != HandlerUniqueId.Value)
                return null;

            PSObject handlerObj = new PSObject();
            handlerObj.Properties.Add(new PSNoteProperty("FormId", FormId));
            handlerObj.Properties.Add(new PSNoteProperty("EventName", eventName));
            handlerObj.Properties.Add(new PSNoteProperty("ControlId", controlId));
            handlerObj.Properties.Add(new PSNoteProperty("TabName", tabName));
            handlerObj.Properties.Add(new PSNoteProperty("SectionName", sectionName));
            handlerObj.Properties.Add(new PSNoteProperty("FunctionName", functionName));
            handlerObj.Properties.Add(new PSNoteProperty("LibraryName", libraryName));
            handlerObj.Properties.Add(new PSNoteProperty("HandlerUniqueId", uniqueId));
            handlerObj.Properties.Add(new PSNoteProperty("Enabled", handler.Attribute("enabled")?.Value != "false"));
            handlerObj.Properties.Add(new PSNoteProperty("Parameters", handler.Attribute("parameters")?.Value));
            handlerObj.Properties.Add(new PSNoteProperty("PassExecutionContext", handler.Attribute("passExecutionContext")?.Value == "true"));

            return handlerObj;
        }
    }
}
