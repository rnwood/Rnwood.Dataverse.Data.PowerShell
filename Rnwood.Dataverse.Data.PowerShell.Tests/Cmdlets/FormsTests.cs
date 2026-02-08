using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Form-related cmdlets:
/// - Get-DataverseForm
/// - Set-DataverseForm  
/// - Get-DataverseFormTab
/// - Get-DataverseFormSection
/// - Get-DataverseFormControl
/// - Set-DataverseFormControl
/// - Remove-DataverseFormControl
/// - New-DataverseFormControl
/// - Get-DataverseFormLibrary
/// - Set-DataverseFormLibrary
/// - Get-DataverseFormEventHandler
/// - Set-DataverseFormEventHandler
/// </summary>
public class FormsTests : TestBase
{
    // Sample FormXML for testing - represents a typical form structure
    private const string SampleFormXml = @"<form>
        <tabs>
            <tab name=""GeneralTab"" id=""{12345678-1234-1234-1234-123456789001}"" expanded=""true"" visible=""true"" showlabel=""true"">
                <labels><label description=""General"" languagecode=""1033"" /></labels>
                <columns>
                    <column width=""50%"">
                        <sections>
                            <section name=""GeneralSection"" id=""{12345678-1234-1234-1234-123456789011}"" showlabel=""true"" showbar=""true"" visible=""true"" columns=""2"">
                                <labels><label description=""General Info"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789021}"" colspan=""1"" rowspan=""1"" auto=""true"">
                                            <labels><label description=""First Name"" languagecode=""1033"" /></labels>
                                            <control id=""firstname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""firstname"" disabled=""false"" />
                                        </cell>
                                        <cell id=""{12345678-1234-1234-1234-123456789022}"">
                                            <labels><label description=""Last Name"" languagecode=""1033"" /></labels>
                                            <control id=""lastname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""lastname"" />
                                        </cell>
                                    </row>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789023}"">
                                            <labels><label description=""Email"" languagecode=""1033"" /></labels>
                                            <control id=""emailaddress1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""emailaddress1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                    <column width=""50%"">
                        <sections>
                            <section name=""DetailsSection"" id=""{12345678-1234-1234-1234-123456789012}"" showlabel=""true"" visible=""true"">
                                <labels><label description=""Details"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789024}"">
                                            <labels><label description=""Phone"" languagecode=""1033"" /></labels>
                                            <control id=""telephone1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""telephone1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                </columns>
            </tab>
            <tab name=""AddressTab"" id=""{12345678-1234-1234-1234-123456789002}"" expanded=""false"" visible=""true"" showlabel=""true"">
                <labels><label description=""Address"" languagecode=""1033"" /></labels>
                <columns>
                    <column width=""100%"">
                        <sections>
                            <section name=""AddressSection"" id=""{12345678-1234-1234-1234-123456789013}"" showlabel=""true"" visible=""true"">
                                <labels><label description=""Address Info"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789025}"">
                                            <labels><label description=""Street"" languagecode=""1033"" /></labels>
                                            <control id=""address1_line1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""address1_line1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                </columns>
            </tab>
        </tabs>
        <header>
            <rows>
                <row>
                    <cell id=""{12345678-1234-1234-1234-123456789030}"">
                        <control id=""header_fullname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""fullname"" />
                    </cell>
                </row>
            </rows>
        </header>
        <hiddencontrols>
            <data id=""contactid"" datafieldname=""contactid"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" />
        </hiddencontrols>
        <clientresources>
            <internalresources>
                <clientincludes>
                    <webresource name=""new_contactform.js"" type=""JScript"" libraryuniquename=""contact_form_lib"" />
                </clientincludes>
            </internalresources>
        </clientresources>
        <formLibraries>
            <Library name=""new_contactform.js"" libraryUniqueId=""{12345678-1234-1234-1234-123456789050}"" />
            <Library name=""another_library.js"" libraryUniqueId=""{12345678-1234-1234-1234-123456789051}"" />
        </formLibraries>
        <events>
            <event name=""onload"" application=""false"" active=""true"">
                <Handlers>
                    <Handler functionName=""onLoad"" libraryName=""new_contactform.js"" handlerUniqueId=""{12345678-1234-1234-1234-123456789040}"" enabled=""true"" parameters="""" passExecutionContext=""true"" />
                </Handlers>
            </event>
        </events>
    </form>";

    private PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        
        // Register form-related cmdlets
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseForm", typeof(GetDataverseFormCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormTab", typeof(GetDataverseFormTabCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormSection", typeof(GetDataverseFormSectionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormControl", typeof(GetDataverseFormControlCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormLibrary", typeof(GetDataverseFormLibraryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormEventHandler", typeof(GetDataverseFormEventHandlerCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    private Entity CreateTestForm(Guid formId, string formName, string objectTypeCode, string formXml = null)
    {
        return new Entity("systemform")
        {
            Id = formId,
            ["formid"] = formId,
            ["name"] = formName,
            ["uniquename"] = $"form_{formName.ToLower().Replace(" ", "_")}",
            ["objecttypecode"] = objectTypeCode,
            ["type"] = new OptionSetValue(2), // Main form
            ["description"] = $"Test form: {formName}",
            ["formactivationstate"] = new OptionSetValue(1), // Active
            ["formpresentation"] = new OptionSetValue(1), // Air form
            ["isdefault"] = false,
            ["formxml"] = formXml ?? SampleFormXml
        };
    }

    #region Get-DataverseForm Tests

    [Fact]
    public void GetDataverseForm_RetrievesFormsByEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestContactForm", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("TestContactForm");
    }

    [Fact]
    public void GetDataverseForm_IncludesFormXml_WhenIncludeFormXmlSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestFormWithXml", "contact", SampleFormXml);
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact")
          .AddParameter("IncludeFormXml", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["FormXml"].Should().NotBeNull();
        ((string)results[0].Properties["FormXml"].Value).Should().Contain("<form>");
    }

    [Fact]
    public void GetDataverseForm_FiltersByFormType()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var mainFormId = Guid.NewGuid();
        var mainForm = CreateTestForm(mainFormId, "MainForm", "contact");
        mainForm["type"] = new OptionSetValue(2); // Main
        Service!.Create(mainForm);
        
        var quickCreateFormId = Guid.NewGuid();
        var quickCreateForm = CreateTestForm(quickCreateFormId, "QuickCreateForm", "contact");
        quickCreateForm["type"] = new OptionSetValue(7); // QuickCreate
        Service!.Create(quickCreateForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact")
          .AddParameter("FormType", FormType.Main);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("MainForm");
    }

    [Fact]
    public void GetDataverseForm_RetrievesByFormId()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "SpecificForm", "contact");
        Service!.Create(testForm);
        
        // Create another form to ensure we get the right one
        var otherFormId = Guid.NewGuid();
        var otherForm = CreateTestForm(otherFormId, "OtherForm", "contact");
        Service!.Create(otherForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("SpecificForm");
    }

    #endregion

    #region Get-DataverseFormTab Tests

    [Fact]
    public void GetDataverseFormTab_RetrievesAllTabsFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2); // GeneralTab and AddressTab
        results.Select(r => r.Properties["Name"].Value).Should().Contain("GeneralTab");
        results.Select(r => r.Properties["Name"].Value).Should().Contain("AddressTab");
    }

    [Fact]
    public void GetDataverseFormTab_RetrievesSpecificTabByName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("GeneralTab");
        results[0].Properties["Expanded"].Value.Should().Be(true);
    }

    [Fact]
    public void GetDataverseFormTab_IncludesSectionsInTabOutput()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var sections = results[0].Properties["Sections"].Value as object[];
        sections.Should().NotBeNull();
        sections.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Get-DataverseFormSection Tests

    [Fact]
    public void GetDataverseFormSection_RetrievesAllSectionsFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(3); // GeneralSection, DetailsSection, AddressSection
        results.Select(r => r.Properties["Name"].Value).Should().Contain("GeneralSection");
    }

    [Fact]
    public void GetDataverseFormSection_RetrievesSectionsByTabName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2); // GeneralSection and DetailsSection in GeneralTab
    }

    [Fact]
    public void GetDataverseFormSection_RetrievesSpecificSectionByNameAndTab()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("GeneralSection");
    }

    [Fact]
    public void GetDataverseFormSection_IncludesControlsInSectionOutput()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var controls = results[0].Properties["Controls"].Value as object[];
        controls.Should().NotBeNull();
        controls.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Get-DataverseFormControl Tests

    [Fact]
    public void GetDataverseFormControl_RetrievesAllControls()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(5); // firstname, lastname, emailaddress1, telephone1, address1_line1
    }

    [Fact]
    public void GetDataverseFormControl_FiltersByTabName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1); // Only address1_line1
        results[0].Properties["DataField"].Value.Should().Be("address1_line1");
    }

    [Fact]
    public void GetDataverseFormControl_FiltersBySectionName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "DetailsSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1); // Only telephone1
        results[0].Properties["DataField"].Value.Should().Be("telephone1");
    }

    [Fact]
    public void GetDataverseFormControl_RetrievesSpecificControlById()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("ControlId", "firstname");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be("firstname");
        results[0].Properties["DataField"].Value.Should().Be("firstname");
    }

    [Fact]
    public void GetDataverseFormControl_RetrievesHeaderControls()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithHeaderControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "[Header]");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["DataField"].Value.Should().Be("fullname");
    }

    #endregion

    #region Get-DataverseFormLibrary Tests

    [Fact]
    public void GetDataverseFormLibrary_RetrievesLibrariesFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithLibraries", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormLibrary")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(1);
        results.Select(r => r.Properties["Name"].Value).Should().Contain("new_contactform.js");
    }

    #endregion

    #region Get-DataverseFormEventHandler Tests

    [Fact]
    public void GetDataverseFormEventHandler_RetrievesEventHandlersFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithEvents", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormEventHandler")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(1);
        results.Select(r => r.Properties["FunctionName"].Value).Should().Contain("onLoad");
    }

    #endregion

    #region Form XML Parsing Tests

    [Fact]
    public void DataverseForm_ParsesTabLayoutCorrectly()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithLayout", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Layout"].Value.Should().Be("TwoColumns");
        results[0].Properties["Column1Width"].Value.Should().Be(50);
        results[0].Properties["Column2Width"].Value.Should().Be(50);
    }

    [Fact]
    public void DataverseForm_ParsesSingleColumnLayout()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSingleColumn", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Layout"].Value.Should().Be("OneColumn");
        results[0].Properties["Column1Width"].Value.Should().Be(100);
    }

    [Fact]
    public void DataverseForm_ParsesTabExpandedState()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithExpandedTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2);
        var generalTab = results.First(r => (string)r.Properties["Name"].Value == "GeneralTab");
        var addressTab = results.First(r => (string)r.Properties["Name"].Value == "AddressTab");
        generalTab.Properties["Expanded"].Value.Should().Be(true);
        addressTab.Properties["Expanded"].Value.Should().Be(false);
    }

    [Fact]
    public void DataverseForm_ParsesControlCellAttributes()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithCellAttrs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("ControlId", "firstname");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Row"].Should().NotBeNull();
        results[0].Properties["Column"].Should().NotBeNull();
    }

    #endregion

    #region Form Retrieval With Multiple Forms Tests

    [Fact]
    public void GetDataverseForm_ReturnsMultipleFormsForEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        for (int i = 1; i <= 3; i++)
        {
            var formId = Guid.NewGuid();
            var testForm = CreateTestForm(formId, $"Form{i}", "contact");
            Service!.Create(testForm);
        }
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(3);
    }

    [Fact]
    public void GetDataverseForm_OnlyReturnsFormsForSpecifiedEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        // Only use systemform metadata to avoid issues with missing metadata for other entities
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var contactFormId = Guid.NewGuid();
        var contactForm = CreateTestForm(contactFormId, "ContactForm", "contact");
        Service!.Create(contactForm);
        
        // Create another form for a different entity code to test filtering
        var otherFormId = Guid.NewGuid();
        var otherForm = CreateTestForm(otherFormId, "OtherForm", "lead");
        Service!.Create(otherForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("ContactForm");
    }

    #endregion

    // Set-DataverseFormControl Tests (require write operations - documented but not implemented without E2E)

    [Fact(Skip = "Requires E2E testing - updates systemform.formxml")]
    public void SetDataverseFormControl_UpdatesControlProperties()
    {
        // Tests updating control attributes (visible, disabled, etc.)
        // This test requires write operations that modify form XML
    }

    [Fact(Skip = "Requires E2E testing - updates systemform.formxml")]
    public void SetDataverseFormControl_AutoControlType_DetectsTypeFromAttribute()
    {
        // Tests automatic control type detection based on attribute metadata
    }

    [Fact(Skip = "Requires E2E testing - updates systemform.formxml")]
    public void SetDataverseFormControl_Subgrid_CreatesSubgridControl()
    {
        // Tests creating/updating subgrid controls
    }

    [Fact(Skip = "Requires E2E testing - updates systemform.formxml")]
    public void SetDataverseFormControl_SupportsWhatIf()
    {
        // Tests -WhatIf parameter
    }

    // New-DataverseFormControl Tests

    [Fact(Skip = "Requires E2E testing - adds control to form")]
    public void NewDataverseFormControl_CreatesNewControlInSection()
    {
        // Tests adding new control to specified tab/section
    }

    [Fact(Skip = "Requires E2E testing - adds control to form")]
    public void NewDataverseFormControl_Header_CreatesControlInHeader()
    {
        // Tests adding control to form header
    }

    // Remove-DataverseFormControl Tests

    [Fact(Skip = "Requires E2E testing - removes control from form")]
    public void RemoveDataverseFormControl_RemovesControlById()
    {
        // Tests removing control from form
    }

    // Form Tab Layout Tests

    [Fact(Skip = "Requires E2E testing - modifies form tabs")]
    public void DataverseFormTab_CanReorderTabs()
    {
        // Tests reordering tabs in form
    }

    [Fact(Skip = "Requires E2E testing - modifies form tabs")]
    public void DataverseFormTab_CanAddNewTab()
    {
        // Tests adding new tab to form
    }

    [Fact(Skip = "Requires E2E testing - modifies form tabs")]
    public void DataverseFormTab_CanRemoveTab()
    {
        // Tests removing tab from form
    }

    // Form Library and Event Handler Tests - Write operations

    [Fact(Skip = "Requires E2E testing - manages form libraries")]
    public void DataverseFormLibrary_CanAddJavaScriptLibrary()
    {
        // Tests adding JavaScript library to form
    }

    [Fact(Skip = "Requires E2E testing - manages form libraries")]
    public void DataverseFormLibrary_CanRemoveLibrary()
    {
        // Tests removing library from form
    }

    [Fact(Skip = "Requires E2E testing - manages form event handlers")]
    public void DataverseFormEventHandler_CanAddOnLoadEventHandler()
    {
        // Tests adding OnLoad event handler
    }

    [Fact(Skip = "Requires E2E testing - manages form event handlers")]
    public void DataverseFormEventHandler_CanAddControlEventHandler()
    {
        // Tests adding control-specific event handler (OnChange, etc.)
    }

    [Fact(Skip = "Requires E2E testing - manages form event handlers")]
    public void DataverseFormEventHandler_CanRemoveEventHandler()
    {
        // Tests removing event handler from form
    }
}
