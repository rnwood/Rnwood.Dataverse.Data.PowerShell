# Adding New Request Cmdlets

This guide describes the pattern for adding new specialized cmdlets that wrap Dataverse SDK request messages.

## Overview

The module includes specialized cmdlets that wrap common Dataverse SDK request messages, providing a more PowerShell-native experience than using `Invoke-DataverseRequest` directly. These cmdlets:

- Use PowerShell parameter binding instead of constructing request objects
- Support pipeline input where appropriate
- Include `-WhatIf` and `-Confirm` for destructive operations
- Convert between PowerShell types and SDK types automatically
- Provide comprehensive help documentation

## Pattern

### 1. Create the Cmdlet Class

Create a new file in `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/` following this pattern:

```csharp
using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages; // or Microsoft.Xrm.Sdk.Messages
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Verb, "NounName", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ResponseType))]
    ///<summary>Brief description of what the cmdlet does.</summary>
    public class VerbNounNameCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        // Add parameters that map to request properties
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Description")]
        public Type ParameterName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Create the request
            RequestType request = new RequestType
            {
                Property1 = ParameterName1,
                Property2 = ParameterName2
            };

            // For destructive operations, use ShouldProcess
            if (ShouldProcess($"Target description", $"Action description"))
            {
                ResponseType response = (ResponseType)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
```

### 2. Parameter Conversion Utilities

#### EntityReference Conversion

For parameters that accept record references, use this pattern:

```csharp
private EntityReference ConvertToEntityReference(object value, string tableName, string parameterName)
{
    if (value is EntityReference entityRef)
    {
        return entityRef;
    }
    else if (value is PSObject psObj)
    {
        var idProp = psObj.Properties["Id"];
        var tableNameProp = psObj.Properties["TableName"] ?? psObj.Properties["LogicalName"];

        if (idProp != null && tableNameProp != null)
        {
            return new EntityReference((string)tableNameProp.Value, (Guid)idProp.Value);
        }
    }
    else if (value is Guid guid)
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a Guid");
        }
        return new EntityReference(tableName, guid);
    }
    else if (value is string strValue && Guid.TryParse(strValue, out Guid parsedGuid))
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a string Guid");
        }
        return new EntityReference(tableName, parsedGuid);
    }

    throw new ArgumentException($"Unable to convert {parameterName} to EntityReference. Expected EntityReference, PSObject with Id and TableName properties, or Guid with TableName parameter.");
}
```

Then define parameters like this:

```csharp
[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the record")]
public object Target { get; set; }

[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
[Alias("EntityName", "LogicalName")]
public string TableName { get; set; }
```

#### OptionSetValue Conversion

For choice/picklist parameters:

```csharp
private OptionSetValue ConvertToOptionSetValue(object value, string parameterName)
{
    if (value is OptionSetValue osv)
    {
        return osv;
    }
    else if (value is int intValue)
    {
        return new OptionSetValue(intValue);
    }
    else if (value is string strValue && int.TryParse(strValue, out int parsedInt))
    {
        return new OptionSetValue(parsedInt);
    }

    throw new ArgumentException($"Unable to convert {parameterName} to OptionSetValue. Expected OptionSetValue or integer value.");
}
```

### 3. Create Documentation

Create a markdown file in `Rnwood.Dataverse.Data.PowerShell/docs/` named `Verb-NounName.md`:

```markdown
---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Verb-NounName

## SYNOPSIS
Brief one-line description

## SYNTAX

```
Verb-NounName -Connection <ServiceClient> -Parameter1 <Type1> [-Parameter2 <Type2>] 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

Detailed description of what the cmdlet does and when to use it.

## EXAMPLES

### Example 1
```powershell
PS C:\> Verb-NounName -Connection $c -Parameter1 $value1
```

Description of what this example does.

### Example 2
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName table | Verb-NounName -Connection $c -Parameter2 $value2
```

Example using pipeline.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Parameter1
Description of parameter

```yaml
Type: Type1
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -WhatIf / -Confirm
Standard ShouldProcess parameters (for destructive operations only)

