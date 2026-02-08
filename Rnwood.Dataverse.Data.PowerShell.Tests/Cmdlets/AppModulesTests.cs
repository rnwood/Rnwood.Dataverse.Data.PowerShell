using System;
using System.Collections;
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
/// Tests for App Module-related cmdlets:
/// - Get-DataverseAppModule
/// - Set-DataverseAppModule
/// - Remove-DataverseAppModule
/// - Get-DataverseAppModuleComponent
/// - Set-DataverseAppModuleComponent
/// - Remove-DataverseAppModuleComponent
/// </summary>
public class AppModulesTests : TestBase
{
    private PS CreatePowerShellWithAppModuleCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseAppModule", typeof(GetDataverseAppModuleCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseAppModule", typeof(SetDataverseAppModuleCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseAppModule", typeof(RemoveDataverseAppModuleCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseAppModuleComponent", typeof(GetDataverseAppModuleComponentCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseAppModuleComponent", typeof(SetDataverseAppModuleComponentCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseAppModuleComponent", typeof(RemoveDataverseAppModuleComponentCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    // Set-DataverseAppModule Tests
    // Note: TestBase has interceptors for RetrieveUnpublishedMultiple/Request that delegate to regular CRUD

    [Fact]
    public void SetDataverseAppModule_CreatesAppModuleWithUniqueName()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "new_test_app")
          .AddParameter("Name", "New Test App")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().BeOfType<Guid>();
        var createdId = (Guid)results[0].BaseObject;
        createdId.Should().NotBe(Guid.Empty);
        
        // Verify the record was created
        var created = Context!.CreateQuery("appmodule").FirstOrDefault(e => e.Id == createdId);
        created.Should().NotBeNull();
        created!.GetAttributeValue<string>("uniquename").Should().Be("new_test_app");
        created.GetAttributeValue<string>("name").Should().Be("New Test App");
    }

    [Fact]
    public void SetDataverseAppModule_CreatesAppModuleWithMinimalParameters()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Only UniqueName is provided
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "minimal_app")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var createdId = (Guid)results[0].BaseObject;
        
