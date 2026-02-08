using FluentAssertions;
using Xunit;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.IO;
using System;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class ModuleLoadingTests : TestBase
{
    [Fact]
    public void Module_CanBeLoadedSuccessfully_WhenNotAlreadyLoaded()
    {
        EnsureModulePath();

        var script = @"
# Module is imported by PowerShellProcessRunner before this script executes
$module = Get-Module Rnwood.Dataverse.Data.PowerShell
if (-not $module) { throw 'Module was not imported' }

$sdkAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } |
    Select-Object -First 1

if (-not $sdkAssembly) { throw 'Microsoft.Xrm.Sdk assembly not loaded' }

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void Module_CanBeLoadedSuccessfully_WhenSdkAssembliesAlreadyLoaded()
    {
        EnsureModulePath();

        var script = @"
# Module manifest and path are provided by PowerShellProcessRunner
if (-not (Test-Path $moduleManifest)) { throw ""Module manifest not found at $moduleManifest"" }
if (-not (Test-Path $modulePath)) { throw ""Module path not found at $modulePath"" }

# Pre-load SDK assembly from module path to simulate preexisting load
$assemblyPath = Join-Path $modulePath 'cmdlets/net8.0/Microsoft.Xrm.Sdk.dll'
$resolvedAssemblyPath = (Resolve-Path $assemblyPath).Path
Add-Type -Path $resolvedAssemblyPath

# Import module after SDK assembly is already loaded
Import-Module $moduleManifest -Force -ErrorAction Stop

$sdkAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } |
    Select-Object -First 1

if (-not $sdkAssembly) { throw 'Microsoft.Xrm.Sdk assembly not loaded after import' }
if ($sdkAssembly.Location -ne $resolvedAssemblyPath) { throw ""Assembly path mismatch: $($sdkAssembly.Location)"" }

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script, importModule: false);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    private static void EnsureModulePath()
    {
        var solutionRoot = FindSolutionRoot();
        var defaultPath = Path.Combine(solutionRoot, "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");

        if (!Directory.Exists(defaultPath))
        {
            throw new InvalidOperationException($"Expected module output at {defaultPath}. Build the module or set TESTMODULEPATH explicitly.");
        }

        Environment.SetEnvironmentVariable("TESTMODULEPATH", defaultPath);
    }

    private static string FindSolutionRoot()
    {
        var slnName = "Rnwood.Dataverse.Data.PowerShell.sln";
        var startDirs = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(Path.GetDirectoryName(typeof(ModuleLoadingTests).Assembly.Location)!)
        };

        foreach (var start in startDirs)
        {
            for (var dir = start; dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, slnName);
                if (File.Exists(candidate))
                {
                    return dir.FullName;
                }
            }
        }

        throw new InvalidOperationException($"Could not locate solution root containing {slnName} starting from {Directory.GetCurrentDirectory()} and {typeof(ModuleLoadingTests).Assembly.Location}.");
    }
}
