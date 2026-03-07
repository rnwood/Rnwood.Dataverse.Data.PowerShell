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

# Detect PowerShell edition and select appropriate cmdlets directory
# Desktop = Windows PowerShell 5.1 (.NET Framework) → net462
# Core = PowerShell 7+ (.NET Core/8+) → net8.0
$targetFramework = if ($PSVersionTable.PSEdition -eq 'Core') { 'net8.0' } else { 'net462' }
Write-Verbose ""Detected PowerShell edition: $($PSVersionTable.PSEdition), using target framework: $targetFramework""

# Pre-load SDK assembly from module path to simulate preexisting load
# Note: Add-Type may throw ReflectionTypeLoadException because some types in the SDK
# have dependencies that aren't loaded yet, but the assembly itself still gets loaded
# into the AppDomain. We suppress this expected error.
$assemblyPath = Join-Path $modulePath ""cmdlets/$targetFramework/Microsoft.Xrm.Sdk.dll""
if (-not (Test-Path $assemblyPath)) { throw ""SDK assembly not found at $assemblyPath"" }
$resolvedAssemblyPath = (Resolve-Path $assemblyPath).Path
try {
    Add-Type -Path $resolvedAssemblyPath -ErrorAction Stop
} catch [System.Reflection.ReflectionTypeLoadException] {
    # Expected error - assembly is still loaded despite some types failing to load
    Write-Verbose ""ReflectionTypeLoadException caught (expected): $($_.Exception.Message)""
}

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

    [Fact]
    public void Module_CanBeLoadedSuccessfully_WhenConflictingDependencyVersionAlreadyLoaded()
    {
        // This test reproduces the Azure Functions bug where the Functions worker pre-loads
        // Microsoft.Extensions.Logging.Abstractions v6.0.0.0 and the Dataverse module
        // requires v8.0.0.0. The module's Default.Resolving hook must handle ALL assemblies
        // in the cmdlets directory, not just the 3 previously hardcoded ones.
        // See: https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues/<n>

        EnsureModulePath();

        // The conflict fixture (v6.0.0.0) is only copied for net8.0 builds
        var fixtureDir = Path.GetDirectoryName(typeof(ModuleLoadingTests).Assembly.Location)!;
        var conflictingDllPath = Path.Combine(fixtureDir, "ConflictTestFixtures",
            "Microsoft.Extensions.Logging.Abstractions.v6.0.0.0.dll");

        if (!File.Exists(conflictingDllPath))
        {
            return; // Skip on net462 builds (fixture not copied there)
        }

        var script = $@"
# Module manifest and path are provided by PowerShellProcessRunner
if (-not (Test-Path $moduleManifest)) {{ throw ""Module manifest not found at $moduleManifest"" }}

# Simulate Azure Functions worker: pre-load an OLDER version of a module dependency.
# The Functions PS 7.4 worker ships with Microsoft.Extensions.Logging.Abstractions v6.0.0.0
# while the Dataverse SDK requires v8.0.0.0. Before the fix, this caused a FileLoadException
# in ProcessRecord because the Default.Resolving hook didn't handle this assembly.
$conflictingDllPath = '{conflictingDllPath.Replace("'", "''")}'
if (-not (Test-Path $conflictingDllPath)) {{ throw ""Conflict fixture DLL not found at $conflictingDllPath"" }}

# Pre-load v6.0.0.0 to simulate what the Azure Functions worker does at startup
$preLoaded = [System.Reflection.Assembly]::LoadFrom($conflictingDllPath)
if ($preLoaded.GetName().Version.Major -ne 6) {{ throw ""Expected v6 fixture, got $($preLoaded.GetName().Version)"" }}

# Import the module - should succeed even though v6 is already loaded
Import-Module $moduleManifest -Force -ErrorAction Stop

# Verify the module loaded
$module = Get-Module Rnwood.Dataverse.Data.PowerShell
if (-not $module) {{ throw 'Module was not imported' }}

# Trigger ProcessRecord which is where the FileLoadException was thrown.
# Use a mock connection so no real Dataverse environment is needed.
# The key test: this must NOT throw FileLoadException - any other error (e.g. connection
# error, invalid argument) means the assembly conflict was resolved successfully.
try {{
    Get-DataverseConnection -Url 'https://localhost.invalid' -ClientId ([guid]::Empty.ToString()) -ClientSecret 'test' -ErrorAction Stop
}} catch [System.IO.FileLoadException] {{
    throw ""BUG: FileLoadException during ProcessRecord - assembly conflict not resolved: $($_.Exception.Message)""
}} catch {{
    # Any other exception (connection failed, invalid URL, etc.) is expected and means
    # the assembly conflict was resolved. The cmdlet executed far enough to fail on the
    # connection rather than on assembly loading.
    Write-Verbose ""Expected non-FileLoadException: $($_.Exception.GetType().Name)""
}}

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script, importModule: false);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }


    private static void EnsureModulePath()
    {
        // Check if TESTMODULEPATH is already set (e.g., by CI)
        var existingPath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
        if (!string.IsNullOrEmpty(existingPath))
        {
            if (!Directory.Exists(existingPath))
            {
                throw new InvalidOperationException($"TESTMODULEPATH is set to {existingPath}, but directory does not exist.");
            }
            return; // Already set, nothing to do
        }
        
        // Not set - try to find it using solution root
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
