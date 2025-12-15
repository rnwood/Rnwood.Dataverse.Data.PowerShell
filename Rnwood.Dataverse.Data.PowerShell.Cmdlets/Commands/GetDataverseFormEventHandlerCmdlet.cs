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
    /// Retrieves event handlers from a Dataverse form (form-level, attribute-level, tab-level, or control-level events).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormEventHandler", DefaultParameterSetName = "FormEvent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormEventHandlerCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the event name to filter results (e.g., onload, onsave, onchange, tabstatechange).
        /// </summary>
        [Parameter(HelpMessage = "Name of the event (e.g., onload, onsave, onchange, tabstatechange)")]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the attribute name for attribute-level events.
        /// </summary>
        [Parameter(ParameterSetName = "AttributeEvent", Mandatory = true, HelpMessage = "Attribute name for attribute-level events")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the tab name for tab-level or control-level events.
        /// </summary>
        [Parameter(ParameterSetName = "TabEvent", Mandatory = true, HelpMessage = "Tab name for tab-level events")]
        [Parameter(ParameterSetName = "ControlEvent", Mandatory = true, HelpMessage = "Tab name containing the control")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for control-level events.
        /// </summary>
        [Parameter(ParameterSetName = "ControlEvent", Mandatory = true, HelpMessage = "Control ID for control-level events")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the section name containing the control.
        /// </summary>
        [Parameter(ParameterSetName = "ControlEvent", Mandatory = true, HelpMessage = "Section name containing the control")]
        public string SectionName { get; set; }
        
        /// <summary>
        /// Gets or sets whether to list all event handlers from all locations (form, attribute, tab, and control levels).
        /// When this switch is used, all other location parameters are ignored.
        /// </summary>
        [Parameter(ParameterSetName = "AllEvents", HelpMessage = "List all event handlers from all locations")]
        public SwitchParameter All { get; set; }

        /// <summary>
        /// Gets or sets the handler unique ID to retrieve a specific handler.
        /// </summary>
        [Parameter(HelpMessage = "Unique ID of a specific handler to retrieve")]
        public Guid? HandlerUniqueId { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve only the published version of the form.
        /// By default, the unpublished version is retrieved.
        /// </summary>
        [Parameter(HelpMessage = "Retrieve only the published version of the form (default is unpublished)")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form = Published.IsPresent 
                ? Connection.Retrieve("systemform", FormId, new ColumnSet("formxml"))
                : FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, formElement) = FormXmlHelper.ParseFormXml(form);

            bool foundAny = false;

            // If no specific location parameters provided or All switch used, list all event handlers
            if (ParameterSetName == "FormEvent" && string.IsNullOrEmpty(EventName) && !HandlerUniqueId.HasValue)
            {
                // List all handlers from all locations
                WriteVerbose("Listing all event handlers from all locations");
                foundAny = GetAllEvents(formElement);
            }
            // Determine which event location to query based on parameter set
            else if (ParameterSetName == "ControlEvent")
            {
                foundAny = GetControlEvents(formElement);
            }
            else if (ParameterSetName == "TabEvent")
            {
                foundAny = GetTabEvents(formElement);
            }
            else if (ParameterSetName == "AttributeEvent")
            {
                foundAny = GetAttributeEvents(formElement);
            }
            else // FormEvent with EventName or HandlerUniqueId
            {
                foundAny = GetFormEvents(formElement);
            }

            // If a specific handler unique ID was requested and not found, throw an error
            // But if just filtering by event name, return empty results instead of throwing
            if (!foundAny && HandlerUniqueId.HasValue)
            {
                string location;
                if (ParameterSetName == "ControlEvent")
                    location = $"control '{ControlId}'";
                else if (ParameterSetName == "TabEvent")
                    location = $"tab '{TabName}'";
                else if (ParameterSetName == "AttributeEvent")
                    location = $"attribute '{AttributeName}'";
                else
                    location = "form";
                    
                throw new InvalidOperationException($"Handler with unique ID '{HandlerUniqueId}' not found in {location}");
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
                string attributeName = eventElement.Attribute("attribute")?.Value;
                
                // Skip attribute-level events (they have an attribute property)
                if (!string.IsNullOrEmpty(attributeName))
                    continue;
                
                // Apply event name filter
                if (!string.IsNullOrEmpty(EventName) && !string.Equals(eventName, EventName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Get handlers
                var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                foreach (var handlersElement in handlersElements)
                {
                    foreach (var handler in handlersElement.Elements("Handler"))
                    {
                        PSObject handlerObj = ParseHandler(handler, eventName, null, null, null, null);
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
        /// Gets attribute-level events.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        /// <returns>True if any events were found, false otherwise.</returns>
        private bool GetAttributeEvents(XElement formElement)
        {
            XElement eventsElement = formElement.Element("events");
            
            if (eventsElement == null || !eventsElement.Elements("event").Any())
            {
                WriteVerbose($"No attribute-level events found in form '{FormId}'");
                return false;
            }

            bool foundAny = false;
            foreach (var eventElement in eventsElement.Elements("event"))
            {
                string eventName = eventElement.Attribute("name")?.Value;
                string attributeName = eventElement.Attribute("attribute")?.Value;
                
                // Only process events with the specified attribute
                if (string.IsNullOrEmpty(attributeName) || !string.Equals(attributeName, AttributeName, StringComparison.OrdinalIgnoreCase))
                    continue;
                
                // Apply event name filter
                if (!string.IsNullOrEmpty(EventName) && !string.Equals(eventName, EventName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Get handlers
                var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                foreach (var handlersElement in handlersElements)
                {
                    foreach (var handler in handlersElement.Elements("Handler"))
                    {
                        PSObject handlerObj = ParseHandler(handler, eventName, null, null, null, attributeName);
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
        /// Gets tab-level events.
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        /// <returns>True if any events were found, false otherwise.</returns>
        private bool GetTabEvents(XElement formElement)
        {
            // Find the tab
            var tab = FormXmlHelper.FindTab(formElement, tabName: TabName);
            
            if (tab == null)
            {
                throw new InvalidOperationException($"Tab '{TabName}' not found");
            }

            XElement eventsElement = tab.Element("events");
            
            if (eventsElement == null || !eventsElement.Elements("event").Any())
            {
                WriteVerbose($"No events found for tab '{TabName}'");
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
                        PSObject handlerObj = ParseHandler(handler, eventName, null, TabName, null, null);
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
                        PSObject handlerObj = ParseHandler(handler, eventName, ControlId, TabName, SectionName, null);
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
        /// Gets all event handlers from all locations (form-level, attribute-level, tab-level, and control-level).
        /// </summary>
        /// <param name="formElement">The form XML element.</param>
        /// <returns>True if any events were found, false otherwise.</returns>
        private bool GetAllEvents(XElement formElement)
        {
            bool foundAny = false;
            
            // Get form-level events (including attribute-level)
            XElement eventsElement = formElement.Element("events");
            if (eventsElement != null)
            {
                foreach (var eventElement in eventsElement.Elements("event"))
                {
                    string eventName = eventElement.Attribute("name")?.Value;
                    string attributeName = eventElement.Attribute("attribute")?.Value;
                    
                    var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                    foreach (var handlersElement in handlersElements)
                    {
                        foreach (var handler in handlersElement.Elements("Handler"))
                        {
                            PSObject handlerObj = ParseHandler(handler, eventName, null, null, null, attributeName);
                            if (handlerObj != null)
                            {
                                foundAny = true;
                                WriteObject(handlerObj);
                            }
                        }
                    }
                }
            }
            
            // Get tab-level events
            var tabs = formElement.Descendants("tab");
            foreach (var tab in tabs)
            {
                string tabName = tab.Attribute("name")?.Value;
                XElement tabEventsElement = tab.Element("events");
                
                if (tabEventsElement != null)
                {
                    foreach (var eventElement in tabEventsElement.Elements("event"))
                    {
                        string eventName = eventElement.Attribute("name")?.Value;
                        
                        var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                        foreach (var handlersElement in handlersElements)
                        {
                            foreach (var handler in handlersElement.Elements("Handler"))
                            {
                                PSObject handlerObj = ParseHandler(handler, eventName, null, tabName, null, null);
                                if (handlerObj != null)
                                {
                                    foundAny = true;
                                    WriteObject(handlerObj);
                                }
                            }
                        }
                    }
                }
            }
            
            // Get control-level events
            var controls = formElement.Descendants("control");
            foreach (var control in controls)
            {
                string controlId = control.Attribute("id")?.Value;
                
                // Find tab and section for this control
                var section = control.Ancestors("section").FirstOrDefault();
                var tab = section?.Ancestors("tab").FirstOrDefault();
                string tabName = tab?.Attribute("name")?.Value;
                string sectionName = section?.Attribute("name")?.Value;
                
                XElement controlEventsElement = control.Element("events");
                
                if (controlEventsElement != null)
                {
                    foreach (var eventElement in controlEventsElement.Elements("event"))
                    {
                        string eventName = eventElement.Attribute("name")?.Value;
                        
                        var handlersElements = eventElement.Elements("Handlers").Concat(eventElement.Elements("InternalHandlers"));
                        foreach (var handlersElement in handlersElements)
                        {
                            foreach (var handler in handlersElement.Elements("Handler"))
                            {
                                PSObject handlerObj = ParseHandler(handler, eventName, controlId, tabName, sectionName, null);
                                if (handlerObj != null)
                                {
                                    foundAny = true;
                                    WriteObject(handlerObj);
                                }
                            }
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
        /// <param name="controlId">The control ID (null for form-level, attribute-level, and tab-level events).</param>
        /// <param name="tabName">The tab name (null for form-level and attribute-level events, or the tab containing the control).</param>
        /// <param name="sectionName">The section name (null for form-level, attribute-level, and tab-level events).</param>
        /// <param name="attributeName">The attribute name (null for form-level, tab-level, and control-level events).</param>
        /// <returns>A PSObject representing the handler, or null if filtered out.</returns>
        private PSObject ParseHandler(XElement handler, string eventName, string controlId, string tabName, string sectionName, string attributeName)
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
            handlerObj.Properties.Add(new PSNoteProperty("Attribute", attributeName));
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