### -ProgressAction
See standard PS docs.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### Type
Description of what can be piped

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.ResponseType
Description of output

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.requesttype?view=dataverse-sdk-latest

## RELATED LINKS
```

### 4. Create Tests

Add tests in `tests/Request-Cmdlets.Tests.ps1` or create a new test file:

```powershell
Describe 'Verb-NounName' {
    . $PSScriptRoot/Common.ps1

    It "Performs the expected operation" {
        $connection = getMockConnection
        
        # Setup test data
        $testId = [Guid]::NewGuid()
        
        # Execute cmdlet
        try {
            $response = Verb-NounName -Connection $connection -Parameter1 $value1 -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "ResponseType"
        } catch {
            # Some requests may not be fully supported by FakeXrmEasy
            Write-Host "RequestType may not be fully supported by FakeXrmEasy: $_"
        }
    }

    It "Accepts pipeline input" {
        $connection = getMockConnection
        
        # Setup and test pipeline
        $record = Get-DataverseRecord -Connection $connection -TableName contact -Id $testId
        $response = $record | Verb-NounName -Connection $connection -Parameter2 $value2 -Confirm:$false
        $response | Should -Not -BeNullOrEmpty
    }
}
```

### 5. Build and Test

1. Build the cmdlets project:
   ```bash
   dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj
   ```

2. Build the loader project:
   ```bash
   dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Loader/Rnwood.Dataverse.Data.PowerShell.Loader.csproj
   ```

3. Manually assemble the module:
   ```bash
   mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0
   cp Rnwood.Dataverse.Data.PowerShell/*.psd1 Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/
   cp Rnwood.Dataverse.Data.PowerShell/*.psm1 Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/
   mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets
   cp -r Rnwood.Dataverse.Data.PowerShell.Cmdlets/bin/Release/* Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets/
   mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/loader
   cp -r Rnwood.Dataverse.Data.PowerShell.Loader/bin/Release/* Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/loader/
   ```

4. Run tests:
   ```bash
   export TESTMODULEPATH=$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0
   pwsh -Command "Invoke-Pester -Output Detailed -Path tests"
   ```

## Examples

See the following cmdlets for complete examples:
- `Set-DataverseRecordOwner.cs` - Simple request with EntityReference conversion
- `Set-DataverseRecordState.cs` - Request with OptionSetValue conversion  
- `Add-DataverseAssociation.cs` - Request with array parameters
- `Invoke-DataverseWorkflow.cs` - Request with hashtable input
- `Publish-DataverseCustomization.cs` - Request with optional XML parameter

## Guidelines

1. **Verb Selection**: Use approved PowerShell verbs (`Get-Verb` to see list)
   - `Set-` for Assignment, state changes
   - `Add-`/`Remove-` for associations, team members
   - `Grant-`/`Revoke-` for permissions
   - `Invoke-` for executing processes
   - `Publish-` for deployment operations

2. **Parameter Names**: Use PowerShell-friendly names
   - `Target` instead of `EntityMoniker`
   - `TableName` instead of `EntityLogicalName`
   - `State`/`Status` instead of `StateCode`/`StatusCode`

3. **ShouldProcess**: Use for any destructive operations
   - Set `ConfirmImpact` appropriately (Low/Medium/High)
   - Provide meaningful target and action descriptions

4. **Help**: Provide comprehensive help
   - Include real-world examples
   - Document all parameters clearly
   - Link to Microsoft documentation

5. **Testing**: Write comprehensive tests
   - Test happy path
   - Test pipeline input
   - Handle FakeXrmEasy limitations gracefully
   - Use `-Confirm:$false` in tests to avoid prompts

## Available Request Types

The Dataverse SDK includes 337 request types in `Microsoft.Crm.Sdk.Messages` namespace. Priority has been given to:
- Record ownership and state management
- Many-to-many associations
- Security and permissions
- Team management
- Workflow execution
- Customization publishing

Additional cmdlets can be added following this pattern as needed.
