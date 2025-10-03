# Cmdlet Generator for Dataverse SDK Request Types
# This tool generates PowerShell cmdlet wrapper classes for SDK requests

param(
    [Parameter(Mandatory=$false)]
    [string]$RequestName,
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateAll,
    
    [Parameter(Mandatory=$false)]
    [int]$BatchSize = 50
)

$ErrorActionPreference = "Stop"

# Load SDK assembly
$sdkPath = "./Rnwood.Dataverse.Data.PowerShell.Cmdlets/bin/Release/net6.0"
$dllPath = Join-Path $sdkPath "Microsoft.Crm.Sdk.Proxy.dll"

if (-not (Test-Path $dllPath)) {
    Write-Error "SDK DLL not found. Please build the project first: dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj"
    exit 1
}

$assembly = [System.Reflection.Assembly]::LoadFrom($dllPath)

# List of already implemented requests
$implemented = @(
    "AssignRequest", "SetStateRequest", "ExecuteWorkflowRequest",
    "AssociateRequest", "DisassociateRequest", "GrantAccessRequest",
    "RevokeAccessRequest", "AddMembersTeamRequest", "RemoveMembersTeamRequest",
    "PublishXmlRequest", "SendEmailRequest", "LockSalesOrderPricingRequest",
    "UnlockSalesOrderPricingRequest", "LockInvoicePricingRequest",
    "UnlockInvoicePricingRequest", "MergeRequest", "RouteToRequest"
)

function Get-PowerShellVerb {
    param([string]$Name)
    
    $verbMap = @{
        "Add" = "Add"
        "Remove" = "Remove"
        "Get" = "Get"
        "Retrieve" = "Get"
        "Set" = "Set"
        "Update" = "Update"
        "New" = "New"
        "Create" = "New"
        "Delete" = "Remove"
        "Send" = "Send"
        "Publish" = "Publish"
        "Unpublish" = "Unpublish"
        "Import" = "Import"
        "Export" = "Export"
        "Lock" = "Lock"
        "Unlock" = "Unlock"
        "Merge" = "Merge"
        "Copy" = "Copy"
        "Clone" = "Copy"
        "Execute" = "Invoke"
        "Invoke" = "Invoke"
        "Install" = "Install"
        "Uninstall" = "Uninstall"
        "Grant" = "Grant"
        "Revoke" = "Revoke"
        "Modify" = "Set"
        "Convert" = "Convert"
        "Transform" = "Convert"
        "Validate" = "Test"
        "Calculate" = "Measure"
        "Apply" = "Set"
        "Cancel" = "Stop"
        "Close" = "Close"
        "Book" = "Register"
        "Reschedule" = "Update"
        "Route" = "Set"
        "Win" = "Complete"
        "Lose" = "Stop"
        "Qualify" = "Approve"
        "Fulfill" = "Complete"
        "Renew" = "Update"
        "Revise" = "Update"
        "Stage" = "Set"
        "Sync" = "Sync"
        "Expand" = "Expand"
        "Search" = "Find"
        "Query" = "Find"
        "Format" = "Format"
        "Parse" = "ConvertFrom"
        "Download" = "Receive"
        "Upload" = "Send"
        "Commit" = "Confirm"
        "Initialize" = "Initialize"
        "Provision" = "Enable"
        "Deprovision" = "Disable"
        "Deliver" = "Send"
        "Distribute" = "Send"
        "Process" = "Invoke"
        "Trigger" = "Start"
        "Pick" = "Select"
        "Release" = "Unlock"
        "Reassign" = "Move"
        "Replace" = "Update"
        "Reset" = "Reset"
        "Revert" = "Undo"
        "Rollup" = "Measure"
        "Unblock" = "Enable"
        "Check" = "Test"
        "Find" = "Find"
        "Auto" = "Auto"
        "Bulk" = "Bulk"
        "Compound" = "Compound"
        "Generate" = "New"
        "Make" = "Set"
        "Order" = "Sort"
        "Override" = "Set"
        "Propagate" = "Copy"
        "Full" = "Search"
    }
    
    foreach ($key in $verbMap.Keys) {
        if ($Name -match "^$key") {
            return $verbMap[$key]
        }
    }
    
    return "Invoke"
}

