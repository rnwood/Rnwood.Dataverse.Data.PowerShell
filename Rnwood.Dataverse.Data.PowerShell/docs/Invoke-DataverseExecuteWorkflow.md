---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseExecuteWorkflow

## SYNOPSIS
Contains the data that’s needed to execute a workflow.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest)

## SYNTAX

```
Invoke-DataverseExecuteWorkflow -Connection <ServiceClient> -EntityId <Guid> -WorkflowId <Guid> -InputArguments <InputArgumentCollection>
```

## DESCRIPTION
Contains the data that’s needed to execute a workflow.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseExecuteWorkflow -Connection <ServiceClient> -EntityId <Guid> -WorkflowId <Guid> -InputArguments <InputArgumentCollection>
```

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

### -EntityId
Gets the of the newly created table.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WorkflowId
Gets or sets the ID of the workflow to execute. Required.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputArguments
For internal use only.

```yaml
Type: InputArgumentCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse)
## NOTES

## RELATED LINKS
