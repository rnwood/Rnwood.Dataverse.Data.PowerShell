---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseOrganizationSettings

## SYNOPSIS
Gets organization settings from the single organization record in a Dataverse environment.

## SYNTAX

```
Get-DataverseOrganizationSettings [-IncludeRawXml] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseOrganizationSettings cmdlet retrieves organization settings from the single organization record that exists in every Dataverse environment. The record is automatically discovered and all organization table columns are returned as PSObject properties.

The OrgDbOrgSettings XML column is parsed into a structured OrgDbOrgSettingsParsed property for easy access to individual settings. By default, the raw XML is removed from the output to keep it clean, but can be included with the -IncludeRawXml parameter.

## EXAMPLES

### Example 1: Get organization settings
```powershell
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $orgSettings = Get-DataverseOrganizationSettings -Connection $connection
PS C:\> $orgSettings.name
Contoso Corporation
```

Gets the organization settings and displays the organization name.

### Example 2: Access parsed OrgDbOrgSettings
```powershell
PS C:\> $orgSettings = Get-DataverseOrganizationSettings -Connection $connection
PS C:\> $orgSettings.OrgDbOrgSettingsParsed.MaxUploadFileSize
5242880
```

Gets the organization settings and accesses a specific setting from the parsed OrgDbOrgSettings XML.

### Example 3: Include raw XML
```powershell
PS C:\> $orgSettings = Get-DataverseOrganizationSettings -Connection $connection -IncludeRawXml
PS C:\> $orgSettings.orgdborgsettings
<OrgSettings>
  <MaxUploadFileSize>5242880</MaxUploadFileSize>
  ...
</OrgSettings>
```

Gets the organization settings and includes the raw OrgDbOrgSettings XML in the output.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeRawXml
If specified, includes the raw OrgDbOrgSettings XML string in the output

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

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
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Set-DataverseOrganizationSettings](Set-DataverseOrganizationSettings.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
