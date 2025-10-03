---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseWorkflow

## SYNOPSIS
Executes a workflow against a specific record.

## SYNTAX

```
Invoke-DataverseWorkflow -Connection <ServiceClient> -EntityId <Guid> -WorkflowId <Guid> 
 [-InputArguments <Hashtable>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet executes a Dataverse workflow (process) against a specific record using the ExecuteWorkflowRequest message.

Workflows in Dataverse can automate business processes. This cmdlet allows you to:
- Trigger on-demand workflows programmatically
- Pass input parameters to workflows that accept them
- Execute workflows as part of larger automation scripts

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseWorkflow -Connection $c -EntityId $accountId -WorkflowId $workflowId
```

Executes a workflow against a specific account record.

### Example 2
```powershell
PS C:\> Invoke-DataverseWorkflow -Connection $c -EntityId $recordId -WorkflowId $workflowId -InputArguments @{ "Param1" = "Value1"; "Param2" = 123 }
```

Executes a workflow with input arguments.

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
ID of the record to execute the workflow against

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -WorkflowId
ID of the workflow to execute

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputArguments
Optional input arguments for the workflow as a hashtable

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
See standard PS docs.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Guid

Can accept EntityId from the pipeline.

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse

Returns an ExecuteWorkflowResponse object containing the ID of the asynchronous operation job that was created.

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.executeworkflowrequest?view=dataverse-sdk-latest

## RELATED LINKS
