using Xunit;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class ModuleLoadingTests : TestBase
{
    [Fact(Skip = "Requires spawning PowerShell process - integration test")]
    public void Module_CanBeLoadedSuccessfully_WhenNotAlreadyLoaded()
    {
        // This test validates that the module can be imported successfully
        // when SDK assemblies are not already loaded
        // 
        // Original Pester test spawned a child PowerShell process:
        // pwsh -noninteractive -noprofile -command {
        //     Import-Module Rnwood.Dataverse.Data.PowerShell
        //     # Verify Microsoft.Xrm.Sdk assembly loaded
        // }
        //
        // This is an integration test that should run in Pester or E2E tests
    }

    [Fact(Skip = "Requires spawning PowerShell process - integration test")]
    public void Module_CanBeLoadedSuccessfully_WhenSdkAssembliesAlreadyLoaded()
    {
        // This test validates that the custom AssemblyLoadContext (net8.0)
        // or AssemblyResolve handler (net462) correctly loads the SDK
        // assemblies from the module's cmdlets folder even when different
        // versions are already loaded in the AppDomain
        //
        // Original Pester test:
        // 1. Pre-loaded Microsoft.Xrm.Sdk.dll from module's cmdlets folder
        // 2. Then imported the module
        // 3. Verified module loaded successfully without assembly conflicts
        //
        // This tests the critical Loader project functionality
        // This is an integration test that should run in Pester or E2E tests
    }
}