function Generate-Cmdlet {
    param(
        [System.Type]$RequestType
    )
    
    $requestName = $RequestType.Name
    $responseName = $requestName -replace "Request$", "Response"
    $name = $requestName -replace "Request$", ""
    
    # Determine verb and noun
    $verb = Get-PowerShellVerb $name
    $noun = $name -replace "^(Add|Remove|Get|Retrieve|Set|Update|New|Create|Delete|Send|Publish|Unpublish|Import|Export|Lock|Unlock|Merge|Copy|Clone|Execute|Invoke|Install|Uninstall|Grant|Revoke|Modify|Convert|Transform|Validate|Calculate|Apply|Cancel|Close|Book|Reschedule|Route|Win|Lose|Qualify|Fulfill|Renew|Revise|Stage|Sync|Expand|Search|Query|Format|Parse|Download|Upload|Commit|Initialize|Provision|Deprovision|Deliver|Distribute|Process|Trigger|Pick|Release|Reassign|Replace|Reset|Revert|Rollup|Unblock|Check|Find|Auto|Bulk|Compound|Generate|Make|Order|Override|Propagate|Full)", ""
    
    if ([string]::IsNullOrEmpty($noun)) {
        $noun = $name
    }
    
    $noun = "Dataverse$noun"
    $className = "${verb}${noun}Cmdlet"
    
    # Get properties
    $props = $RequestType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | 
        Where-Object { $_.CanWrite -and $_.DeclaringType -eq $RequestType }
    
    # Determine if needs ShouldProcess
    $needsShouldProcess = $verb -in @("Set", "Remove", "Update", "New", "Add", "Delete", "Send", "Publish", "Import", "Lock", "Unlock", "Merge", "Copy", "Install", "Uninstall", "Grant", "Revoke", "Modify", "Convert", "Apply", "Cancel", "Close")
    
    $shouldProcessAttr = if ($needsShouldProcess) { ", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium" } else { "" }
    
    # Generate code
    $code = @"
using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.$verb, "$noun"$shouldProcessAttr)]
    [OutputType(typeof($responseName))]
    ///<summary>Executes $requestName SDK message.</summary>
    public class $className : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

"@
    
    # Add parameters
    foreach ($prop in $props) {
        $propName = $prop.Name
        $propType = $prop.PropertyType
        
        if ($propType.FullName -like "Microsoft.Xrm.Sdk.EntityReference*") {
            $code += @"
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to a Dataverse record. Can be: EntityReference object, PSObject with Id/TableName properties (e.g., from Get-DataverseRecord), or Guid value (requires corresponding TableName parameter). Conversion handled by DataverseTypeConverter.")]
        public object $propName { get; set; }

"@
        }
        elseif ($propType.FullName -like "Microsoft.Xrm.Sdk.OptionSetValue*") {
            $code += @"
        [Parameter(Mandatory = false, HelpMessage = "OptionSet (picklist) value. Can be: numeric value (option set integer code) or string label (display name of the option). Conversion handled by DataverseTypeConverter.")]
        public object $propName { get; set; }

"@
        }
        elseif ($propType.FullName -like "Microsoft.Xrm.Sdk.Entity*" -and $propType.Name -eq "Entity") {
            $code += @"
        [Parameter(Mandatory = false, HelpMessage = "PSObject representing a Dataverse Entity record. Properties should match the logical names of columns in the target table. For lookup fields, accepts Guid, EntityReference, or PSObject with Id/TableName. For choice fields (picklists), accepts numeric value or string label. Conversion handled by DataverseEntityConverter.")]
        public PSObject $propName { get; set; }

"@
        }
        else {
            $simpleType = $propType.FullName -replace "^System\.", "" -replace "`1.*$", ""
            
            # Add more specific help based on property name patterns
            $helpMessage = if ($propName -match "TableName|EntityName|EntityLogicalName") {
                "Logical name of the Dataverse table (entity). Required when providing Guid values for record references instead of EntityReference or PSObject."
            } else {
                "$propName parameter for the $requestName operation."
            }
            
            $code += @"
        [Parameter(Mandatory = false, HelpMessage = "$helpMessage")]
        public $simpleType $propName { get; set; }

"@
        }
    }
    
    $code += @"

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new $requestName();