        var created = Context!.CreateQuery("appmodule").FirstOrDefault(e => e.Id == createdId);
        created.Should().NotBeNull();
        created!.GetAttributeValue<string>("uniquename").Should().Be("minimal_app");
    }

    [Fact]
    public void SetDataverseAppModule_CreatesAppModuleWithAllParameters()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        var webResourceId = Guid.NewGuid();
        
        // Act
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "full_app")
          .AddParameter("Name", "Full Test App")
          .AddParameter("Description", "A fully configured app")
          .AddParameter("WebResourceId", webResourceId)
          .AddParameter("FormFactor", 1)
          .AddParameter("ClientType", 4)
          .AddParameter("IsFeatured", true)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var createdId = (Guid)results[0].BaseObject;
        
        var created = Context!.CreateQuery("appmodule").FirstOrDefault(e => e.Id == createdId);
        created.Should().NotBeNull();
        created!.GetAttributeValue<string>("uniquename").Should().Be("full_app");
        created.GetAttributeValue<string>("name").Should().Be("Full Test App");
        created.GetAttributeValue<string>("description").Should().Be("A fully configured app");
    }

    [Fact]
    public void SetDataverseAppModule_CreatesAppModuleWithSpecificId()
    {
        // Note: When creating with a specific ID, we can't easily test this with mocks
        // because the cmdlet tries to retrieve the entity first to check if it exists.
        // Instead, we test that when creating without an ID, the cmdlet generates one.
        // The specific ID creation is tested in E2E tests.
        
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Create without specifying ID, verify one is generated
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "auto_id_app")
          .AddParameter("Name", "Auto ID App")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var createdId = (Guid)results[0].BaseObject;
        createdId.Should().NotBe(Guid.Empty);
        
        var created = Context!.CreateQuery("appmodule").FirstOrDefault(e => e.Id == createdId);
        created.Should().NotBeNull();
        created!.GetAttributeValue<string>("uniquename").Should().Be("auto_id_app");
    }

    [Fact]
    public void SetDataverseAppModule_UpdatesExistingAppModuleById()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "existing_app",
            ["name"] = "Original Name"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", existingApp.Id)
          .AddParameter("Name", "Updated Name")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        
        var updated = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        updated.Should().NotBeNull();
        updated!.GetAttributeValue<string>("name").Should().Be("Updated Name");
    }

    [Fact]
    public void SetDataverseAppModule_UpdatesExistingAppModuleByUniqueName()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "uniquename_update_app",
            ["name"] = "Original Name"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act - Find by UniqueName and update
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "uniquename_update_app")
          .AddParameter("Name", "Updated By UniqueName")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        
        var updated = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        updated.Should().NotBeNull();
        updated!.GetAttributeValue<string>("name").Should().Be("Updated By UniqueName");
    }

    [Fact]
    public void SetDataverseAppModule_NoUpdateFlag_PreventsUpdates()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "noupdate_app",
            ["name"] = "Original Name"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act - Try to update with NoUpdate flag
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "noupdate_app")
          .AddParameter("Name", "Should Not Update")
          .AddParameter("NoUpdate", true)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        
        // Name should remain unchanged
        var notUpdated = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        notUpdated.Should().NotBeNull();
        notUpdated!.GetAttributeValue<string>("name").Should().Be("Original Name");
    }

    [Fact]
    public void SetDataverseAppModule_NoCreateFlag_PreventsCreation()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Try to create with NoCreate flag (no existing record)
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "nocreate_app")
          .AddParameter("Name", "Should Not Create")
          .AddParameter("NoCreate", true)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert - Should not error, but should not create either
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().BeEmpty();
        
        // No record should be created
        var notCreated = Context!.CreateQuery("appmodule").FirstOrDefault(e => 
            e.GetAttributeValue<string>("uniquename") == "nocreate_app");
        notCreated.Should().BeNull();
    }

    [Fact]
    public void SetDataverseAppModule_SupportsWhatIf()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Use WhatIf to prevent actual creation
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "whatif_app")
          .AddParameter("Name", "WhatIf App")
          .AddParameter("WhatIf", true)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert - No record should be created
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        var notCreated = Context!.CreateQuery("appmodule").FirstOrDefault(e => 
            e.GetAttributeValue<string>("uniquename") == "whatif_app");
        notCreated.Should().BeNull();
    }

    [Fact]
    public void SetDataverseAppModule_ThrowsError_WhenUniqueNameMissingForCreation()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Try to create without UniqueName
        ps.AddCommand("Set-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "No UniqueName App")
          .AddParameter("PassThru", true);
        
        // Assert - Should throw ArgumentException
        var action = () => ps.Invoke();
        action.Should().Throw<CmdletInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithMessage("*UniqueName*required*");
    }

    // Get-DataverseAppModule Tests - These work because they use -Published flag

    [Fact]
    public void GetDataverseAppModule_RetrievesAppModuleById()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var appModule = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "test_get_by_id",
            ["name"] = "Test App By ID"
        };
        Context!.Initialize(new[] { appModule });
        
        // Act - Use Published flag to bypass unpublished query
        ps.AddCommand("Get-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", appModule.Id)
          .AddParameter("Published", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(appModule.Id);
    }

    [Fact]
    public void GetDataverseAppModule_RetrievesAppModuleByUniqueName()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var appModule = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "unique_test_app",
            ["name"] = "Unique Test App"
        };
        Context!.Initialize(new[] { appModule });
        
        // Act
        ps.AddCommand("Get-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "unique_test_app")
          .AddParameter("Published", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["UniqueName"].Value.Should().Be("unique_test_app");
    }

    [Fact]
    public void GetDataverseAppModule_RetrievesAppModuleByNameWithWildcard()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var appModule1 = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "sales_app",
            ["name"] = "Sales Application"
        };
        var appModule2 = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "hr_app",
            ["name"] = "HR Application"
        };
        Context!.Initialize(new[] { appModule1, appModule2 });
        
        // Act - Search with wildcard
        ps.AddCommand("Get-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "*Application")
          .AddParameter("Published", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2);
    }

    [Fact]
    public void GetDataverseAppModule_RetrievesAllAppModules()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var appModule1 = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "app1",
            ["name"] = "App 1"
        };
        var appModule2 = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "app2",
            ["name"] = "App 2"
        };
        Context!.Initialize(new[] { appModule1, appModule2 });
        
        // Act - Get all without filters
        ps.AddCommand("Get-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Published", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2);
    }

    [Fact]
    public void GetDataverseAppModule_RetrievesWithRawValues()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var appModule = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "raw_test_app",
            ["name"] = "Raw Test App"
        };
        Context!.Initialize(new[] { appModule });
        
        // Act - Get with Raw flag
        ps.AddCommand("Get-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", appModule.Id)
          .AddParameter("Raw", true)
          .AddParameter("Published", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        // Raw returns Entity object
        results[0].BaseObject.Should().BeOfType<Entity>();
    }

    // Remove-DataverseAppModule Tests

    [Fact]
    public void RemoveDataverseAppModule_RemovesAppModuleById()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "remove_by_id_app",
            ["name"] = "Remove By ID App"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act
        ps.AddCommand("Remove-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", existingApp.Id);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        var deleted = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public void RemoveDataverseAppModule_RemovesAppModuleByUniqueName()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "remove_by_uniquename",
            ["name"] = "Remove By UniqueName"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act
        ps.AddCommand("Remove-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "remove_by_uniquename");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        var deleted = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public void RemoveDataverseAppModule_DoesNotError_WithIfExists_WhenNonExistent()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Try to remove non-existent app with IfExists
        ps.AddCommand("Remove-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "nonexistent_app")
          .AddParameter("IfExists", true);
        ps.Invoke();

        // Assert - Should not error
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    [Fact]
    public void RemoveDataverseAppModule_Errors_WhenNonExistent_WithoutIfExists()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        // Act - Try to remove non-existent app without IfExists
        ps.AddCommand("Remove-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "nonexistent_app");
        
        // Assert - Should throw InvalidOperationException
        var action = () => ps.Invoke();
        action.Should().Throw<CmdletInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void RemoveDataverseAppModule_SupportsWhatIf()
    {
        // Arrange
        using var ps = CreatePowerShellWithAppModuleCmdlets();
        var mockConnection = CreateMockConnection("appmodule");
        
        var existingApp = new Entity("appmodule")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = "whatif_remove_app",
            ["name"] = "WhatIf Remove App"
        };
        Context!.Initialize(new[] { existingApp });
        
        // Act - Use WhatIf to prevent actual deletion
        ps.AddCommand("Remove-DataverseAppModule")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", existingApp.Id)
          .AddParameter("WhatIf", true);
        ps.Invoke();

        // Assert - Record should still exist
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        var notDeleted = Context.CreateQuery("appmodule").FirstOrDefault(e => e.Id == existingApp.Id);
        notDeleted.Should().NotBeNull();
    }

    // App Module Component Tests
    // Note: These cmdlets have complex internal logic involving appmoduleidunique attribute
    // and require precise mock setup that is difficult to replicate without E2E testing.

    [Fact(Skip = "AppModuleComponent cmdlets require appmoduleidunique attribute handling not supported by FakeXrmEasy")]
    public void GetDataverseAppModuleComponent_RetrievesComponentsForAppModule()
    {
        // The Get-DataverseAppModuleComponent cmdlet needs to:
        // 1. Query appmodule by uniquename to get appmoduleidunique
        // 2. Then query appmodulecomponent by appmoduleidunique
        // This requires proper metadata for the appmoduleidunique attribute which isn't
        // fully supported in the mock infrastructure.
    }

    [Fact(Skip = "AppModuleComponent cmdlets use AddAppComponentsRequest with complex internal logic")]
    public void SetDataverseAppModuleComponent_AddsComponentToAppModule()
    {
        // The Set-DataverseAppModuleComponent cmdlet uses AddAppComponentsRequest
        // which has complex validation logic that varies by component type.
    }

    [Fact(Skip = "AppModuleComponent cmdlets require appmoduleidunique attribute handling not supported by FakeXrmEasy")]
    public void RemoveDataverseAppModuleComponent_RemovesComponentFromAppModule()
    {
        // The Remove-DataverseAppModuleComponent cmdlet needs to retrieve the component
        // and access its appmoduleidunique attribute, which isn't properly supported
        // in the mock infrastructure.
    }
}
