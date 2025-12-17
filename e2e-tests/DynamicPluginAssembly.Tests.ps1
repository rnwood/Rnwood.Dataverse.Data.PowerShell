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
            
            Write-Host "Test assembly: $assemblyName"
            Write-Host "Marker V1: $markerValue1"
            Write-Host "Marker V2: $markerValue2"
            
            # Create initial plugin source code that sets a marker field
            $pluginSourceV1 = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestDynamicPlugins
{
    public class ContactPreCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace("ContactPreCreatePlugin V1 executing");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                
                // Set a marker in description to prove plugin ran (V1)
                target["description"] = "$markerValue1";
                
                trace.Trace("Set description to: $markerValue1");
            }
        }
    }
}
"@

            # Step 2: Create dynamic plugin assembly from source
            Write-Host "Step 2: Creating dynamic plugin assembly from source..."
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
            
            # Step 3: Verify plugin type was auto-created
            Write-Host "Step 3: Verifying plugin type was auto-created..."
            $pluginTypes = @(Get-DataversePluginType -Connection $connection -PluginAssemblyId $assemblyId)
            
            if ($pluginTypes.Count -ne 1) {
                throw "Expected 1 plugin type, found $($pluginTypes.Count)"
            }
            
            $pluginType = $pluginTypes[0]
            Write-Host "✓ Found plugin type: $($pluginType.typename)"
            
            # Step 4: Register plugin step for PreCreate on contact
            Write-Host "Step 4: Registering plugin step..."
            
            # Get the SDK message ID for "Create"
            $createMessage = Get-DataverseRecord -Connection $connection -TableName sdkmessage -FilterValues @{ name = 'Create' } -Columns sdkmessageid | Select-Object -First 1
            if (-not $createMessage) {
                throw "Could not find 'Create' SDK message"
            }
            
            # Get the SDK message filter ID for Create on contact
            $messageFilter = Get-DataverseRecord -Connection $connection -TableName sdkmessagefilter `
                -FilterValues @{ sdkmessageid = $createMessage.sdkmessageid; primaryobjecttypecode = 'contact' } `
                -Columns sdkmessagefilterid | Select-Object -First 1
            if (-not $messageFilter) {
                throw "Could not find message filter for Create on contact"
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
            
            # Step 5: Create a contact to trigger the plugin
            Write-Host "Step 5: Creating contact to trigger plugin V1..."
            $testContact = @{
                "firstname" = "DynTest"
                "lastname" = "V1_$testRunId"
            }
            
            $contactId = Set-DataverseRecord -Connection $connection -TableName contact -InputObject $testContact -PassThru | Select-Object -ExpandProperty Id
            Write-Host "✓ Created contact: $contactId"
            
            # Step 6: Verify plugin executed (check description field)
            Write-Host "Step 6: Verifying plugin V1 executed..."
            $contact = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId
            
            if ($contact.description -ne $markerValue1) {
                throw "Plugin V1 did not execute correctly. Expected description='$markerValue1', got '$($contact.description)'"
            }
            
            Write-Host "✓ Plugin V1 executed successfully! Description = $($contact.description)"
            
            # Step 7: Update plugin assembly with modified code
            Write-Host "Step 7: Updating plugin assembly with modified source (V2)..."
            $pluginSourceV2 = @"
using System;
using Microsoft.Xrm.Sdk;

namespace TestDynamicPlugins
{
    public class ContactPreCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            trace.Trace("ContactPreCreatePlugin V2 executing");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var target = (Entity)context.InputParameters["Target"];
                
                // Set a different marker in description to prove updated plugin ran (V2)
                target["description"] = "$markerValue2";
                
                trace.Trace("Set description to: $markerValue2");
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
            
            # Step 8: Create another contact to trigger the UPDATED plugin
            Write-Host "Step 8: Creating contact to trigger plugin V2..."
            $testContact2 = @{
                "firstname" = "DynTest"
                "lastname" = "V2_$testRunId"
            }
            
            $contactId2 = Set-DataverseRecord -Connection $connection -TableName contact -InputObject $testContact2 -PassThru | Select-Object -ExpandProperty Id
            Write-Host "✓ Created contact: $contactId2"
            
            # Step 9: Verify UPDATED plugin executed with new behavior
            Write-Host "Step 9: Verifying plugin V2 executed with new behavior..."
            $contact2 = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId2
            
            if ($contact2.description -ne $markerValue2) {
                throw "Plugin V2 did not execute correctly. Expected description='$markerValue2', got '$($contact2.description)'"
            }
            
            Write-Host "✓ Plugin V2 executed successfully! Description = $($contact2.description)"
            
            # Step 10: Extract source from assembly to verify metadata
            Write-Host "Step 10: Extracting source from updated assembly..."
            $retrievedAssembly = Get-DataversePluginAssembly -Connection $connection -Name $assemblyName
            $assemblyBytes = [Convert]::FromBase64String($retrievedAssembly.content)
            $metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes
            
            if (-not $metadata.SourceCode.Contains($markerValue2)) {
                throw "Extracted source does not contain V2 marker"
            }
            
            Write-Host "✓ Successfully extracted source code with V2 marker"
            Write-Host "  Assembly Name: $($metadata.AssemblyName)"
            Write-Host "  Version: $($metadata.Version)"
            Write-Host "  Public Key Token: $($metadata.PublicKeyToken)"
            
            # Cleanup
            Write-Host "Step 11: Cleaning up..."
            try {
                Remove-DataversePluginStep -Connection $connection -Id $stepId -Confirm:$false
                Write-Host "✓ Removed plugin step"
            } catch {
                Write-Warning "Failed to remove plugin step: $_"
            }
            
            try {
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId2 -Confirm:$false
                Write-Host "✓ Removed test contacts"
            } catch {
                Write-Warning "Failed to remove contacts: $_"
            }
            
            try {
                Remove-DataversePluginAssembly -Connection $connection -Id $assemblyId -Confirm:$false
                Write-Host "✓ Removed plugin assembly"
            } catch {
                Write-Warning "Failed to remove assembly: $_"
            }
            
            Write-Host ""
            Write-Host "=== ALL TESTS PASSED ===" -ForegroundColor Green
            Write-Host "✓ Created dynamic plugin assembly from C# source"
            Write-Host "✓ Plugin type auto-discovered and registered"
            Write-Host "✓ Plugin step created successfully"
            Write-Host "✓ Plugin V1 executed via real trigger"
            Write-Host "✓ Plugin assembly updated with new source code"
            Write-Host "✓ Plugin V2 executed with new behavior"
            Write-Host "✓ Source code successfully extracted from assembly"
            
        } catch {
            Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host $_.ScriptStackTrace
            throw
        }
    }
}
