using System;
using System.IO;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    /// <summary>
    /// Tests demonstrating that the module works correctly when run inside an Azure Functions
    /// PowerShell 7.4 worker that pre-loads conflicting assembly versions — specifically
    /// <see href="https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues/168">issue #168</see>.
    ///
    /// The Azure Functions PS 7.4 worker ships Microsoft.Extensions.Logging.Abstractions v6.0.11
    /// (AssemblyVersion=6.0.0.0). The module requires v8.0.0.0. Without the fix in this PR the
    /// Default.Resolving handler's SemVer version parsing threw FormatException for the bundled
    /// "8.0.13+eba..." ProductVersion string, returned null, and .NET fell back to DEFAULT ALC
    /// where it found the incompatible v6.x — resulting in FileLoadException (0x80131621).
    ///
    /// The fix strips SemVer suffixes before parsing, successfully routes the assembly to
    /// CmdletsLoadContext, and the module works despite the conflict in DEFAULT ALC.
    /// </summary>
    [CrossPlatformTest]
    public class AzureFunctionsCompatibilityTests : E2ETestBase
    {
        private static readonly string? FuncWorkerPs74Dir = FindFuncWorkerPs74Dir();

        private static string? FindFuncWorkerPs74Dir()
        {
            // Check common locations for Azure Functions Core Tools PS 7.4 worker.
            // The subdirectory structure is: <tools-root>/bin/in-proc8/workers/powershell/7.4
            const string workerRelativePath = "bin/in-proc8/workers/powershell/7.4";
            const string workerRelativePathWin = @"bin\in-proc8\workers\powershell\7.4";

            var searchRoots = new System.Collections.Generic.List<string>
            {
                // Homebrew (macOS)
                "/opt/homebrew/lib/node_modules/azure-functions-core-tools",
                // npm global (common Linux)
                "/usr/lib/node_modules/azure-functions-core-tools",
                // npm global (common Linux — user install)
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".npm-global/lib/node_modules/azure-functions-core-tools"),
                // Windows: npm global install (AppData\Roaming\npm)
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"npm\node_modules\azure-functions-core-tools"),
                // Windows: Chocolatey
                @"C:\ProgramData\chocolatey\lib\azure-functions-core-tools\tools",
            };

            // Also try to discover from `npm root -g` (handles non-standard npm prefix including GitHub Actions)
            try
            {
                var npmRoot = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "root -g",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if (npmRoot != null)
                {
                    var root = npmRoot.StandardOutput.ReadToEnd().Trim();
                    npmRoot.WaitForExit();
                    if (!string.IsNullOrEmpty(root))
                    {
                        searchRoots.Insert(0, Path.Combine(root, "azure-functions-core-tools"));
                    }
                }
            }
            catch { /* npm not available or failed — continue */ }

            var workerSuffix = Path.DirectorySeparatorChar == '\\' ? workerRelativePathWin : workerRelativePath;
            foreach (var toolsRoot in searchRoots)
            {
                var candidatePath = Path.Combine(toolsRoot, workerSuffix);
                if (Directory.Exists(candidatePath) &&
                    File.Exists(Path.Combine(candidatePath, "Microsoft.Extensions.Logging.Abstractions.dll")))
                {
                    return candidatePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Verifies that the module can be imported and Get-DataverseConnection can be called
        /// even when an older version of Microsoft.Extensions.Logging.Abstractions is pre-loaded
        /// in DEFAULT ALC — simulating the Azure Functions PS 7.4 worker environment.
        ///
        /// This test reproduces issue #168: the stable module (v2.20.0) fails with
        /// FileLoadException (0x80131621) because its Default.Resolving handler cannot parse
        /// the SemVer ProductVersion "8.0.13+eba..." and returns null, causing .NET to find the
        /// incompatible v6.x already loaded in DEFAULT ALC.
        ///
        /// The fix: strip the SemVer suffix before parsing, successfully route to CmdletsLoadContext.
        /// </summary>
        [SkippableFact]
        public void ModuleWorksWhenAzureFunctionsWorkerHasConflictingAssemblyPreloaded()
        {
            Skip.If(FuncWorkerPs74Dir == null,
                "Azure Functions Core Tools not found. Install with: npm install -g azure-functions-core-tools@4");

            var conflictingDll = Path.Combine(FuncWorkerPs74Dir!, "Microsoft.Extensions.Logging.Abstractions.dll");

            // GetConnectionScript() already includes Import-Module and $connection = Get-DataverseConnection.
            // We prepend the Azure Functions simulation (pre-loading the conflicting assembly) BEFORE those
            // calls to accurately replicate what happens in the real Azure Functions PS 7.4 worker where
            // the worker process has already loaded its own assemblies before run.ps1 runs.
            // importModule: false prevents PowerShellProcessRunner from adding a duplicate Import-Module.
            var connectionScript = GetConnectionScript($@"
Write-Host 'Module loaded successfully despite ALC conflict'
Write-Host 'Connection established: SUCCESS'
");

            var fullScript = $@"
# Simulate Azure Functions PS 7.4 worker startup: pre-load conflicting assembly version.
# The Azure Functions worker ships Microsoft.Extensions.Logging.Abstractions v6.0.11
# (AssemblyVersion=6.0.0.0) but the module requires v8.0.0.0. This is exactly the conflict
# that caused issue #168 (FileLoadException 0x80131621) on the stable module.
$conflictingDll = '{conflictingDll.Replace("'", "''")}'
$preloaded = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($conflictingDll)
Write-Host ""[AzureFunc simulation] Pre-loaded: $($preloaded.GetName().Name) v$($preloaded.GetName().Version) into DEFAULT ALC""

{connectionScript}
";

            // importModule: false because connectionScript (from GetConnectionScript) already contains
            // the Import-Module statement; we don't want PowerShellProcessRunner to add a duplicate.
            var result = PowerShellProcessRunner.Run(
                fullScript,
                importModule: false,
                timeoutSeconds: 120);

            result.Success.Should().BeTrue(
                because: $"Module should work in Azure Functions environment (issue #168 fix). " +
                         $"Full output:\n{result.GetFullOutput()}");
            result.StandardOutput.Should().Contain("Connection established: SUCCESS",
                because: result.GetFullOutput());
        }
    }
}
