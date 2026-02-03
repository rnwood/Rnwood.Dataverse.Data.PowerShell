using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Model
{
    /// <summary>
    /// Helper class for working with Dataverse FormXML.
    /// Provides consolidated functionality for parsing, manipulating, and updating form XML.
    /// </summary>
    public static class FormXmlHelper
    {
        /// <summary>
        /// Retrieves a form from Dataverse, attempting to get the unpublished version first,
        /// then falling back to published if not found.
        /// </summary>
        /// <param name="connection">The organization service connection.</param>
        /// <param name="formId">The ID of the form to retrieve.</param>
        /// <param name="columnSet">The columns to retrieve. If null, defaults to formxml and objecttypecode.</param>
        /// <returns>The retrieved form entity.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the form is not found.</exception>
        public static Entity RetrieveForm(ServiceClient connection, Guid formId, ColumnSet columnSet = null)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            columnSet = columnSet ?? new ColumnSet("formxml", "objecttypecode");

            try
            {
                var retrieveUnpublishedRequest = new RetrieveUnpublishedRequest
                {
                    Target = new EntityReference("systemform", formId),
                    ColumnSet = columnSet
                };
                var response = (RetrieveUnpublishedResponse)connection.Execute(retrieveUnpublishedRequest);
                return response.Entity;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (QueryHelpers.IsNotFoundException(ex))
                {
                    // Try published version
                    return connection.Retrieve("systemform", formId, columnSet);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                // For testing with FakeXrmEasy or other scenarios where RetrieveUnpublished is not supported,
                // fall back to regular Retrieve
                return connection.Retrieve("systemform", formId, columnSet);
            }
        }

        /// <summary>
        /// Parses FormXML from an entity and returns the XDocument and SystemForm element.
        /// </summary>
        /// <param name="form">The form entity containing the formxml attribute.</param>
        /// <returns>A tuple containing the XDocument and SystemForm XElement.</returns>
        /// <exception cref="ArgumentNullException">Thrown when form is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when FormXml is missing or invalid.</exception>
        public static (XDocument Document, XElement SystemForm) ParseFormXml(Entity form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            if (!form.Contains("formxml"))
            {
                throw new InvalidOperationException($"Form '{form.Id}' does not contain FormXml");
            }

            string formXml = form.GetAttributeValue<string>("formxml");
            if (string.IsNullOrWhiteSpace(formXml))
            {
                throw new InvalidOperationException($"Form '{form.Id}' has empty FormXml");
            }

            XDocument doc = XDocument.Parse(formXml);
            XElement systemForm = doc.Root;

            if (systemForm == null)
            {
                throw new InvalidOperationException($"Form '{form.Id}' has invalid FormXml structure - missing SystemForm element");
            }

            return (doc, systemForm);
        }

        /// <summary>
        /// Updates a form with new FormXML content.
        /// </summary>
        /// <param name="connection">The organization service connection.</param>
        /// <param name="formId">The ID of the form to update.</param>
        /// <param name="document">The updated XML document.</param>
        public static void UpdateFormXml(ServiceClient connection, Guid formId, XDocument document)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            Entity updateForm = new Entity("systemform", formId);
            updateForm["formxml"] = document.ToString();
            connection.Update(updateForm);
        }

        /// <summary>
        /// Publishes the entity associated with the form.
        /// </summary>
        /// <param name="connection">The organization service connection.</param>
        /// <param name="entityName">The logical name of the entity to publish.</param>
        public static void PublishEntity(ServiceClient connection, string entityName)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity name cannot be null or empty", nameof(entityName));

            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><entities><entity>{entityName}</entity></entities></importexportxml>"
            };
            connection.Execute(publishRequest);
        }

        /// <summary>
        /// Finds a tab element by name or ID.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="tabName">The tab name to search for (optional).</param>
        /// <param name="tabId">The tab ID to search for (optional).</param>
        /// <returns>The found tab element, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when both tabName and tabId are null or empty.</exception>
        public static XElement FindTab(XElement systemForm, string tabName = null, string tabId = null)
        {
            if (systemForm == null)
                throw new ArgumentNullException(nameof(systemForm));

            if (string.IsNullOrWhiteSpace(tabName) && string.IsNullOrWhiteSpace(tabId))
                throw new ArgumentException("Either tabName or tabId must be provided");

            XElement tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
                return null;

            if (!string.IsNullOrWhiteSpace(tabName))
            {
                return tabsElement.Elements("tab")
                    .FirstOrDefault(t => t.Attribute("name")?.Value == tabName);
            }
            else
            {
                return tabsElement.Elements("tab")
                    .FirstOrDefault(t => t.Attribute("id")?.Value == tabId);
            }
        }

        /// <summary>
        /// Finds a control element by ID or data field name.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="controlId">The control ID to search for (optional).</param>
        /// <param name="dataField">The data field name to search for (optional).</param>
        /// <returns>A tuple containing the found control element and its parent row, or (null, null) if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when both controlId and dataField are null or empty.</exception>
        public static (XElement Control, XElement ParentRow) FindControl(XElement systemForm, string controlId = null, string dataField = null)
        {
            if (systemForm == null)
                throw new ArgumentNullException(nameof(systemForm));

            if (string.IsNullOrWhiteSpace(controlId) && string.IsNullOrWhiteSpace(dataField))
                throw new ArgumentException("Either controlId or dataField must be provided");

            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
                return (null, null);

            foreach (var tab in tabsElement.Elements("tab"))
            {
                var columnsElement = tab.Element("columns");
                if (columnsElement == null) continue;

                foreach (var column in columnsElement.Elements("column"))
                {
                    var sectionsElement = column.Element("sections");
                    if (sectionsElement == null) continue;

                    foreach (var section in sectionsElement.Elements("section"))
                    {
                        var rowsElement = section.Element("rows");
                        if (rowsElement == null) continue;

                        foreach (var row in rowsElement.Elements("row"))
                        {
                            foreach (var cell in row.Elements("cell"))
                            {
                                XElement control;
                                if (!string.IsNullOrWhiteSpace(controlId))
                                {
                                    control = cell.Elements("control")
                                        .FirstOrDefault(c => c.Attribute("id")?.Value == controlId);
                                }
                                else
                                {
                                    control = cell.Elements("control")
                                        .FirstOrDefault(c => c.Attribute("datafieldname")?.Value == dataField);
                                }

                                if (control != null)
                                    return (control, row);
                            }
                        }
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Finds a section element within a specific tab.
        /// </summary>
        /// <param name="tab">The tab element to search within.</param>
        /// <param name="sectionName">The section name to search for (optional).</param>
        /// <param name="sectionId">The section ID to search for (optional).</param>
        /// <returns>The found section element, or null if not found.</returns>
        public static XElement FindSectionInTab(XElement tab, string sectionName = null, string sectionId = null)
        {
            if (tab == null)
                throw new ArgumentNullException(nameof(tab));

            if (string.IsNullOrWhiteSpace(sectionName) && string.IsNullOrWhiteSpace(sectionId))
                throw new ArgumentException("Either sectionName or sectionId must be provided");

            var columnsElement = tab.Element("columns");
            if (columnsElement == null)
                return null;

            foreach (var column in columnsElement.Elements("column"))
            {
                var sectionsElement = column.Element("sections");
                if (sectionsElement == null) continue;

                XElement section;
                if (!string.IsNullOrWhiteSpace(sectionName))
                {
                    section = sectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("name")?.Value == sectionName);
                }
                else
                {
                    section = sectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("id")?.Value == sectionId);
                }

                if (section != null)
                    return section;
            }

            return null;
        }

        /// <summary>
        /// Gets the rows element from a section, creating it if it doesn't exist.
        /// </summary>
        /// <param name="section">The section element.</param>
        /// <returns>The rows element.</returns>
        public static XElement GetOrCreateRowsElement(XElement section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            var rowsElement = section.Element("rows");
            if (rowsElement == null)
            {
                rowsElement = new XElement("rows");
                section.Add(rowsElement);
            }

            return rowsElement;
        }

        /// <summary>
        /// Finds the target section for a form operation, with comprehensive error handling.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="tabName">The tab name (required).</param>
        /// <param name="sectionName">The section name (optional).</param>
        /// <param name="sectionId">The section ID (optional, used if sectionName is not provided).</param>
        /// <returns>A tuple containing the target section and tab elements.</returns>
        /// <exception cref="InvalidOperationException">Thrown when tab or section is not found.</exception>
        public static (XElement Section, XElement Tab) FindTargetSection(XElement systemForm, string tabName, string sectionName = null, string sectionId = null)
        {
            if (systemForm == null)
                throw new ArgumentNullException(nameof(systemForm));

            if (string.IsNullOrWhiteSpace(tabName))
                throw new ArgumentException("Tab name is required", nameof(tabName));

            // Find the target tab
            var targetTab = FindTab(systemForm, tabName: tabName);
            if (targetTab == null)
            {
                throw new InvalidOperationException($"Tab '{tabName}' not found in form");
            }

            // If no section specified, return the first section or null
            if (string.IsNullOrWhiteSpace(sectionName) && string.IsNullOrWhiteSpace(sectionId))
            {
                var firstSection = FindSectionInTab(targetTab);
                return (firstSection, targetTab);
            }

            // Find the specific section
            XElement targetSection;
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                targetSection = FindSectionInTab(targetTab, sectionName: sectionName);
                if (targetSection == null)
                {
                    throw new InvalidOperationException($"Section '{sectionName}' not found in tab '{tabName}'");
                }
            }
            else
            {
                targetSection = FindSectionInTab(targetTab, sectionId: sectionId);
                if (targetSection == null)
                {
                    throw new InvalidOperationException($"Section with ID '{sectionId}' not found in tab '{tabName}'");
                }
            }

            return (targetSection, targetTab);
        }

        /// <summary>
        /// Gets all sections from a tab.
        /// </summary>
        /// <param name="tab">The tab element.</param>
        /// <returns>An enumerable of section elements.</returns>
        public static System.Collections.Generic.IEnumerable<XElement> GetSectionsInTab(XElement tab)
        {
            if (tab == null)
                yield break;

            var columnsElement = tab.Element("columns");
            if (columnsElement == null)
                yield break;

            foreach (var column in columnsElement.Elements("column"))
            {
                var sectionsElement = column.Element("sections");
                if (sectionsElement == null) continue;

                foreach (var section in sectionsElement.Elements("section"))
                {
                    yield return section;
                }
            }
        }

        /// <summary>
        /// Gets all controls from a form, optionally filtered by section, control ID, or data field.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="tabName">Optional tab name filter.</param>
        /// <param name="sectionName">Optional section name filter.</param>
        /// <param name="controlId">Optional control ID filter.</param>
        /// <param name="dataField">Optional data field name filter.</param>
        /// <returns>An enumerable of tuples containing tab name, section name, and control element.</returns>
        public static System.Collections.Generic.IEnumerable<(string TabName, string SectionName, XElement Control)> GetControls(
            XElement systemForm, 
            string tabName = null,
            string sectionName = null, 
            string controlId = null, 
            string dataField = null)
        {
            if (systemForm == null)
                throw new ArgumentNullException(nameof(systemForm));

            // Check if we should search in the header
            bool searchHeader = string.IsNullOrEmpty(tabName) || tabName.Equals("[Header]", StringComparison.OrdinalIgnoreCase);
            bool searchOnlyHeader = !string.IsNullOrEmpty(tabName) && tabName.Equals("[Header]", StringComparison.OrdinalIgnoreCase);

            // Search header controls if requested
            if (searchHeader)
            {
                var header = systemForm.Element("header");
                if (header != null)
                {
                    var rowsElement = header.Element("rows");
                    if (rowsElement != null)
                    {
                        foreach (var row in rowsElement.Elements("row"))
                        {
                            foreach (var cell in row.Elements("cell"))
                            {
                                foreach (var control in cell.Elements("control"))
                                {
                                    string currentControlId = control.Attribute("id")?.Value;
                                    string currentDataField = control.Attribute("datafieldname")?.Value;

                                    if ((!string.IsNullOrEmpty(controlId) && currentControlId != controlId) ||
                                        (!string.IsNullOrEmpty(dataField) && currentDataField != dataField))
                                        continue;

                                    yield return ("[Header]", "[Header]", control);
                                }
                            }
                        }
                    }
                }
            }

            // If only searching header, don't search tabs
            if (searchOnlyHeader)
                yield break;

            foreach (var tab in systemForm.Elements("tabs").Elements("tab"))
            {
                string currentTabName = tab.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(tabName) && currentTabName != tabName)
                    continue;

                foreach (var column in tab.Elements("columns").Elements("column"))
                {
                    foreach (var sections in column.Elements("sections"))
                    {
                        foreach (var section in sections.Elements("section"))
                        {
                            string currentSectionName = section.Attribute("name")?.Value;
                            if (!string.IsNullOrEmpty(sectionName) && currentSectionName != sectionName)
                                continue;

                            var rowsElement = section.Element("rows");
                            if (rowsElement != null)
                            {
                                foreach (var row in rowsElement.Elements("row"))
                                {
                                    foreach (var cell in row.Elements("cell"))
                                    {
                                        foreach (var control in cell.Elements("control"))
                                        {
                                            string currentControlId = control.Attribute("id")?.Value;
                                            string currentDataField = control.Attribute("datafieldname")?.Value;

                                            if ((!string.IsNullOrEmpty(controlId) && currentControlId != controlId) ||
                                                (!string.IsNullOrEmpty(dataField) && currentDataField != dataField))
                                                continue;

                                            yield return (currentTabName, currentSectionName, control);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses the complete form structure into a PowerShell object.
        /// </summary>
        /// <param name="document">The form XML document.</param>
        /// <returns>A PSObject containing the parsed form structure.</returns>
        public static PSObject ParseFormStructure(XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            PSObject parsed = new PSObject();
            XElement formElement = document.Root;
            
            if (formElement == null)
            {
                return parsed;
            }

            // Parse form root attributes
            if (formElement.Name.LocalName == "form")
            {
                var formAttributes = new PSObject();
                foreach (var attr in formElement.Attributes())
                {
                    formAttributes.Properties.Add(new PSNoteProperty(attr.Name.LocalName, attr.Value));
                }
                parsed.Properties.Add(new PSNoteProperty("FormAttributes", formAttributes));
            }

            // Parse hidden controls
            var hiddenControlsElement = formElement.Element("hiddencontrols");
            if (hiddenControlsElement != null)
            {
                var hiddenControls = hiddenControlsElement.Elements("data").Select(data => new PSObject(new
                {
                    Id = data.Attribute("id")?.Value,
                    DataFieldName = data.Attribute("datafieldname")?.Value,
                    ClassId = data.Attribute("classid")?.Value
                })).ToArray();
                parsed.Properties.Add(new PSNoteProperty("HiddenControls", hiddenControls));
            }

            // Parse tabs
            var tabsElement = formElement.Element("tabs");
            if (tabsElement != null)
            {
                var tabs = tabsElement.Elements("tab").Select(tab => ParseTab(tab)).ToArray();
                parsed.Properties.Add(new PSNoteProperty("Tabs", tabs));
            }

            // Parse header
            var headerElement = formElement.Element("header");
            if (headerElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("Header", ParseHeader(headerElement)));
            }

            // Parse client resources
            var clientResourcesElement = formElement.Element("clientresources");
            if (clientResourcesElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("ClientResources", ParseClientResources(clientResourcesElement)));
            }

            // Parse events
            var eventsElement = formElement.Element("events");
            if (eventsElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("Events", ParseEvents(eventsElement)));
            }

            // Parse navigation
            var navElement = formElement.Element("Navigation");
            if (navElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("Navigation", ParseNavigation(navElement)));
            }

            return parsed;
        }

        /// <summary>
        /// Parses a tab element into a PowerShell object.
        /// </summary>
        /// <param name="tab">The tab XElement to parse.</param>
        /// <returns>A PSObject representing the tab.</returns>
        public static PSObject ParseTab(XElement tab)
        {
            if (tab == null)
                throw new ArgumentNullException(nameof(tab));

            PSObject tabObj = new PSObject();
            tabObj.Properties.Add(new PSNoteProperty("Id", tab.Attribute("id")?.Value));
            tabObj.Properties.Add(new PSNoteProperty("Name", tab.Attribute("name")?.Value));
            tabObj.Properties.Add(new PSNoteProperty("Expanded", tab.Attribute("expanded")?.Value == "true"));
            tabObj.Properties.Add(new PSNoteProperty("Visible", tab.Attribute("visible")?.Value != "false"));

            // Parse labels
            var labelsElement = tab.Element("labels");
            if (labelsElement != null)
            {
                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                {
                    Description = l.Attribute("description")?.Value,
                    LanguageCode = l.Attribute("languagecode")?.Value
                })).ToArray();
                tabObj.Properties.Add(new PSNoteProperty("Labels", labels));
            }

            // Parse columns and determine layout
            var columnsElement = tab.Element("columns");
            if (columnsElement != null)
            {
                var columns = columnsElement.Elements("column").ToArray();
                
                // Determine layout based on column count
                string layout;
                switch (columns.Length)
                {
                    case 1:
                        layout = "OneColumn";
                        break;
                    case 2:
                        layout = "TwoColumns";
                        break;
                    case 3:
                        layout = "ThreeColumns";
                        break;
                    default:
                        layout = "Custom"; // More than 3 columns or 0 columns
                        break;
                }
                tabObj.Properties.Add(new PSNoteProperty("Layout", layout));

                // Add individual column width properties
                if (columns.Length >= 1)
                {
                    var width1Str = columns[0].Attribute("width")?.Value;
                    if (width1Str != null && width1Str.EndsWith("%") && int.TryParse(width1Str.TrimEnd('%'), out int width1))
                    {
                        tabObj.Properties.Add(new PSNoteProperty("Column1Width", width1));
                    }
                }
                if (columns.Length >= 2)
                {
                    var width2Str = columns[1].Attribute("width")?.Value;
                    if (width2Str != null && width2Str.EndsWith("%") && int.TryParse(width2Str.TrimEnd('%'), out int width2))
                    {
                        tabObj.Properties.Add(new PSNoteProperty("Column2Width", width2));
                    }
                }
                if (columns.Length >= 3)
                {
                    var width3Str = columns[2].Attribute("width")?.Value;
                    if (width3Str != null && width3Str.EndsWith("%") && int.TryParse(width3Str.TrimEnd('%'), out int width3))
                    {
                        tabObj.Properties.Add(new PSNoteProperty("Column3Width", width3));
                    }
                }

                // Parse sections (maintain backward compatibility)
                var sections = columns
                    .SelectMany(col => col.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>())
                    .Select(sec => ParseSection(sec))
                    .ToArray();
                tabObj.Properties.Add(new PSNoteProperty("Sections", sections));
            }
            else
            {
                // No columns element
                tabObj.Properties.Add(new PSNoteProperty("Layout", "None"));
                tabObj.Properties.Add(new PSNoteProperty("Sections", new object[0]));
            }

            return tabObj;
        }

        /// <summary>
        /// Parses a section element into a PowerShell object.
        /// </summary>
        /// <param name="section">The section XElement to parse.</param>
        /// <returns>A PSObject representing the section.</returns>
        public static PSObject ParseSection(XElement section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            PSObject secObj = new PSObject();
            secObj.Properties.Add(new PSNoteProperty("Id", section.Attribute("id")?.Value));
            secObj.Properties.Add(new PSNoteProperty("Name", section.Attribute("name")?.Value));
            secObj.Properties.Add(new PSNoteProperty("ShowLabel", section.Attribute("showlabel")?.Value != "false"));
            secObj.Properties.Add(new PSNoteProperty("Visible", section.Attribute("visible")?.Value != "false"));

            // Parse labels
            var labelsElement = section.Element("labels");
            if (labelsElement != null)
            {
                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                {
                    Description = l.Attribute("description")?.Value,
                    LanguageCode = l.Attribute("languagecode")?.Value
                })).ToArray();
                secObj.Properties.Add(new PSNoteProperty("Labels", labels));
            }

            // Parse controls - pass the parent cell to ParseControl so it can read labels from the cell
            var rowsElement = section.Element("rows");
            if (rowsElement != null)
            {
                var controls = rowsElement.Elements("row")
                    .SelectMany(row => row.Elements("cell"))
                    .SelectMany(cell => cell.Elements("control").Select(ctrl => ParseControl(ctrl, cell)))
                    .ToArray();
                secObj.Properties.Add(new PSNoteProperty("Controls", controls));
            }

            return secObj;
        }

        /// <summary>
        /// Parses a control element into a PowerShell object.
        /// </summary>
        /// <param name="control">The control XElement to parse.</param>
        /// <param name="parentCell">The parent cell XElement for cell-level attributes.</param>
        /// <returns>A PSObject representing the control.</returns>
        public static PSObject ParseControl(XElement control, XElement parentCell = null)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            PSObject ctrlObj = new PSObject();
            ctrlObj.Properties.Add(new PSNoteProperty("Id", control.Attribute("id")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("DataField", control.Attribute("datafieldname")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("ClassId", control.Attribute("classid")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("Disabled", control.Attribute("disabled")?.Value == "true"));
            
            // Visibility is stored at the cell level, not control level
            bool visible = parentCell?.Attribute("visible")?.Value != "false";
            ctrlObj.Properties.Add(new PSNoteProperty("Visible", visible));
            ctrlObj.Properties.Add(new PSNoteProperty("Hidden", !visible));
            
            ctrlObj.Properties.Add(new PSNoteProperty("ShowLabel", control.Attribute("showlabel")?.Value != "false"));
            ctrlObj.Properties.Add(new PSNoteProperty("IsRequired", control.Attribute("isrequired")?.Value == "true"));

            // Parse labels - labels are stored in the cell, not in the control
            // Per the form XML schema, the labels element goes inside the cell, before the control
            XElement labelsElement = null;
            if (parentCell != null)
            {
                labelsElement = parentCell.Element("labels");
            }
            
            if (labelsElement != null)
            {
                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                {
                    Description = l.Attribute("description")?.Value,
                    LanguageCode = l.Attribute("languagecode")?.Value
                })).ToArray();
                ctrlObj.Properties.Add(new PSNoteProperty("Labels", labels));
            }

            // Parse control events
            var eventsElement = control.Element("events");
            if (eventsElement != null)
            {
                ctrlObj.Properties.Add(new PSNoteProperty("Events", ParseEvents(eventsElement)));
            }

            // Parse control parameters
            var parametersElement = control.Element("parameters");
            if (parametersElement != null)
            {
                var parameters = new PSObject();
                foreach (var param in parametersElement.Elements())
                {
                    parameters.Properties.Add(new PSNoteProperty(param.Name.LocalName, param.Value));
                }
                ctrlObj.Properties.Add(new PSNoteProperty("Parameters", parameters));
            }

            // Parse cell-level attributes if parentCell is provided
            if (parentCell != null)
            {
                ctrlObj.Properties.Add(new PSNoteProperty("CellId", parentCell.Attribute("id")?.Value));
                ctrlObj.Properties.Add(new PSNoteProperty("ColSpan", parentCell.Attribute("colspan") != null ? (int?)int.Parse(parentCell.Attribute("colspan").Value) : null));
                ctrlObj.Properties.Add(new PSNoteProperty("RowSpan", parentCell.Attribute("rowspan") != null ? (int?)int.Parse(parentCell.Attribute("rowspan").Value) : null));
                ctrlObj.Properties.Add(new PSNoteProperty("Auto", parentCell.Attribute("auto")?.Value == "true"));
                ctrlObj.Properties.Add(new PSNoteProperty("LockLevel", parentCell.Attribute("locklevel")?.Value));
            }

            return ctrlObj;
        }

        /// <summary>
        /// Parses form header into a PowerShell object.
        /// </summary>
        /// <param name="header">The header XElement to parse.</param>
        /// <returns>A PSObject representing the header.</returns>
        public static PSObject ParseHeader(XElement header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header));

            PSObject headerObj = new PSObject();
            headerObj.Properties.Add(new PSNoteProperty("Id", header.Attribute("id")?.Value));
            headerObj.Properties.Add(new PSNoteProperty("CellLabelPosition", header.Attribute("celllabelposition")?.Value));

            // Parse header rows and controls - pass the parent cell to ParseControl so it can read labels from the cell
            var rowsElement = header.Element("rows");
            if (rowsElement != null)
            {
                var controls = rowsElement.Elements("row")
                    .SelectMany(row => row.Elements("cell"))
                    .SelectMany(cell => cell.Elements("control").Select(ctrl => ParseControl(ctrl, cell)))
                    .ToArray();
                headerObj.Properties.Add(new PSNoteProperty("Controls", controls));
            }

            return headerObj;
        }

        /// <summary>
        /// Parses client resources into a PowerShell object.
        /// </summary>
        /// <param name="clientResources">The clientresources XElement to parse.</param>
        /// <returns>A PSObject representing the client resources.</returns>
        public static PSObject ParseClientResources(XElement clientResources)
        {
            if (clientResources == null)
                throw new ArgumentNullException(nameof(clientResources));

            PSObject resourcesObj = new PSObject();

            // Parse internal resources
            var internalResourcesElement = clientResources.Element("internalresources");
            if (internalResourcesElement != null)
            {
                var clientIncludesElement = internalResourcesElement.Element("clientincludes");
                if (clientIncludesElement != null)
                {
                    var jsFiles = clientIncludesElement.Elements("internaljscriptfile").Select(js => new PSObject(new
                    {
                        Src = js.Attribute("src")?.Value
                    })).ToArray();
                    resourcesObj.Properties.Add(new PSNoteProperty("JavaScriptFiles", jsFiles));
                }
            }

            return resourcesObj;
        }

        /// <summary>
        /// Parses form events into a PowerShell object.
        /// </summary>
        /// <param name="events">The events XElement to parse.</param>
        /// <returns>A PSObject representing the events.</returns>
        public static PSObject ParseEvents(XElement events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            PSObject eventsObj = new PSObject();

            // Parse form/control events
            var formEvents = events.Elements("event").Select(evt => new PSObject(new
            {
                Name = evt.Attribute("name")?.Value,
                Application = evt.Attribute("application")?.Value,
                Active = evt.Attribute("active")?.Value != "false",
                Handlers = evt.Elements("Handler").Select(h => new PSObject(new
                {
                    FunctionName = h.Attribute("functionName")?.Value,
                    LibraryName = h.Attribute("libraryName")?.Value,
                    HandlerUniqueId = h.Attribute("handlerUniqueId")?.Value,
                    Enabled = h.Attribute("enabled")?.Value != "false",
                    Parameters = h.Attribute("parameters")?.Value,
                    PassExecutionContext = h.Attribute("passExecutionContext")?.Value
                })).ToArray(),
                InternalHandlers = evt.Element("InternalHandlers")?.Elements("Handler").Select(h => new PSObject(new
                {
                    FunctionName = h.Attribute("functionName")?.Value,
                    LibraryName = h.Attribute("libraryName")?.Value,
                    HandlerUniqueId = h.Attribute("handlerUniqueId")?.Value,
                    Enabled = h.Attribute("enabled")?.Value != "false",
                    PassExecutionContext = h.Attribute("passExecutionContext")?.Value
                })).ToArray()
            })).ToArray();

            eventsObj.Properties.Add(new PSNoteProperty("Events", formEvents));
            return eventsObj;
        }

        /// <summary>
        /// Parses form navigation into a PowerShell object.
        /// </summary>
        /// <param name="navigation">The navigation XElement to parse.</param>
        /// <returns>A PSObject representing the navigation.</returns>
        public static PSObject ParseNavigation(XElement navigation)
        {
            if (navigation == null)
                throw new ArgumentNullException(nameof(navigation));

            PSObject navObj = new PSObject();

            var navBarElement = navigation.Element("NavBar");
            if (navBarElement != null)
            {
                var navItems = navBarElement.Elements("NavBarByRelationshipItem").Select(item => new PSObject(new
                {
                    Id = item.Attribute("Id")?.Value,
                    RelationshipName = item.Attribute("RelationshipName")?.Value,
                    TitleResourceId = item.Attribute("TitleResourceId")?.Value,
                    Icon = item.Attribute("Icon")?.Value,
                    ViewId = item.Attribute("ViewId")?.Value,
                    Privileges = item.Element("Privileges")?.Elements("Privilege").Select(p => new PSObject(new
                    {
                        Entity = p.Attribute("Entity")?.Value,
                        Privilege = p.Attribute("Privilege")?.Value
                    })).ToArray()
                })).ToArray();

                navObj.Properties.Add(new PSNoteProperty("Items", navItems));
            }

            // Also handle direct NavBarItem elements for backward compatibility
            var directNavItems = navigation.Elements("NavBarItem").Select(item => new PSObject(new
            {
                Id = item.Attribute("Id")?.Value,
                Area = item.Attribute("Area")?.Value,
                Sequence = item.Attribute("Sequence")?.Value
            })).ToArray();

            if (directNavItems.Length > 0)
            {
                navObj.Properties.Add(new PSNoteProperty("DirectItems", directNavItems));
            }

            return navObj;
        }

        /// <summary>
        /// Finds a section element by name or ID within all tabs.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="sectionName">The section name to search for (optional).</param>
        /// <param name="sectionId">The section ID to search for (optional).</param>
        /// <returns>The found section element, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when both sectionName and sectionId are null or empty.</exception>
        public static XElement FindSection(XElement systemForm, string sectionName = null, string sectionId = null)
        {
            if (systemForm == null)
                throw new ArgumentNullException(nameof(systemForm));

            if (string.IsNullOrWhiteSpace(sectionName) && string.IsNullOrWhiteSpace(sectionId))
                throw new ArgumentException("Either sectionName or sectionId must be provided");

            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
                return null;

            foreach (var tab in tabsElement.Elements("tab"))
            {
                var section = FindSectionInTab(tab, sectionName, sectionId);
                if (section != null)
                    return section;
            }

            return null;
        }

        /// <summary>
        /// Finds a control row for positioning (InsertBefore/InsertAfter logic)
        /// </summary>
        /// <param name="systemForm">The form XML element</param>
        /// <param name="tabName">Name of the tab</param>
        /// <param name="sectionName">Name of the section</param>
        /// <param name="controlIdentifier">Control ID or data field name to find</param>
        /// <returns>The row containing the control, or null if not found</returns>
        public static XElement FindControlRowForPositioning(XElement systemForm, string tabName, string sectionName, string controlIdentifier)
        {
            var sectionResult = FindTargetSection(systemForm, tabName, sectionName);
            if (sectionResult.Section == null) return null;

            var rowsElement = GetOrCreateRowsElement(sectionResult.Section);
            
            foreach (var row in rowsElement.Elements("row"))
            {
                foreach (var cell in row.Elements("cell"))
                {
                    var control = cell.Elements("control").FirstOrDefault(c => 
                        c.Attribute("id")?.Value == controlIdentifier || 
                        c.Attribute("datafieldname")?.Value == controlIdentifier);
                    
                    if (control != null)
                    {
                        return row;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Finds an existing control by ID in the specified section
        /// </summary>
        /// <param name="systemForm">The form XML element</param>
        /// <param name="tabName">Name of the tab</param>
        /// <param name="sectionName">Name of the section</param>
        /// <param name="controlId">The control ID to find</param>
        /// <returns>Tuple of (control element, parent cell element) or (null, null) if not found</returns>
        public static (XElement Control, XElement ParentCell) FindControlById(XElement systemForm, string tabName, string sectionName, string controlId)
        {
            var sectionResult = FindTargetSection(systemForm, tabName, sectionName);
            if (sectionResult.Section == null) return (null, null);

            var rowsElement = GetOrCreateRowsElement(sectionResult.Section);
            
            foreach (var row in rowsElement.Elements("row"))
            {
                foreach (var cell in row.Elements("cell"))
                {
                    var control = cell.Elements("control").FirstOrDefault(c => c.Attribute("id")?.Value == controlId);
                    if (control != null)
                    {
                        return (control, cell);
                    }
                }
            }
            
            return (null, null);
        }

        /// <summary>
        /// Finds an existing control by data field name in the specified section
        /// </summary>
        /// <param name="systemForm">The form XML element</param>
        /// <param name="tabName">Name of the tab</param>
        /// <param name="sectionName">Name of the section</param>
        /// <param name="dataField">The data field name to find</param>
        /// <returns>Tuple of (control element, parent cell element) or (null, null) if not found</returns>
        public static (XElement Control, XElement ParentCell) FindControlByDataField(XElement systemForm, string tabName, string sectionName, string dataField)
        {
            var sectionResult = FindTargetSection(systemForm, tabName, sectionName);
            if (sectionResult.Section == null) return (null, null);

            var rowsElement = GetOrCreateRowsElement(sectionResult.Section);
            
            foreach (var row in rowsElement.Elements("row"))
            {
                foreach (var cell in row.Elements("cell"))
                {
                    var control = cell.Elements("control").FirstOrDefault(c => c.Attribute("datafieldname")?.Value == dataField);
                    if (control != null)
                    {
                        return (control, cell);
                    }
                }
            }
            
            return (null, null);
        }

        /// <summary>
        /// Finds a control element by ID or data field name in the form header.
        /// </summary>
        /// <param name="systemForm">The SystemForm element to search.</param>
        /// <param name="controlId">The control ID to search for (optional).</param>
        /// <param name="dataField">The data field name to search for (optional).</param>
        /// <returns>A tuple containing the found control element and its parent cell, or (null, null) if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when both controlId and dataField are null or empty.</exception>
        public static (XElement Control, XElement ParentCell) FindControlInHeader(XElement systemForm, string controlId = null, string dataField = null)
        {
            var header = systemForm.Element("header");
            if (header == null)
            {
                return (null, null);
            }

            var rows = header.Element("rows");
            if (rows == null)
            {
                return (null, null);
            }

            foreach (var row in rows.Elements("row"))
            {
                foreach (var cell in row.Elements("cell"))
                {
                    var control = cell.Element("control");
                    if (control != null)
                    {
                        if (!string.IsNullOrEmpty(controlId) && control.Attribute("id")?.Value == controlId)
                        {
                            return (control, cell);
                        }
                        if (!string.IsNullOrEmpty(dataField) && control.Attribute("datafieldname")?.Value == dataField)
                        {
                            return (control, cell);
                        }
                    }
                }
            }

            return (null, null);
        }
    }
}