"@
    
    # Add property assignments
    foreach ($prop in $props) {
        $propName = $prop.Name
        $propType = $prop.PropertyType
        
        if ($propType.FullName -like "Microsoft.Xrm.Sdk.EntityReference*") {
            $code += @"
            if ($propName != null)
            {
                request.$propName = DataverseTypeConverter.ToEntityReference($propName, null, "$propName");
            }

"@
        }
        elseif ($propType.FullName -like "Microsoft.Xrm.Sdk.OptionSetValue*") {
            $code += @"
            if ($propName != null)
            {
                request.$propName = DataverseTypeConverter.ToOptionSetValue($propName, "$propName");
            }

"@
        }
        elseif ($propType.FullName -like "Microsoft.Xrm.Sdk.Entity*" -and $propType.Name -eq "Entity") {
            $code += @"
            if ($propName != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in $propName.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.$propName = entity;
            }

"@
        }
        else {
            $code += @"
            request.$propName = $propName;
"@
        }
    }
    
    if ($needsShouldProcess) {
        $code += @"

            if (ShouldProcess("Executing $requestName", "$requestName"))
            {
                var response = ($responseName)Connection.Execute(request);
                WriteObject(response);
            }
"@
    }
    else {
        $code += @"

            var response = ($responseName)Connection.Execute(request);
            WriteObject(response);
"@
    }
    
    $code += @"

        }
    }
}
"@
    
    return @{
        ClassName = $className
        FileName = "${className}.cs"
        Code = $code
    }
}

# Main execution
$outputDir = "./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands"

if ($RequestName) {
    # Generate single cmdlet
    $type = $assembly.GetTypes() | Where-Object { $_.Name -eq $RequestName }
    if (-not $type) {
        Write-Error "Request type '$RequestName' not found"
        exit 1
    }
    
    $result = Generate-Cmdlet $type
    $filePath = Join-Path $outputDir $result.FileName
    
    Set-Content -Path $filePath -Value $result.Code -Encoding UTF8
    Write-Host "Generated: $($result.FileName)"
}
elseif ($GenerateAll) {
    # Generate all remaining cmdlets
    $requestTypes = $assembly.GetTypes() | Where-Object { 
        $_.Name -like "*Request" -and 
        $_.IsPublic -and 
        -not $_.IsAbstract -and
        $_.Namespace -eq "Microsoft.Crm.Sdk.Messages" -and
        $implemented -notcontains $_.Name
    } | Sort-Object Name
    
    Write-Host "Generating cmdlets for $($requestTypes.Count) remaining request types..."
    $generated = 0
    
    foreach ($type in $requestTypes) {
        $result = Generate-Cmdlet $type
        $filePath = Join-Path $outputDir $result.FileName
        
        # Skip if already exists
        if (Test-Path $filePath) {
            Write-Host "Skipping $($result.FileName) (already exists)"
            continue
        }
        
        Set-Content -Path $filePath -Value $result.Code -Encoding UTF8
        Write-Host "Generated: $($result.FileName)"
        $generated++
        
        # Limit batch size
        if ($generated -ge $BatchSize) {
            Write-Host "`nGenerated $generated cmdlet files (batch limit reached)"
            break
        }
    }
    
    Write-Host "`nTotal generated: $generated cmdlet files"
}
else {
    Write-Host "Usage:"
    Write-Host "  Generate single: ./tools/Generate-Cmdlets.ps1 -RequestName 'SendEmailRequest'"
    Write-Host "  Generate all: ./tools/Generate-Cmdlets.ps1 -GenerateAll"
    Write-Host "  Generate batch: ./tools/Generate-Cmdlets.ps1 -GenerateAll -BatchSize 100"
}
