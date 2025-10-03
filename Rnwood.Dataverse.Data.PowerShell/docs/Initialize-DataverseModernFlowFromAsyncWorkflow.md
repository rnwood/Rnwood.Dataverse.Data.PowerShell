# Initialize-DataverseModernFlowFromAsyncWorkflow

## SYNOPSIS
Executes InitializeModernFlowFromAsyncWorkflowRequest SDK message.

## SYNTAX

```
Initialize-DataverseModernFlowFromAsyncWorkflow -Connection <ServiceClient> [-WorkflowId <Guid>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `InitializeModernFlowFromAsyncWorkflowRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes InitializeModernFlowFromAsyncWorkflowRequest SDK message.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

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
### -WorkflowId
Parameter for the InitializeModernFlowFromAsyncWorkflowRequest operation.

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### InitializeModernFlowFromAsyncWorkflowResponse

Returns the response from the `InitializeModernFlowFromAsyncWorkflowRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
