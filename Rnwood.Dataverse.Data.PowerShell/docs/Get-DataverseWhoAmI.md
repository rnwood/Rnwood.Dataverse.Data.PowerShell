---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseWhoAmI

## SYNOPSIS
Retrieves details about the current Dataverse user and organization specified by the connection provided.

## SYNTAX

```
Get-DataverseWhoAmI -Connection <ServiceClient> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet executes the Dataverse WhoAmI message and returns information about the authenticated user and organization.

The response includes:
- BusinessUnitId - The ID of the user's business unit
- UserId - The ID of the authenticated user
- OrganizationId - The ID of the organization

This is useful for verifying authentication and determining the context in which operations will be performed.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseWhoAmI -Connection $c
```

Returns info for the existing connection `$c`.

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

### -ProgressAction
See standard PS docs.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

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

This cmdlet does not accept pipeline input.

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.WhoAmIResponse

Returns a WhoAmIResponse object containing information about the authenticated user and organization:
- **UserId** (Guid): The unique identifier of the authenticated user
- **BusinessUnitId** (Guid): The unique identifier of the user's business unit
- **OrganizationId** (Guid): The unique identifier of the organization

This is useful for:
- Verifying authentication succeeded
- Determining the security context for operations
- Logging and auditing purposes

Access these properties directly on the response object, for example: `$whoami.UserId` after calling `$whoami = Get-DataverseWhoAmI -Connection $connection`

## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.whoamiresponse?view=dataverse-sdk-latest

## RELATED LINKS
