using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Plugin
{
    /// <summary>
    /// Tests for dynamic plugin assembly creation, update, and invocation.
    /// Converted from e2e-tests/DynamicPluginAssembly.Tests.ps1
    /// </summary>
    public class DynamicPluginAssemblyTests : E2ETestBase
    {
[Fact]
        public void CanCreateRegisterInvokeUpdateAndReInvokeDynamicPluginAssembly()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    Write-Host '=== Dynamic Plugin Assembly E2E Test ==='
    
    # Generate unique test identifiers
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 6)
    $assemblyName = ""TestDynPlugin_${timestamp}_${testRunId}""
    $markerValue1 = ""V1_${testRunId}""
    $markerValue2 = ""V2_${testRunId}""
    $testEntityName = ""new_e2edynplugin_${timestamp}_${testRunId}""
    
    Write-Host ""Test assembly: $assemblyName""
    Write-Host ""Test entity: $testEntityName""
    
    # Create test entity
    Write-Host 'Step 2: Creating test entity...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $testEntityName `
            -SchemaName (""new_E2EDynPlugin_${timestamp}_${testRunId}"") `
            -DisplayName 'E2E Dynamic Plugin Test' `
            -DisplayCollectionName 'E2E Dynamic Plugin Tests' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entity created'
    
    # Add description field
    Write-Host 'Step 3: Adding description field...'
    Invoke-WithRetry {
        Set-DataverseAttributeMetadata -Connection $connection `
            -EntityName $testEntityName `
            -AttributeName 'new_description' `
            -SchemaName 'new_Description' `
            -AttributeType Memo `
            -DisplayName 'Description' `
            -MaxLength 2000 `
            -Confirm:$false
    }
    Write-Host '✓ Description field added'
    Start-Sleep -Seconds 10
    
    # Create plugin V1
    $pluginSourceV1 = @""
using System;
using Microsoft.Xrm.Sdk;

namespace TestDynamicPlugins
{
    public class TestEntityPreCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace(""TestEntityPreCreatePlugin V1 executing"");
            
            if (context.InputParameters.Contains(""Target"") && context.InputParameters[""Target""] is Entity)
            {
                var target = (Entity)context.InputParameters[""Target""];
                target[""new_description""] = ""$markerValue1"";
                trace.Trace(""Set new_description to: $markerValue1"");
            }
        }
    }
}
""@

    # Create dynamic plugin assembly
    Write-Host 'Step 4: Creating dynamic plugin assembly...'
    $assembly = Invoke-WithRetry {
        Set-DataverseDynamicPluginAssembly `
            -Connection $connection `
            -SourceCode $pluginSourceV1 `
            -Name $assemblyName `
            -Version '1.0.0.0' `
            -PassThru
    }
    $assemblyId = $assembly.Id
    Write-Host ""✓ Created plugin assembly: $assemblyId""
    
    # Verify plugin type
    Write-Host 'Step 5: Verifying plugin type...'
    $pluginTypes = @(Get-DataversePluginType -Connection $connection -PluginAssemblyId $assemblyId)
    
    if ($pluginTypes.Count -ne 1) {
        throw ""Expected 1 plugin type, found $($pluginTypes.Count)""
    }
    
    $pluginType = $pluginTypes[0]
    Write-Host ""✓ Found plugin type: $($pluginType.typename)""
    
    # Register plugin step
    Write-Host 'Step 6: Registering plugin step...'
    
    $createMessage = Get-DataverseRecord -Connection $connection -TableName sdkmessage -FilterValues @{ name = 'Create' } -Columns sdkmessageid | Select-Object -First 1
    $messageFilter = Get-DataverseRecord -Connection $connection -TableName sdkmessagefilter `
        -FilterValues @{ sdkmessageid = $createMessage.sdkmessageid; primaryobjecttypecode = $testEntityName } `
        -Columns sdkmessagefilterid | Select-Object -First 1
    
    $stepId = Invoke-WithRetry {
        Set-DataversePluginStep `
            -Connection $connection `
            -Name 'Test Dynamic Plugin Step' `
            -PluginTypeId $pluginType.Id `
            -SdkMessageId $createMessage.sdkmessageid `
            -SdkMessageFilterId $messageFilter.sdkmessagefilterid `
            -Stage PreOperation `
            -Mode Synchronous `
            -PassThru | Select-Object -ExpandProperty Id
    }
    Write-Host ""✓ Registered plugin step: $stepId""
    Start-Sleep -Seconds 5
    
    # Create test record to trigger plugin V1
    Write-Host 'Step 7: Creating test record to trigger plugin V1...'
    $testRecord = @{
        'new_name' = ""Test V1_$testRunId""
    }
    
    $recordId = Set-DataverseRecord -Connection $connection -TableName $testEntityName -InputObject $testRecord -PassThru | Select-Object -ExpandProperty Id
    Write-Host ""✓ Created test record: $recordId""
    
    # Verify plugin executed
    Write-Host 'Step 8: Verifying plugin V1 executed...'
    $record = Get-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId
    
    if ($record.new_description -ne $markerValue1) {
        throw ""Plugin V1 did not execute correctly. Expected '$markerValue1', got '$($record.new_description)'""
    }
    
    Write-Host ""✓ Plugin V1 executed successfully! new_description = $($record.new_description)""
    
    # Update plugin to V2
    Write-Host 'Step 9: Updating plugin assembly to V2...'
    $pluginSourceV2 = @""
using System;
using Microsoft.Xrm.Sdk;

namespace TestDynamicPlugins
{
    public class TestEntityPreCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace(""TestEntityPreCreatePlugin V2 executing"");
            
            if (context.InputParameters.Contains(""Target"") && context.InputParameters[""Target""] is Entity)
            {
                var target = (Entity)context.InputParameters[""Target""];
                target[""new_description""] = ""$markerValue2"";
                trace.Trace(""Set new_description to: $markerValue2"");
            }
        }
    }
}
""@

    $assemblyV2 = Invoke-WithRetry {
        Set-DataverseDynamicPluginAssembly `
            -Connection $connection `
            -SourceCode $pluginSourceV2 `
            -Name $assemblyName `
            -Version '2.0.0.0' `
            -PassThru
    }
    Write-Host '✓ Updated plugin assembly to version 2.0.0.0'
    Start-Sleep -Seconds 5
    
    # Create another record to trigger plugin V2
    Write-Host 'Step 10: Creating test record to trigger plugin V2...'
    $testRecord2 = @{
        'new_name' = ""Test V2_$testRunId""
    }
    
    $recordId2 = Set-DataverseRecord -Connection $connection -TableName $testEntityName -InputObject $testRecord2 -PassThru | Select-Object -ExpandProperty Id
    Write-Host ""✓ Created test record: $recordId2""
    
    # Verify updated plugin executed
    Write-Host 'Step 11: Verifying plugin V2 executed with new behavior...'
    $record2 = Get-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId2
    
    if ($record2.new_description -ne $markerValue2) {
        throw ""Plugin V2 did not execute correctly. Expected '$markerValue2', got '$($record2.new_description)'""
    }
    
    Write-Host ""✓ Plugin V2 executed successfully! new_description = $($record2.new_description)""
    
    # Extract source from assembly
    Write-Host 'Step 12: Extracting source from updated assembly...'
    $retrievedAssembly = Get-DataversePluginAssembly -Connection $connection -Name $assemblyName
    $assemblyBytes = [Convert]::FromBase64String($retrievedAssembly.content)
    $metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes
    
    if (-not $metadata.SourceCode.Contains($markerValue2)) {
        throw 'Extracted source does not contain V2 marker'
    }
    
    Write-Host '✓ Successfully extracted source code with V2 marker'
    
    # Cleanup
    Write-Host 'Step 13: Cleaning up...'
    Invoke-WithRetry {
        Remove-DataversePluginStep -Connection $connection -Id $stepId -Confirm:$false -ErrorAction SilentlyContinue
        Remove-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId -Confirm:$false -ErrorAction SilentlyContinue
        Remove-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId2 -Confirm:$false -ErrorAction SilentlyContinue
        Remove-DataversePluginAssembly -Connection $connection -Id $assemblyId -Confirm:$false -ErrorAction SilentlyContinue
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $testEntityName -Confirm:$false -ErrorAction SilentlyContinue
    }
    
    Write-Host ''
    Write-Host '=== ALL TESTS PASSED ===' -ForegroundColor Green
    Write-Host 'Success'
} catch {
    Write-ErrorDetails $_
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("ALL TESTS PASSED");
        }
    }
}
