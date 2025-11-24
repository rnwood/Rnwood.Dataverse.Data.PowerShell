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
    /// Removes an event handler from a Dataverse form (form-level, attribute-level, tab-level, or control-level).
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormEventHandler", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "FormEventByUniqueId")]
    public class RemoveDataverseFormEventHandlerCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the event name (e.g., onload, onsave, onchange, tabstatechange).
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the event (e.g., onload, onsave, onchange, tabstatechange)")]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the handler unique ID to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FormEventByUniqueId", HelpMessage = "Unique ID of the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "AttributeEventByUniqueId", HelpMessage = "Unique ID of the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "TabEventByUniqueId", HelpMessage = "Unique ID of the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByUniqueId", HelpMessage = "Unique ID of the handler to remove")]
        public Guid HandlerUniqueId { get; set; }

        /// <summary>
        /// Gets or sets the function name to identify the handler to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FormEventByFunction", HelpMessage = "Function name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "AttributeEventByFunction", HelpMessage = "Function name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "TabEventByFunction", HelpMessage = "Function name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByFunction", HelpMessage = "Function name to identify the handler to remove")]
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets or sets the library name to identify the handler to remove (required with FunctionName).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FormEventByFunction", HelpMessage = "Library name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "AttributeEventByFunction", HelpMessage = "Library name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "TabEventByFunction", HelpMessage = "Library name to identify the handler to remove")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByFunction", HelpMessage = "Library name to identify the handler to remove")]
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets the attribute name for attribute-level events.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "AttributeEventByUniqueId", HelpMessage = "Attribute name for attribute-level events")]
        [Parameter(Mandatory = true, ParameterSetName = "AttributeEventByFunction", HelpMessage = "Attribute name for attribute-level events")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the tab name for tab-level or control-level events.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "TabEventByUniqueId", HelpMessage = "Tab name for tab-level events")]
        [Parameter(Mandatory = true, ParameterSetName = "TabEventByFunction", HelpMessage = "Tab name for tab-level events")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByUniqueId", HelpMessage = "Tab name containing the control")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByFunction", HelpMessage = "Tab name containing the control")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for control-level events.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByUniqueId", HelpMessage = "Control ID for control-level events")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByFunction", HelpMessage = "Control ID for control-level events")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the section name containing the control.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByUniqueId", HelpMessage = "Section name containing the control")]
        [Parameter(Mandatory = true, ParameterSetName = "ControlEventByFunction", HelpMessage = "Section name containing the control")]
        public string SectionName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, formElement) = FormXmlHelper.ParseFormXml(form);

            // Determine event type based on parameter set
            bool isControlEvent = ParameterSetName.Contains("ControlEvent");
            bool isTabEvent = ParameterSetName.Contains("TabEvent");
            bool isAttributeEvent = ParameterSetName.Contains("AttributeEvent");
            
            string location = isControlEvent 
                ? $"control '{ControlId}' in section '{SectionName}' of tab '{TabName}'" 
                : isTabEvent
                ? $"tab '{TabName}'"
                : isAttributeEvent
                ? $"attribute '{AttributeName}'"
                : $"form";

            if (ShouldProcess($"Form '{FormId}'", $"Remove event handler from '{EventName}' on {location}"))
            {
                if (isControlEvent)
                {
                    RemoveControlEventHandler(formElement);
                }
                else if (isTabEvent)
                {
                    RemoveTabEventHandler(formElement);
                }
                else if (isAttributeEvent)
                {
                    RemoveAttributeEventHandler(formElement);
                }
                else
                {
                    RemoveFormEventHandler(formElement);
                }

                // Update the form
                FormXmlHelper.UpdateFormXml(Connection, FormId, doc);
            }
        }

        /// <summary>
        /// Removes a form-level event handler.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        private void RemoveFormEventHandler(XElement formElement)
        {
            XElement eventsElement = formElement.Element("events");
            
            if (eventsElement == null)
            {
                throw new InvalidOperationException($"No events found in form");
            }

            XElement eventElement = eventsElement.Elements("event")
                .FirstOrDefault(e => string.Equals(e.Attribute("name")?.Value, EventName, StringComparison.OrdinalIgnoreCase));

            if (eventElement == null)
            {
                throw new InvalidOperationException($"Event '{EventName}' not found in form");
            }

            RemoveHandler(eventElement);

            // Clean up empty elements
            CleanupEmptyEventElements(eventElement, eventsElement, formElement);
        }

        /// <summary>
        /// Removes an attribute-level event handler.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        private void RemoveAttributeEventHandler(XElement formElement)
        {
            XElement eventsElement = formElement.Element("events");
            
            if (eventsElement == null)
            {
                throw new InvalidOperationException($"No events found in form");
            }

            XElement eventElement = eventsElement.Elements("event")
                .FirstOrDefault(e => 
                    string.Equals(e.Attribute("name")?.Value, EventName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.Attribute("attribute")?.Value, AttributeName, StringComparison.OrdinalIgnoreCase));

            if (eventElement == null)
            {
                throw new InvalidOperationException($"Event '{EventName}' for attribute '{AttributeName}' not found in form");
            }

            RemoveHandler(eventElement);

            // Clean up empty elements
            CleanupEmptyEventElements(eventElement, eventsElement, formElement);
        }

        /// <summary>
        /// Removes a tab-level event handler.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        private void RemoveTabEventHandler(XElement formElement)
        {
            // Find the tab
            var tab = FormXmlHelper.FindTab(formElement, tabName: TabName);
            
            if (tab == null)
            {
                throw new InvalidOperationException($"Tab '{TabName}' not found");
            }

            XElement eventsElement = tab.Element("events");
            
            if (eventsElement == null)
            {
                throw new InvalidOperationException($"No events found for tab '{TabName}'");
            }

            XElement eventElement = eventsElement.Elements("event")
                .FirstOrDefault(e => string.Equals(e.Attribute("name")?.Value, EventName, StringComparison.OrdinalIgnoreCase));

            if (eventElement == null)
            {
                throw new InvalidOperationException($"Event '{EventName}' not found for tab '{TabName}'");
            }

            RemoveHandler(eventElement);

            // Clean up empty elements
            CleanupEmptyEventElements(eventElement, eventsElement, tab);
        }

        /// <summary>
        /// Removes a control-level event handler.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        private void RemoveControlEventHandler(XElement formElement)
        {
            // Find the control
            var (control, parentCell) = FormXmlHelper.FindControlById(formElement, TabName, SectionName, ControlId);
            
            if (control == null)
            {
                throw new InvalidOperationException($"Control '{ControlId}' not found in section '{SectionName}' of tab '{TabName}'");
            }

            XElement eventsElement = control.Element("events");
            
            if (eventsElement == null)
            {
                throw new InvalidOperationException($"No events found for control '{ControlId}'");
            }

            XElement eventElement = eventsElement.Elements("event")
                .FirstOrDefault(e => string.Equals(e.Attribute("name")?.Value, EventName, StringComparison.OrdinalIgnoreCase));

            if (eventElement == null)
            {
                throw new InvalidOperationException($"Event '{EventName}' not found for control '{ControlId}'");
            }

            RemoveHandler(eventElement);

            // Clean up empty elements
            CleanupEmptyEventElements(eventElement, eventsElement, control);
        }

        /// <summary>
        /// Removes a handler from an event element.
        /// </summary>
        /// <param name="eventElement">The event XML element.</param>
        private void RemoveHandler(XElement eventElement)
        {
            // Get all handler containers (Handlers and InternalHandlers)
            var handlerContainers = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers")).ToList();
            
            bool found = false;
            foreach (var container in handlerContainers)
            {
                XElement handlerToRemove;
                
                if (ParameterSetName.Contains("ByUniqueId"))
                {
                    handlerToRemove = container.Elements("Handler")
                        .FirstOrDefault(h =>
                        {
                            string uniqueIdStr = h.Attribute("handlerUniqueId")?.Value?.Trim('{', '}');
                            return Guid.TryParse(uniqueIdStr, out Guid id) && id == HandlerUniqueId;
                        });
                }
                else // ByFunction
                {
                    handlerToRemove = container.Elements("Handler")
                        .FirstOrDefault(h => 
                            string.Equals(h.Attribute("functionName")?.Value, FunctionName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(h.Attribute("libraryName")?.Value, LibraryName, StringComparison.OrdinalIgnoreCase));
                }

                if (handlerToRemove != null)
                {
                    string functionName = handlerToRemove.Attribute("functionName")?.Value;
                    handlerToRemove.Remove();
                    WriteVerbose($"Removed handler for function '{functionName}'");
                    found = true;

                    // Remove empty container
                    if (!container.Elements("Handler").Any())
                    {
                        container.Remove();
                    }

                    break;
                }
            }

            if (!found)
            {
                string identifier = ParameterSetName.Contains("ByUniqueId") 
                    ? $"Handler with unique ID '{HandlerUniqueId}'" 
                    : $"Handler for function '{FunctionName}' in library '{LibraryName}'";
                throw new InvalidOperationException($"{identifier} not found in event '{EventName}'");
            }
        }

        /// <summary>
        /// Cleans up empty event elements after handler removal.
        /// </summary>
        /// <param name="eventElement">The event XML element.</param>
        /// <param name="eventsElement">The events container XML element.</param>
        /// <param name="parentElement">The parent element (form or control).</param>
        private void CleanupEmptyEventElements(XElement eventElement, XElement eventsElement, XElement parentElement)
        {
            // If event has no more handlers, remove it
            if (!eventElement.Elements("Handlers").Any() && !eventElement.Elements("InternalHandlers").Any())
            {
                eventElement.Remove();
                WriteVerbose($"Removed empty event '{EventName}'");
            }

            // If events element has no more events, remove it
            if (!eventsElement.Elements("event").Any())
            {
                eventsElement.Remove();
                WriteVerbose("Removed empty events element");
            }
        }
    }
}
