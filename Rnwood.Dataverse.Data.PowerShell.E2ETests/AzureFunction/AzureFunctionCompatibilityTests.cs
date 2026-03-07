using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.AzureFunction
{
    /// <summary>
    /// Tests that validate the module works correctly in an Azure Functions-like environment.
    /// These tests simulate the assembly pre-loading that Azure Functions performs before
    /// running user PowerShell scripts, which was the root cause of issue #168.
    ///
    /// In Azure Functions (PowerShell worker), the Azure Functions SDK pre-loads its own
    /// version of common assemblies (e.g., Microsoft.Extensions.Logging.Abstractions 8.0.0)
    /// before the user module is imported. Without the ALC fix, this caused a FileLoadException
    /// (0x80131621 FUSION_E_REF_DEF_MISMATCH) when our module tried to load the same assembly
    /// from a different path.
    ///
    /// These tests do NOT require real Dataverse credentials - they only test module loading.
    /// </summary>
    public class AzureFunctionCompatibilityTests
    {
        /// <summary>
        /// Gets the PowerShell statement to import the module (same logic as E2ETestBase).
        /// </summary>
        private static string GetModuleImportStatement()
        {
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            if (!string.IsNullOrWhiteSpace(modulePath))
            {
                var manifestPath = Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
                return $"Import-Module '{manifestPath.Replace("'", "''")}' -Force -ErrorAction Stop";
            }
            return "Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop";
        }

        /// <summary>
        /// Gets the NuGet global packages path for locating Azure Functions SDK assemblies.
        /// </summary>
        private static string GetNugetPackagesPath()
        {
            // Check environment variable first (common in CI)
            var nugetPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrWhiteSpace(nugetPackages) && Directory.Exists(nugetPackages))
            {
                return nugetPackages;
            }

            // Default location: ~/.nuget/packages
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages");
        }

        /// <summary>
        /// Validates that the module loads correctly when Microsoft.Extensions.Logging.Abstractions
        /// has been pre-loaded from the Azure Functions SDK path (same AssemblyVersion=8.0.0.0
        /// but from a different physical path than our bundled version).
        ///
        /// This test reproduces the exact scenario from issue #168 where users got:
        ///   FileLoadException: Could not load file or assembly
        ///   'Microsoft.Extensions.Logging.Abstractions, Version=8.0.0.0'
        /// </summary>
        [Fact]
        public void ModuleLoads_WhenAzureFunctionsLoggingAssemblyPreLoaded()
        {
            // Azure Functions Worker SDK 2.x uses Microsoft.Extensions.Logging.Abstractions 8.0.2
            // (AssemblyVersion=8.0.0.0) while our module bundles version 8.0.3 (also AssemblyVersion=8.0.0.0).
            // Both have the same assembly identity but different physical file paths,
            // which caused the FileLoadException (0x80131621) in issue #168.
            var nugetPath = GetNugetPackagesPath();
            var azFuncLoggingPath = Path.Combine(nugetPath,
                "microsoft.extensions.logging.abstractions", "8.0.2", "lib", "net8.0",
                "Microsoft.Extensions.Logging.Abstractions.dll");

            Skip.If(!File.Exists(azFuncLoggingPath),
                $"Azure Functions SDK assembly not found at {azFuncLoggingPath}. " +
                "Run 'dotnet restore' to download packages.");

            var importStatement = GetModuleImportStatement();

            // In string.Format(), $variable is not interpolated by C# and is passed literally to PowerShell.
            // Only {0}, {1} etc. placeholders are replaced with C# values.
            var script = string.Format(@"
# =====================================================================================
# Simulate Azure Functions PowerShell worker environment (issue #168 reproduction)
# =====================================================================================
# Azure Functions loads its SDK assemblies BEFORE user modules are imported.
# Without the ALC fix, this caused FileLoadException (0x80131621).

Write-Host 'Step 1: Simulating Azure Functions runtime pre-loading its SDK assemblies...'

# Azure Functions Worker pre-loads Microsoft.Extensions.Logging.Abstractions from its SDK path
# This is version 8.0.2, AssemblyVersion=8.0.0.0 - same identity as our bundled 8.0.3
$azFuncLoggingPath = '{0}'
$preloadedAsm = [System.Reflection.Assembly]::LoadFrom($azFuncLoggingPath)
Write-Host ""  Pre-loaded: $($preloadedAsm.FullName)""
Write-Host ""  From: $($preloadedAsm.Location)""

Write-Host ''
Write-Host 'Step 2: Importing Rnwood.Dataverse.Data.PowerShell module...'
Write-Host '  (Simulating module import via requirements.psd1 in Azure Functions)'

{1}

Write-Host '  Module imported successfully!'
Write-Host ''
Write-Host 'Step 3: Verifying cmdlets are available...'

# Verify Get-DataverseConnection is available - the cmdlet that failed in issue #168
$cmd = Get-Command Get-DataverseConnection -ErrorAction Stop
Write-Host ""  Get-DataverseConnection: available""
$cmd2 = Get-Command Get-DataverseRecord -ErrorAction Stop
Write-Host ""  Get-DataverseRecord: available""
$cmd3 = Get-Command Set-DataverseRecord -ErrorAction Stop
Write-Host ""  Set-DataverseRecord: available""

Write-Host ''
Write-Host 'SUCCESS: Module works correctly in Azure Functions environment.'
Write-Host 'The ALC fix (issue #168) is working as expected.'
", azFuncLoggingPath.Replace("'", "''"), importStatement);

            var result = PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: 120, importModule: false);

            result.Success.Should().BeTrue(
                $"Module should load in Azure Functions environment without FileLoadException.\n{result.GetFullOutput()}");
            result.StandardOutput.Should().Contain("SUCCESS",
                because: result.GetFullOutput());
            result.StandardOutput.Should().Contain("Get-DataverseConnection: available",
                because: result.GetFullOutput());
        }

        /// <summary>
        /// Validates that the module also loads correctly when multiple Azure Functions
        /// SDK assemblies are pre-loaded, simulating a more complete Azure Functions environment.
        /// </summary>
        [Fact]
        public void ModuleLoads_WhenMultipleAzureFunctionsSdkAssembliesPreLoaded()
        {
            var nugetPath = GetNugetPackagesPath();

            // Azure Functions Worker SDK 2.x dependency versions
            var sdkAssembliesToPreload = new[]
            {
                Path.Combine(nugetPath, "microsoft.extensions.logging.abstractions", "8.0.2", "lib", "net8.0",
                    "Microsoft.Extensions.Logging.Abstractions.dll"),
                Path.Combine(nugetPath, "microsoft.extensions.logging", "8.0.1", "lib", "net8.0",
                    "Microsoft.Extensions.Logging.dll"),
                Path.Combine(nugetPath, "microsoft.extensions.dependencyinjection.abstractions", "8.0.2", "lib", "net8.0",
                    "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
            };

            // Check at least the primary assembly (logging abstractions) is available
            Skip.If(!File.Exists(sdkAssembliesToPreload[0]),
                $"Azure Functions SDK assemblies not found. Run 'dotnet restore' to download packages.");

            // Build pre-load statements for all available assemblies
            var preloadLines = new StringBuilder();
            foreach (var dllPath in sdkAssembliesToPreload)
            {
                if (File.Exists(dllPath))
                {
                    preloadLines.AppendLine($"[System.Reflection.Assembly]::LoadFrom('{dllPath.Replace("'", "''")}') | Out-Null");
                    preloadLines.AppendLine($"Write-Host '  Pre-loaded: {Path.GetFileName(dllPath)}'");
                }
            }

            var importStatement = GetModuleImportStatement();
            var script = string.Format(@"
Write-Host 'Pre-loading Azure Functions SDK assemblies...'
{0}

Write-Host 'Importing module...'
{1}

Write-Host 'Verifying all main cmdlets available...'
$cmdlets = @('Get-DataverseConnection', 'Get-DataverseRecord', 'Set-DataverseRecord',
  'Remove-DataverseRecord', 'Invoke-DataverseSql')
foreach ($cmdletName in $cmdlets) {{
    $cmd = Get-Command $cmdletName -ErrorAction Stop
    Write-Host ""  $($cmd.Name): OK""
}}

Write-Host 'SUCCESS: All cmdlets available after Azure Functions SDK pre-loading.'
", preloadLines.ToString(), importStatement);

            var result = PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: 120, importModule: false);

            result.Success.Should().BeTrue(
                $"Module should load with multiple Azure Functions SDK assemblies pre-loaded.\n{result.GetFullOutput()}");
            result.StandardOutput.Should().Contain("SUCCESS",
                because: result.GetFullOutput());
        }
    }
}
