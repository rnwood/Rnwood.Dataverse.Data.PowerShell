$ErrorActionPreference = "Stop"

Describe "Dynamic Plugin Assembly E2E Tests" {

    BeforeAll {
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        
        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can create, register, invoke, update and re-invoke a dynamic plugin assembly" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        $VerbosePreference = 'Continue'
        
        try {
            Write-Host "=== Dynamic Plugin Assembly E2E Test ==="
            
            # Connect to Dataverse
            Write-Host "Step 1: Connecting to Dataverse..."
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            Write-Host "✓ Connected"
            
            # Generate unique test identifiers
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 6)
            $assemblyName = "TestDynPlugin_${timestamp}_${testRunId}"
            $markerValue1 = "V1_${testRunId}"
            $markerValue2 = "V2_${testRunId}"
            $testEntityName = "new_e2edynplugin_${timestamp}_${testRunId}"
            
            Write-Host "Test assembly: $assemblyName"
            Write-Host "Marker V1: $markerValue1"
            Write-Host "Marker V2: $markerValue2"
            Write-Host "Test entity: $testEntityName"
            
            # Step 2: Create test entity
            Write-Host "Step 2: Creating test entity..."
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName $testEntityName `
                -SchemaName ("new_E2EDynPlugin_${timestamp}_${testRunId}") `
                -DisplayName "E2E Dynamic Plugin Test" `
                -DisplayCollectionName "E2E Dynamic Plugin Tests" `
                -PrimaryAttributeSchemaName "new_name" `
                -OwnershipType UserOwned `
                -Confirm:$false
            
            Write-Host "✓ Test entity created: $testEntityName"
            
            # Add a description field to the test entity
            Write-Host "Step 3: Adding description field to test entity..."
            Set-DataverseAttributeMetadata -Connection $connection `
                -EntityName $testEntityName `
                -AttributeName "new_description" `
                -SchemaName "new_Description" `
                -AttributeType Memo `
                -DisplayName "Description" `
                -MaxLength 2000 `
                -Confirm:$false
            
            Write-Host "✓ Description field added"
            
            # Wait for entity metadata to be published
            Write-Host "Waiting for entity metadata to publish..."
            Start-Sleep -Seconds 10
            
            # Create initial plugin source code that sets a marker field
            $pluginSourceV1 = @"
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
            
            trace.Trace("TestEntityPreCreatePlugin V1 executing");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                
                // Set a marker in description to prove plugin ran (V1)
                target["new_description"] = "$markerValue1";
                
                trace.Trace("Set new_description to: $markerValue1");
            }
        }
    }
}
"@

            # Step 4: Create dynamic plugin assembly from source
            Write-Host "Step 4: Creating dynamic plugin assembly from source..."
            $assembly = Set-DataverseDynamicPluginAssembly `
                -Connection $connection `
                -SourceCode $pluginSourceV1 `
                -Name $assemblyName `
                -Version "1.0.0.0" `
                -PassThru
            
            if (-not $assembly) {
                throw "Failed to create plugin assembly"
            }
            
            $assemblyId = $assembly.Id
            Write-Host "✓ Created plugin assembly: $assemblyId"
            
            # Step 5: Verify plugin type was auto-created
            Write-Host "Step 5: Verifying plugin type was auto-created..."
            $pluginTypes = @(Get-DataversePluginType -Connection $connection -PluginAssemblyId $assemblyId)
            
            if ($pluginTypes.Count -ne 1) {
                throw "Expected 1 plugin type, found $($pluginTypes.Count)"
            }
            
            $pluginType = $pluginTypes[0]
            Write-Host "✓ Found plugin type: $($pluginType.typename)"
            
            # Step 6: Register plugin step for PreCreate on test entity
            Write-Host "Step 6: Registering plugin step..."
            
            # Get the SDK message ID for "Create"
            $createMessage = Get-DataverseRecord -Connection $connection -TableName sdkmessage -FilterValues @{ name = 'Create' } -Columns sdkmessageid | Select-Object -First 1
            if (-not $createMessage) {
                throw "Could not find 'Create' SDK message"
            }
            
            # Get the SDK message filter ID for Create on test entity
            $messageFilter = Get-DataverseRecord -Connection $connection -TableName sdkmessagefilter `
                -FilterValues @{ sdkmessageid = $createMessage.sdkmessageid; primaryobjecttypecode = $testEntityName } `
                -Columns sdkmessagefilterid | Select-Object -First 1
            if (-not $messageFilter) {
                throw "Could not find message filter for Create on $testEntityName"
            }
            
            $stepId = Set-DataversePluginStep `
                -Connection $connection `
                -Name "Test Dynamic Plugin Step" `
                -PluginTypeId $pluginType.Id `
                -SdkMessageId $createMessage.sdkmessageid `
                -SdkMessageFilterId $messageFilter.sdkmessagefilterid `
                -Stage PreOperation `
                -Mode Synchronous `
                -PassThru | Select-Object -ExpandProperty Id
            
            Write-Host "✓ Registered plugin step: $stepId"
            
            # Wait for registration to propagate
            Write-Host "Waiting 5 seconds for plugin registration to propagate..."
            Start-Sleep -Seconds 5
            
            # Step 7: Create a record to trigger the plugin
            Write-Host "Step 7: Creating test record to trigger plugin V1..."
            $testRecord = @{
                "new_name" = "Test V1_$testRunId"
            }
            
            $recordId = Set-DataverseRecord -Connection $connection -TableName $testEntityName -InputObject $testRecord -PassThru | Select-Object -ExpandProperty Id
            Write-Host "✓ Created test record: $recordId"
            
            # Step 8: Verify plugin executed (check description field)
            Write-Host "Step 8: Verifying plugin V1 executed..."
            $record = Get-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId
            
            if ($record.new_description -ne $markerValue1) {
                throw "Plugin V1 did not execute correctly. Expected new_description='$markerValue1', got '$($record.new_description)'"
            }
            
            Write-Host "✓ Plugin V1 executed successfully! new_description = $($record.new_description)"
            
            # Step 9: Update plugin assembly with modified code
            Write-Host "Step 9: Updating plugin assembly with modified source (V2)..."
            $pluginSourceV2 = @"
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
            
            trace.Trace("TestEntityPreCreatePlugin V2 executing");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                
                // Set a different marker in description to prove updated plugin ran (V2)
                target["new_description"] = "$markerValue2";
                
                trace.Trace("Set new_description to: $markerValue2");
            }
        }
    }
}
"@

            $assemblyV2 = Set-DataverseDynamicPluginAssembly `
                -Connection $connection `
                -SourceCode $pluginSourceV2 `
                -Name $assemblyName `
                -Version "2.0.0.0" `
                -PassThru
            
            Write-Host "✓ Updated plugin assembly to version 2.0.0.0"
            
            # Wait for update to propagate
            Write-Host "Waiting 5 seconds for plugin update to propagate..."
            Start-Sleep -Seconds 5
            
            # Step 10: Create another record to trigger the UPDATED plugin
            Write-Host "Step 10: Creating test record to trigger plugin V2..."
            $testRecord2 = @{
                "new_name" = "Test V2_$testRunId"
            }
            
            $recordId2 = Set-DataverseRecord -Connection $connection -TableName $testEntityName -InputObject $testRecord2 -PassThru | Select-Object -ExpandProperty Id
            Write-Host "✓ Created test record: $recordId2"
            
            # Step 11: Verify UPDATED plugin executed with new behavior
            Write-Host "Step 11: Verifying plugin V2 executed with new behavior..."
            $record2 = Get-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId2
            
            if ($record2.new_description -ne $markerValue2) {
                throw "Plugin V2 did not execute correctly. Expected new_description='$markerValue2', got '$($record2.new_description)'"
            }
            
            Write-Host "✓ Plugin V2 executed successfully! new_description = $($record2.new_description)"
            
            # Step 12: Extract source from assembly to verify metadata (old approach)
            Write-Host "Step 12: Extracting source from updated assembly (legacy bytes approach)..."
            $retrievedAssembly = Get-DataversePluginAssembly -Connection $connection -Name $assemblyName
            $assemblyBytes = [Convert]::FromBase64String($retrievedAssembly.content)
            $metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes
            
            if (-not $metadata.SourceCode.Contains($markerValue2)) {
                throw "Extracted source does not contain V2 marker"
            }
            
            Write-Host "✓ Successfully extracted source code with V2 marker (legacy approach)"
            Write-Host "  Assembly Name: $($metadata.AssemblyName)"
            Write-Host "  Version: $($metadata.Version)"
            Write-Host "  Public Key Token: $($metadata.PublicKeyToken)"
            
            # Step 13: Test new connection-based retrieval by name
            Write-Host "Step 13: Testing connection-based retrieval by name..."
            $metadataByName = Get-DataverseDynamicPluginAssembly -Connection $connection -Name $assemblyName
            
            if (-not $metadataByName) {
                throw "Failed to retrieve metadata by name"
            }
            
            if (-not $metadataByName.SourceCode.Contains($markerValue2)) {
                throw "Retrieved metadata by name does not contain V2 marker"
            }
            
            if ($metadataByName.AssemblyName -ne $assemblyName) {
                throw "Assembly name mismatch. Expected '$assemblyName', got '$($metadataByName.AssemblyName)'"
            }
            
            Write-Host "✓ Successfully retrieved metadata by name"
            Write-Host "  Assembly Name: $($metadataByName.AssemblyName)"
            Write-Host "  Version: $($metadataByName.Version)"
            
            # Step 14: Test connection-based retrieval by ID
            Write-Host "Step 14: Testing connection-based retrieval by ID..."
            $metadataById = Get-DataverseDynamicPluginAssembly -Connection $connection -Id $assemblyId
            
            if (-not $metadataById) {
                throw "Failed to retrieve metadata by ID"
            }
            
            if (-not $metadataById.SourceCode.Contains($markerValue2)) {
                throw "Retrieved metadata by ID does not contain V2 marker"
            }
            
            if ($metadataById.AssemblyName -ne $assemblyName) {
                throw "Assembly name mismatch. Expected '$assemblyName', got '$($metadataById.AssemblyName)'"
            }
            
            Write-Host "✓ Successfully retrieved metadata by ID"
            
            # Step 15: Test VS project export by name
            Write-Host "Step 15: Testing VS project export by name..."
            $projectPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "E2E_VSProject_$testRunId")
            
            if (Test-Path $projectPath) {
                Remove-Item $projectPath -Recurse -Force
            }
            
            Get-DataverseDynamicPluginAssembly -Connection $connection -Name $assemblyName -OutputProjectPath $projectPath
            
            # Verify files were created
            $csprojPath = Join-Path $projectPath "$assemblyName.csproj"
            $csPath = Join-Path $projectPath "$assemblyName.cs"
            $snkPath = Join-Path $projectPath "$assemblyName.snk"
            
            if (-not (Test-Path $csprojPath)) {
                throw "Project file not created: $csprojPath"
            }
            
            if (-not (Test-Path $csPath)) {
                throw "Source file not created: $csPath"
            }
            
            if (-not (Test-Path $snkPath)) {
                throw "Key file not created: $snkPath"
            }
            
            # Verify source code content
            $sourceContent = Get-Content $csPath -Raw
            if (-not $sourceContent.Contains($markerValue2)) {
                throw "Exported source code does not contain V2 marker"
            }
            
            Write-Host "✓ Successfully exported VS project by name"
            Write-Host "  Project: $csprojPath"
            Write-Host "  Source: $csPath"
            Write-Host "  Key: $snkPath"
            
            # Step 16: Test default connection (set as default and use without -Connection)
            Write-Host "Step 16: Testing default connection usage..."
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Retrieve without -Connection parameter
            $metadataDefault = Get-DataverseDynamicPluginAssembly -Name $assemblyName
            
            if (-not $metadataDefault) {
                throw "Failed to retrieve metadata using default connection"
            }
            
            if ($metadataDefault.AssemblyName -ne $assemblyName) {
                throw "Assembly name mismatch with default connection"
            }
            
            Write-Host "✓ Successfully used default connection (no -Connection parameter)"
            
            # Cleanup project directory
            if (Test-Path $projectPath) {
                Remove-Item $projectPath -Recurse -Force
                Write-Host "✓ Cleaned up VS project directory"
            }
            
            # Cleanup
            Write-Host "Step 17: Cleaning up..."
            try {
                Remove-DataversePluginStep -Connection $connection -Id $stepId -Confirm:$false
                Write-Host "✓ Removed plugin step"
            } catch {
                Write-Warning "Failed to remove plugin step: $_"
            }
            
            try {
                Remove-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId -Confirm:$false
                Remove-DataverseRecord -Connection $connection -TableName $testEntityName -Id $recordId2 -Confirm:$false
                Write-Host "✓ Removed test records"
            } catch {
                Write-Warning "Failed to remove test records: $_"
            }
            
            try {
                Remove-DataversePluginAssembly -Connection $connection -Id $assemblyId -Confirm:$false
                Write-Host "✓ Removed plugin assembly"
            } catch {
                Write-Warning "Failed to remove assembly: $_"
            }
            
            try {
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $testEntityName -Confirm:$false
                Write-Host "✓ Removed test entity"
            } catch {
                Write-Warning "Failed to remove test entity: $_"
            }
            
            Write-Host ""
            Write-Host "=== ALL TESTS PASSED ===" -ForegroundColor Green
            Write-Host "✓ Created dynamic plugin assembly from C# source"
            Write-Host "✓ Plugin type auto-discovered and registered"
            Write-Host "✓ Plugin step created successfully"
            Write-Host "✓ Plugin V1 executed via real trigger"
            Write-Host "✓ Plugin assembly updated with new source code"
            Write-Host "✓ Plugin V2 executed with new behavior"
            Write-Host "✓ Source code successfully extracted from assembly (legacy approach)"
            Write-Host "✓ Connection-based retrieval by name works"
            Write-Host "✓ Connection-based retrieval by ID works"
            Write-Host "✓ VS project export by name works"
            Write-Host "✓ Default connection usage works (no -Connection parameter)"
            
        } catch {
            Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host $_.ScriptStackTrace
            throw
        }
    }
}
