using System;
using System.IO;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    /// <summary>
    /// Verifies that the loader routes SDK dependency assemblies to the folder that matches
    /// the current .NET runtime (e.g. <c>cmdlets/net10.0/</c> when running on .NET 10).
    ///
    /// The module manifest always loads the <c>net8.0</c> Loader and Cmdlets binaries
    /// (the only runtime variable allowed in a .psd1 restricted-language section is
    /// <c>$PSEdition</c>).  The Loader's <c>OnImport()</c> then detects
    /// <c>Environment.Version.Major</c> at runtime and sets <c>CmdletsLoadContext.basePath</c>
    /// to the matching TFM folder, falling back to <c>net8.0</c> if that folder does not exist.
    ///
    /// No Dataverse connection is required; the test only imports the module.
    /// </summary>
    [CrossPlatformTest]
    public class ModuleTfmRoutingTests
    {
        [Fact]
        public void SdkDependenciesLoadFromRuntimeMatchedTfmFolder()
        {
            Skip.If(
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TESTMODULEPATH")),
                "TESTMODULEPATH not set — cannot determine module location");

            // The PS script determines the expected TFM from the .NET runtime version (mirrors
            // the logic in ModuleInitProvider.OnImport), then asserts that the assemblies
            // loaded into the unnamed CmdletsLoadContext come from that folder.
            var script = @"
$runtimeMajor = [Environment]::Version.Major
$expectedTfm  = if ($runtimeMajor -ge 10) { ""net$runtimeMajor.0"" }
                elseif ($runtimeMajor -eq 9) { ""net9.0"" }
                else { ""net8.0"" }

# Resolve the module root so we can check whether the runtime-specific folder was built
$moduleRoot = Split-Path -Parent $moduleManifest
$tfmFolder  = Join-Path $moduleRoot ""cmdlets/$expectedTfm""
if (-not (Test-Path $tfmFolder)) {
    # Folder absent (e.g. partial build) — loader falls back to net8.0; adjust expectation
    $expectedTfm = ""net8.0""
}

Write-Host ""Runtime major : $runtimeMajor""
Write-Host ""Expected TFM  : $expectedTfm""
Write-Host ""TFM folder    : $tfmFolder""
Write-Host """"

# Enumerate every AssemblyLoadContext looking for SDK assemblies
$sdkAsms = [System.Runtime.Loader.AssemblyLoadContext]::All |
    ForEach-Object { $_.Assemblies } |
    Where-Object {
        $_.Location -and (
            $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' -or
            $_.GetName().Name -eq 'Microsoft.PowerPlatform.Dataverse.Client'
        )
    }

if (-not $sdkAsms) {
    throw 'No SDK assemblies found in any AssemblyLoadContext after module import'
}

$failures = @()
foreach ($asm in $sdkAsms) {
    $location = $asm.Location -replace '\\', '/'
    Write-Host ""  $($asm.GetName().Name) => $location""
    if ($location -notlike ""*/cmdlets/$expectedTfm/*"") {
        $failures += ""$($asm.GetName().Name): expected path containing 'cmdlets/$expectedTfm/' but got: $location""
    }
}

if ($failures.Count -gt 0) {
    throw ""TFM routing mismatch:`n$($failures -join ""`n"")""
}

Write-Host """"
Write-Host ""SUCCESS: all SDK assemblies loaded from cmdlets/$expectedTfm/""
";

            var result = PowerShellProcessRunner.Run(script, usePowerShellCore: true, timeoutSeconds: 120);

            result.StandardOutput.Should().Contain("SUCCESS",
                because: $"SDK dependencies should load from the runtime-matched TFM folder.\nFull output:\n{result.GetFullOutput()}");

            result.Success.Should().BeTrue(
                because: $"Script should exit cleanly.\nFull output:\n{result.GetFullOutput()}");
        }
    }
}
