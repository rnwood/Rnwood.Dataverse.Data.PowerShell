using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Linq;
using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Solution-related cmdlets:
/// - Get-DataverseSolution
/// - Import-DataverseSolution
/// - Export-DataverseSolution
/// - Get-DataverseSolutionFile
/// - Get-DataverseSolutionDependency
/// </summary>
public class SolutionsTests : TestBase
{
    private new PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolution", typeof(GetDataverseSolutionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolutionFile", typeof(GetDataverseSolutionFileCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolutionDependency", typeof(GetDataverseSolutionDependencyCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Import-DataverseSolution", typeof(ImportDataverseSolutionCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    /// <summary>
    /// Creates a minimal solution zip file with a valid solution.xml structure.
    /// </summary>
    private static byte[] CreateSolutionZipBytes(string uniqueName, string friendlyName, string version, bool isManaged = false)
    {
        var solutionXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{uniqueName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""{friendlyName}"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions>
      <Description description=""Test solution description"" languagecode=""1033"" />
    </Descriptions>
    <Version>{version}</Version>
    <Managed>{(isManaged ? "1" : "0")}</Managed>
    <Publisher>
      <UniqueName>publisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""Test Publisher"" languagecode=""1033"" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
    <RootComponents />
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("solution.xml");
            using (var entryStream = entry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(solutionXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates solution and publisher records in the mock context.
    /// </summary>
    private Entity CreateSolutionEntity(string uniqueName, string friendlyName, string version, bool isManaged)
    {
        // Create publisher first
        var publisherId = Guid.NewGuid();
        var publisher = new Entity("publisher", publisherId)
        {
            ["friendlyname"] = "Test Publisher",
            ["uniquename"] = "testpublisher",
            ["customizationprefix"] = "test"
        };
        Service!.Create(publisher);

        // Create solution
        var solution = new Entity("solution")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = uniqueName,
            ["friendlyname"] = friendlyName,
            ["version"] = version,
            ["ismanaged"] = isManaged,
            ["description"] = "Test description",
            ["publisherid"] = new EntityReference("publisher", publisherId)
        };
        Service!.Create(solution);
        return solution;
    }

    // Get-DataverseSolution Wildcard Tests (Get-DataverseSolution-Wildcards.Tests.ps1 - 5 tests)

    [Fact]
    public void GetDataverseSolution_UsesEqualOperator_ForExactNameNoWildcards()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution", "My Solution", "1.0.0.0", false);
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "MySolution");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0].BaseObject as SolutionInfo;
        result.Should().NotBeNull();
        result!.UniqueName.Should().Be("MySolution");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithPercent_ForWildcardPatternWithAsterisk()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", false);

        // Act - * should translate to % for SQL Like
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().NotContain("OtherSolution");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithUnderscore_ForWildcardPatternWithQuestionMark()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("MySolution12", "My Solution 12", "1.0.0.0", false);

        // Act - ? should translate to _ for SQL Like (matches exactly one character)
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "MySolution?");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().NotContain("MySolution12");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithBothPercentAndUnderscore_ForCombinedWildcards()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("MyOtherSolution3", "My Other Solution 3", "1.0.0.0", false);
        CreateSolutionEntity("TheirSolution4", "Their Solution 4", "1.0.0.0", false);

        // Act - combine * and ? wildcards
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*Solution?");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().Contain("MyOtherSolution3");
        solutions.Should().NotContain("TheirSolution4");
    }

    [Fact]
    public void GetDataverseSolution_CombinesWildcardFilterWithManagedFilter()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false); // unmanaged
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", true);  // managed
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", true); // managed

