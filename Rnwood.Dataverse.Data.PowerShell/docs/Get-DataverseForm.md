---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseForm

## SYNOPSIS
Retrieves forms from a Dataverse environment.

## SYNTAX

### ById
```
Get-DataverseForm -Id <Guid> [-IncludeFormXml] [-ParseFormXml] [-Unpublished] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByEntity
```
Get-DataverseForm -Entity <String> [-FormType <FormType>] [-UniqueNameFilter <String>] [-IncludeFormXml]
 [-ParseFormXml] [-Unpublished] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ByName
```
Get-DataverseForm -Entity <String> -Name <String> [-IncludeFormXml] [-ParseFormXml] [-Unpublished]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByUniqueName
```
Get-DataverseForm -Entity <String> -UniqueName <String> [-IncludeFormXml] [-ParseFormXml] [-Unpublished]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseForm cmdlet retrieves form definitions from a Dataverse environment. Forms can be retrieved by ID, by entity name (optionally filtered by form type), or by entity name and form name. The cmdlet returns simplified PSObject output with form properties, and can optionally include the raw FormXml or parse it into structured tab/section/control information.

## EXAMPLES

### Example 1: Get all forms for an entity
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseForm -Connection $conn -Entity 'contact'
```

Retrieves all forms for the contact entity.

### Example 2: Get a specific form by ID
```powershell
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Get-DataverseForm -Connection $conn -Id $formId
```

Retrieves a specific form by its ID.

### Example 3: Get forms of a specific type
```powershell
PS C:\> Get-DataverseForm -Connection $conn -Entity 'account' -FormType 'Main'
```

Retrieves all main forms for the account entity.

### Example 4: Get a form by entity and name
```powershell
PS C:\> Get-DataverseForm -Connection $conn -Entity 'contact' -Name 'Information'
```

Retrieves the contact form named "Information".

### Example 5: Get form with FormXml
```powershell
PS C:\> $form = Get-DataverseForm -Connection $conn -Entity 'contact' -Name 'Information' -IncludeFormXml
PS C:\> $form.FormXml
```

Retrieves a form and includes the raw FormXml content.

### Example 6: Get form with parsed structure
```powershell
PS C:\> $form = Get-DataverseForm -Connection $conn -Entity 'contact' -Name 'Information' -ParseFormXml
PS C:\> $form.ParsedForm.Tabs | ForEach-Object { $_.Name }
```

Retrieves a form and parses the FormXml into a structured object with tabs, sections, and controls.

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

### -Entity
Logical name of the entity/table for which to retrieve forms

```yaml
Type: String
Parameter Sets: ByEntity, ByName, ByUniqueName
Aliases: EntityName, TableName

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormType
Form type filter: Main (2), QuickCreate (5), QuickView (6), Card (11), Dashboard (0), MainInteractionCentric (63), Other (100), MainBackup (101), AppointmentBook (102), Dialog (103)

```yaml
Type: FormType
Parameter Sets: ByEntity
Aliases:
Accepted values: Main, QuickCreate, QuickView, Card, Dashboard, MainInteractionCentric, Other, MainBackup, AppointmentBook, Dialog

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the form to retrieve

```yaml
Type: Guid
Parameter Sets: ById
Aliases: formid

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IncludeFormXml
Include the FormXml in the output (default: false for performance)

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

### -Name
Name of the form to retrieve

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParseFormXml
Parse FormXml and include structured tab/section/control information

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

### -Unpublished
Include unpublished forms in the results

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

### -UniqueName
Unique name of the form to retrieve

```yaml
Type: String
Parameter Sets: ByUniqueName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UniqueNameFilter
Unique name filter for forms

```yaml
Type: String
Parameter Sets: ByEntity
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

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
