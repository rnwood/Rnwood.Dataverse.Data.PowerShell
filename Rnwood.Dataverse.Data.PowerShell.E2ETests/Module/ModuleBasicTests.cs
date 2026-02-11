using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    /// <summary>
    /// Module infrastructure and help system tests.
    /// Tests related to module loading, help files, and documentation.
    /// </summary>
    public class ModuleBasicTests : E2ETestBase
    {
        [Fact]
        public void AllCmdletsHaveHelpAvailable()
        {
            var script = $@"
{GetModuleImportStatement()}

$$cmdlets = Get-Command -Module Rnwood.Dataverse.Data.PowerShell
Write-Host ""Testing help for $$($cmdlets.Count) cmdlets""

$$cmdletsWithoutHelp = @()
foreach ($$cmdlet in $$cmdlets) {{
    $$help = Get-Help $$cmdlet.Name -ErrorAction SilentlyContinue
    if (-not $$help) {{
        $$cmdletsWithoutHelp += $$cmdlet.Name
    }}
}}

if ($$cmdletsWithoutHelp.Count -gt 0) {{
    throw ""The following cmdlets do not have help available: $$($$cmdletsWithoutHelp -join ', ')""
}}

Write-Host 'Success: All cmdlets have help available'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void HelpContentReflectsHelpFilesWithExpectedStructure()
        {
            var script = $@"
{GetModuleImportStatement()}

# Test a sample of important cmdlets to ensure help structure is correct
$$testCmdlets = @(
    'Get-DataverseConnection',
    'Get-DataverseRecord',
    'Set-DataverseRecord',
    'Remove-DataverseRecord',
    'Invoke-DataverseRequest',
    'Invoke-DataverseSql',
    'Get-DataverseWhoAmI'
)

$$issues = @()

foreach ($$cmdletName in $$testCmdlets) {{
    Write-Host ""Testing help for $$cmdletName""
    $$help = Get-Help $$cmdletName -Full
    
    # Verify help has a name
    if (-not $$help.Name) {{
        $$issues += ""$${{cmdletName}}: Missing Name""
    }}
    
    # Verify help has syntax information
    if (-not $$help.Syntax) {{
        $$issues += ""$${{cmdletName}}: Missing Syntax""
    }}
    
    # Verify help has parameters
    if (-not $$help.Parameters) {{
        $$issues += ""$${{cmdletName}}: Missing Parameters section""
    }}
    
    # For cmdlets with parameters, verify parameter details exist
    if ($$help.Parameters -and $$help.Parameters.Parameter) {{
        $$paramCount = @($$help.Parameters.Parameter).Count
        Write-Host ""  - Found $$paramCount parameters""
        
        # Verify at least one parameter has description
        $$paramsWithDescription = @($$help.Parameters.Parameter | Where-Object {{ 
            ($$_.Description -is [string] -and $$_.Description) -or 
            ($$_.Description.Text -and $$_.Description.Text)
        }})
        if ($$paramsWithDescription.Count -eq 0) {{
            $$issues += ""$${{cmdletName}}: No parameters have descriptions""
        }}
    }}
}}

if ($$issues.Count -gt 0) {{
    throw ""Help validation issues found:`n$$($$issues -join ""`n"")""
}}

Write-Host 'Success: All tested cmdlets have proper help structure'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void HelpFilesExistInEnGBDirectory()
        {
            var script = $@"
{GetModuleImportStatement()}

# Check that the module directory has the en-GB help files
$$modulePath = (Get-Module Rnwood.Dataverse.Data.PowerShell).ModuleBase
$$helpPath = Join-Path $$modulePath 'en-GB'

if (-not (Test-Path $$helpPath)) {{
    throw ""Help directory not found at: $$helpPath""
}}

$$helpFiles = Get-ChildItem -Path $$helpPath -Filter '*.xml'
Write-Host ""Found $$($$helpFiles.Count) help files in en-GB directory""

if ($$helpFiles.Count -eq 0) {{
    throw ""No help XML files found in $$helpPath""
}}

# Verify the main help file exists
$$mainHelpFile = Join-Path $$helpPath 'Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml'
if (-not (Test-Path $$mainHelpFile)) {{
    throw ""Main help file not found: $$mainHelpFile""
}}

Write-Host 'Success: Help files exist and are accessible'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