        // Act - filter by wildcard name AND managed
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*")
          .AddParameter("Managed", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0].BaseObject as SolutionInfo;
        result.Should().NotBeNull();
        result!.UniqueName.Should().Be("MySolution2");
        result.IsManaged.Should().BeTrue();
    }

    // Get-DataverseSolutionFile Tests (Get-DataverseSolutionFile.Tests.ps1 - 2 tests)

    [Fact]
    public void GetDataverseSolutionFile_ParsesSolutionFileAndReturnsMetadata()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.2.3.4", false);
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, solutionBytes);

            // Act
            ps.AddCommand("Get-DataverseSolutionFile")
              .AddParameter("Path", tempFile);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var solutionInfo = results[0].BaseObject as SolutionFileInfo;
            solutionInfo.Should().NotBeNull();
            solutionInfo!.UniqueName.Should().Be("TestSolution");
            solutionInfo.Name.Should().Be("Test Solution");
            solutionInfo.Version.Should().Be(new Version(1, 2, 3, 4));
            solutionInfo.IsManaged.Should().BeFalse();
            solutionInfo.PublisherUniqueName.Should().Be("publisher");
            solutionInfo.PublisherName.Should().Be("Test Publisher");
            solutionInfo.PublisherPrefix.Should().Be("test");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetDataverseSolutionFile_ReturnsError_ForMissingFile()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");

        // Act
        ps.AddCommand("Get-DataverseSolutionFile")
          .AddParameter("Path", nonExistentPath);
        
        // The cmdlet throws a terminating error
        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        
        // Assert
        exception.InnerException.Should().BeOfType<FileNotFoundException>();
    }

    // Get-DataverseSolutionDependency Tests (Get-DataverseSolutionDependency.Tests.ps1 - 10 tests)

    [Fact]
    public void GetDataverseSolutionDependency_RetrievesMissingDependencies_WithMissingSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var dependencyEntities = new EntityCollection();
        dependencyEntities.Entities.Add(new Entity("dependency")
        {
            Id = Guid.NewGuid(),
            ["dependentcomponenttype"] = new OptionSetValue(1),
            ["requiredcomponenttype"] = new OptionSetValue(2)
        });

        var mockConnection = CreateMockConnection(
            request =>
            {
                if (request is RetrieveMissingDependenciesRequest)
                {
                    var response = new RetrieveMissingDependenciesResponse();
                    response.Results["EntityCollection"] = dependencyEntities;
                    return response;
                }
                return null;
            },
            "contact");

        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseSolutionDependency_AcceptsSolutionUniqueNameFromPipeline()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Create a PSObject with UniqueName property to simulate pipeline input
        var pipelineInput = new PSObject();
        pipelineInput.Properties.Add(new PSNoteProperty("SolutionUniqueName", "TestSolution"));

        // Act - Accept input from pipeline via property name
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Missing", true);
        var results = ps.Invoke(new[] { pipelineInput });

        // Assert
        ps.HadErrors.Should().BeFalse();
        // Default interceptor returns empty collection
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_SupportsUniqueNameAlias()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act - use UniqueName alias instead of SolutionUniqueName
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        // Default interceptor returns empty collection
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_ReturnsEmptyCollection_WhenNoMissingDependencies()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_RetrievesUninstallDependencies_WithUninstallSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var dependencyEntities = new EntityCollection();
        dependencyEntities.Entities.Add(new Entity("dependency")
        {
            Id = Guid.NewGuid(),
            ["dependentcomponenttype"] = new OptionSetValue(1),
            ["requiredcomponenttype"] = new OptionSetValue(2)
        });

        var mockConnection = CreateMockConnection(
            request =>
            {
                if (request is RetrieveDependenciesForUninstallRequest)
                {
                    var response = new RetrieveDependenciesForUninstallResponse();
                    response.Results["EntityCollection"] = dependencyEntities;
                    return response;
                }
                return null;
            },
            "contact");

        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Uninstall", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseSolutionDependency_ReturnsEmptyCollection_WhenNoUninstallDependencies()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Uninstall", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty();
    }

    // Import-DataverseSolution Version Check Tests (Import-DataverseSolution-VersionChecks.Tests.ps1 - 7 tests)

    [Fact]
    public void ImportDataverseSolution_ExtractsVersionFromSolutionFileCorrectly()
    {
        // This test verifies that Get-DataverseSolutionFile correctly extracts version
        // The Import-DataverseSolution uses the same extraction logic
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "2.5.1.100", false);
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, solutionBytes);

            ps.AddCommand("Get-DataverseSolutionFile")
              .AddParameter("Path", tempFile);
            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse();
            var solutionInfo = results[0].BaseObject as SolutionFileInfo;
            solutionInfo!.Version.Should().Be(new Version(2, 5, 1, 100));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ImportDataverseSolution_SkipIfSameVersionParameter_Exists()
    {
        // Verify the parameter exists by checking cmdlet metadata
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("SkipIfSameVersion");
        property.Should().NotBeNull();
        var paramAttr = property!.GetCustomAttributes(typeof(ParameterAttribute), false).FirstOrDefault() as ParameterAttribute;
        paramAttr.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_SkipIfSameVersionParameter_HasHelpMessage()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("SkipIfSameVersion");
        var paramAttr = property!.GetCustomAttributes(typeof(ParameterAttribute), false).FirstOrDefault() as ParameterAttribute;
        paramAttr!.HelpMessage.Should().NotBeNullOrEmpty();
        paramAttr.HelpMessage.Should().Contain("same");
    }

    [Fact]
    public void ImportDataverseSolution_SkipIfLowerVersionParameter_Exists()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("SkipIfLowerVersion");
        property.Should().NotBeNull();
        var paramAttr = property!.GetCustomAttributes(typeof(ParameterAttribute), false).FirstOrDefault() as ParameterAttribute;
        paramAttr.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_SkipIfLowerVersionParameter_HasHelpMessage()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("SkipIfLowerVersion");
        var paramAttr = property!.GetCustomAttributes(typeof(ParameterAttribute), false).FirstOrDefault() as ParameterAttribute;
        paramAttr!.HelpMessage.Should().NotBeNullOrEmpty();
        paramAttr.HelpMessage.Should().Contain("lower");
    }

    [Fact(Skip = "Requires E2E testing - async import with full Dataverse environment")]
    public void ImportDataverseSolution_DetectsSameVersionAndSkips_WithWarning()
    {
        // This requires full async import infrastructure which needs a real environment
    }

    [Fact(Skip = "Requires E2E testing - async import with full Dataverse environment")]
    public void ImportDataverseSolution_DetectsLowerVersionAndSkips_WithWarning()
    {
        // This requires full async import infrastructure which needs a real environment
    }

    // Import-DataverseSolution Component Update Tests (Import-DataverseSolution-SkipWithComponentUpdate.Tests.ps1 - 6 tests)

    [Fact]
    public void ImportDataverseSolution_ConnectionReferencesParameter_Exists()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("ConnectionReferences");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(System.Collections.Hashtable));
    }

    [Fact]
    public void ImportDataverseSolution_EnvironmentVariablesParameter_Exists()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("EnvironmentVariables");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(System.Collections.Hashtable));
    }

    [Fact(Skip = "Requires E2E testing - async import with connection reference update")]
    public void ImportDataverseSolution_UpdatesConnectionReferences_WhenProvided()
    {
        // This requires full async import infrastructure which needs a real environment
    }

    [Fact(Skip = "Requires E2E testing - async import with environment variable update")]
    public void ImportDataverseSolution_UpdatesEnvironmentVariables_WhenProvided()
    {
        // This requires full async import infrastructure which needs a real environment
    }

    // Import-DataverseSolution Null Reference Tests (Import-DataverseSolution-NullReferenceException.Tests.ps1 - 2 tests)

    [Fact(Skip = "Requires E2E testing - async import with empty solution")]
    public void ImportDataverseSolution_DoesNotThrowNullReferenceException_ForNewSolutionWithNoComponents()
    {
        // Tests handling of solution files with no components - requires real environment
    }

    [Fact(Skip = "Requires E2E testing - async import with version matching")]
    public void ImportDataverseSolution_DoesNotThrowNullReferenceException_WithUseUpdateIfVersionMajorMinorMatches()
    {
        // Tests -UseUpdateIfVersionMajorMinorMatches flag - requires real environment
    }

    // Import-DataverseSolution Holding Solution Tests (Import-DataverseSolution-HoldingSolutionVersionCheck.Tests.ps1 - 3 tests)

    [Fact]
    public void ImportDataverseSolution_HoldingSolutionMode_Exists()
    {
        var cmdletType = typeof(ImportDataverseSolutionCmdlet);
        var property = cmdletType.GetProperty("Mode");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(ImportMode));

        // Verify HoldingSolution is a valid mode
        Enum.IsDefined(typeof(ImportMode), ImportMode.HoldingSolution).Should().BeTrue();
    }

    [Fact(Skip = "Requires E2E testing - holding solution import logic")]
    public void ImportDataverseSolution_SkipsImport_WhenHoldingSolutionExistsWithSameVersion()
    {
        // Tests skipping when holding solution already imported - requires real environment
    }

    [Fact(Skip = "Requires E2E testing - holding solution import logic")]
    public void ImportDataverseSolution_FailsWithClearError_WhenHoldingSolutionExistsWithDifferentVersion()
    {
        // Tests error when holding solution version mismatch - requires real environment
    }

    [Fact(Skip = "Requires E2E testing - holding solution import logic")]
    public void ImportDataverseSolution_ProceedsWithHoldingSolutionImport_WhenNoHoldingSolutionExists()
    {
        // Tests normal import when no holding solution exists - requires real environment
    }

    // Import-DataverseSolution Component Parameter Filtering Tests (Import-DataverseSolution-ComponentParameterFiltering.Tests.ps1 - 3 tests)

    [Fact(Skip = "Requires E2E testing - async import with filtered parameters")]
    public void ImportDataverseSolution_DoesNotThrow_WhenExtraConnectionReferencesProvided()
    {
        // Tests that extra connection references (not in solution) are ignored - requires real environment
    }

    [Fact(Skip = "Requires E2E testing - async import with filtered parameters")]
    public void ImportDataverseSolution_DoesNotThrow_WhenExtraEnvironmentVariablesProvided()
    {
        // Tests that extra environment variables (not in solution) are ignored - requires real environment
    }

    [Fact(Skip = "Requires E2E testing - async import with all extra parameters")]
    public void ImportDataverseSolution_Handles_AllProvidedParametersNotInSolution()
    {
        // Tests graceful handling when all provided parameters don't exist in solution - requires real environment
    }

    // Import-DataverseSolution Case Insensitive Tests (Import-DataverseSolution-CaseInsensitive.Tests.ps1 - ~1 test)

    [Fact(Skip = "Requires E2E testing - async import with case insensitive matching")]
    public void ImportDataverseSolution_ConnectionReferencesAndEnvVars_AreCaseInsensitive()
    {
        // Tests that schema names are matched case-insensitively - requires real environment
    }
}
