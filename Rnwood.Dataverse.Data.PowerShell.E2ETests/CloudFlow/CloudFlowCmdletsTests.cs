using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.CloudFlow
{
    /// <summary>
    /// E2E tests for cloud flow cmdlets.
    /// Tests Get-DataverseCloudFlow, Set-DataverseCloudFlow, Remove-DataverseCloudFlow,
    /// and action manipulation cmdlets.
    /// </summary>
    public class CloudFlowCmdletsTests : E2ETestBase
    {
        [Fact]
        public void GetDataverseCloudFlow_Should_ReturnFlows()
        {
            var script = GetConnectionScript(@"
# Test Get-DataverseCloudFlow
Write-Host 'Testing Get-DataverseCloudFlow...'
$flows = Get-DataverseCloudFlow -Connection $connection
Write-Host ""Found $($flows.Count) cloud flows""

if ($flows.Count -eq 0) {
    throw 'Expected at least one cloud flow in the environment'
}

# Verify properties
$flow = $flows[0]
if (-not $flow.Id) {
    throw 'Flow missing Id property'
}
if (-not $flow.Name) {
    throw 'Flow missing Name property'
}
if (-not $flow.State) {
    throw 'Flow missing State property'
}
if ($flow.Category -ne 5) {
    throw ""Expected Category to be 5 (Modern Flow), got $($flow.Category)""
}

Write-Host 'Success: Get-DataverseCloudFlow returned flows with correct properties'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithNameFilter_Should_ReturnMatchingFlows()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$allFlows = Get-DataverseCloudFlow -Connection $connection
if ($allFlows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlowName = $allFlows[0].Name
Write-Host ""Testing with flow: $testFlowName""

# Test exact name match
$specificFlow = Get-DataverseCloudFlow -Connection $connection -Name $testFlowName
if ($specificFlow.Count -ne 1) {
    throw ""Expected 1 flow, got $($specificFlow.Count)""
}
if ($specificFlow.Name -ne $testFlowName) {
    throw ""Expected flow name '$testFlowName', got '$($specificFlow.Name)'""
}

Write-Host 'Success: Name filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithStateFilter_Should_FilterByState()
        {
            var script = GetConnectionScript(@"
# Test state filtering
Write-Host 'Testing state filters...'

$draftFlows = Get-DataverseCloudFlow -Connection $connection -Draft
Write-Host ""Found $($draftFlows.Count) draft flows""

$activatedFlows = Get-DataverseCloudFlow -Connection $connection -Activated
Write-Host ""Found $($activatedFlows.Count) activated flows""

# Verify all draft flows have State = 'Draft'
foreach ($flow in $draftFlows) {
    if ($flow.State -ne 'Draft') {
        throw ""Expected draft flow to have State='Draft', got '$($flow.State)'""
    }
}

# Verify all activated flows have State = 'Activated'
foreach ($flow in $activatedFlows) {
    if ($flow.State -ne 'Activated') {
        throw ""Expected activated flow to have State='Activated', got '$($flow.State)'""
    }
}

Write-Host 'Success: State filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithIncludeClientData_Should_ReturnClientData()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlowId = $flows[0].Id
Write-Host ""Testing with flow ID: $testFlowId""

# Get flow without clientdata
$flowWithoutData = Get-DataverseCloudFlow -Connection $connection -Id $testFlowId
if ($flowWithoutData.ClientData) {
    throw 'Expected ClientData to be null when not requested'
}

# Get flow with clientdata
$flowWithData = Get-DataverseCloudFlow -Connection $connection -Id $testFlowId -IncludeClientData
if (-not $flowWithData.ClientData) {
    throw 'Expected ClientData to be present when requested'
}

Write-Host ""ClientData length: $($flowWithData.ClientData.Length) characters""
Write-Host 'Success: IncludeClientData parameter works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_Should_ReturnActions()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
Write-Host ""Testing actions for flow: $($testFlow.Name) (ID: $($testFlow.Id))""

# Get actions
$actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id
Write-Host ""Found $($actions.Count) actions""

if ($actions.Count -gt 0) {
    # Verify properties
    $action = $actions[0]
    if (-not $action.ActionId) {
        throw 'Action missing ActionId property'
    }
    if (-not $action.Name) {
        throw 'Action missing Name property'
    }
    if (-not $action.Type) {
        throw 'Action missing Type property'
    }
    if ($action.FlowId -ne $testFlow.Id) {
        throw ""Action FlowId mismatch: expected $($testFlow.Id), got $($action.FlowId)""
    }
    
    Write-Host 'Success: Get-DataverseCloudFlowAction returned actions with correct properties'
} else {
    Write-Host 'SKIP: No actions found in flow'
}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_WithActionNameFilter_Should_FilterActions()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
$allActions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id

if ($allActions.Count -eq 0) {
    Write-Host 'SKIP: No actions found in flow'
    exit 0
}

$firstActionName = $allActions[0].Name
Write-Host ""Testing with action: $firstActionName""

# Test exact name match
$specificAction = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id -ActionName $firstActionName
if ($specificAction.Count -ne 1) {
    throw ""Expected 1 action, got $($specificAction.Count)""
}
if ($specificAction.Name -ne $firstActionName) {
    throw ""Expected action name '$firstActionName', got '$($specificAction.Name)'""
}

Write-Host 'Success: Action name filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_ByFlowName_Should_Work()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
Write-Host ""Testing with flow: $($testFlow.Name)""

# Get actions by flow name
$actions = Get-DataverseCloudFlowAction -Connection $connection -FlowName $testFlow.Name
Write-Host ""Found $($actions.Count) actions""

# Verify all actions have the correct flow ID
foreach ($action in $actions) {
    if ($action.FlowId -ne $testFlow.Id) {
        throw ""Action FlowId mismatch: expected $($testFlow.Id), got $($action.FlowId)""
    }
    if ($action.FlowName -ne $testFlow.Name) {
        throw ""Action FlowName mismatch: expected $($testFlow.Name), got $($action.FlowName)""
    }
}

Write-Host 'Success: Get-DataverseCloudFlowAction by FlowName works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void SetDataverseCloudFlow_WhatIf_Should_NotModifyFlow()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
$originalDesc = $testFlow.Description
Write-Host ""Testing WhatIf with flow: $($testFlow.Name)""

# Try to update with WhatIf
Set-DataverseCloudFlow -Connection $connection -Id $testFlow.Id -Description 'Test description' -WhatIf

# Verify flow was not modified
$afterWhatIf = Get-DataverseCloudFlow -Connection $connection -Id $testFlow.Id
if ($afterWhatIf.Description -ne $originalDesc) {
    throw ""WhatIf modified the flow! Original: '$originalDesc', After: '$($afterWhatIf.Description)'""
}

Write-Host 'Success: WhatIf does not modify flows'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void CloudFlowCmdlets_HelpIsAvailable()
        {
            var script = $@"
{GetModuleImportStatement()}

$cmdlets = @(
    'Get-DataverseCloudFlow',
    'Set-DataverseCloudFlow',
    'Remove-DataverseCloudFlow',
    'Get-DataverseCloudFlowAction',
    'Set-DataverseCloudFlowAction',
    'Remove-DataverseCloudFlowAction'
)

$issues = @()

foreach ($cmdletName in $cmdlets) {{
    Write-Host ""Testing help for $cmdletName""
    $help = Get-Help $cmdletName -Full
    
    if (-not $help.Name) {{
        $issues += ""$cmdletName`: Missing Name""
    }}
    
    if (-not $help.Syntax) {{
        $issues += ""$cmdletName`: Missing Syntax""
    }}
    
    if (-not $help.Examples) {{
        $issues += ""$cmdletName`: Missing Examples""
    }}
    
    # Verify help has parameters
    if (-not $help.Parameters) {{
        $issues += ""$cmdletName`: Missing Parameters section""
    }}
}}

if ($issues.Count -gt 0) {{
    throw ""Help validation issues found:`n$($issues -join ""`n"")""
}}

Write-Host 'Success: All cloud flow cmdlets have complete help'
";

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
