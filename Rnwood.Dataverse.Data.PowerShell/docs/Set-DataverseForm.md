---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseForm

## SYNOPSIS
Creates or updates a form in a Dataverse environment.

## SYNTAX

### Update
```
Set-DataverseForm -Id <Guid> [-Entity <String>] [-Name <String>] [-FormType <String>] [-Description <String>]
 [-IsActive] [-IsDefault] [-FormPresentation <String>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### UpdateWithXml
```
Set-DataverseForm -Id <Guid> [-Entity <String>] [-Name <String>] [-FormType <String>] -FormXmlContent <String>
 [-Description <String>] [-IsActive] [-IsDefault] [-FormPresentation <String>] [-PassThru] [-Publish]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Create
```
Set-DataverseForm -Entity <String> -Name <String> -FormType <String> [-Description <String>] [-IsActive]
 [-IsDefault] [-FormPresentation <String>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### CreateWithXml
```
Set-DataverseForm -Entity <String> -Name <String> -FormType <String> -FormXmlContent <String>
 [-Description <String>] [-IsActive] [-IsDefault] [-FormPresentation <String>] [-PassThru] [-Publish]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseForm cmdlet creates or updates form definitions in a Dataverse environment. When creating a form, you must specify the entity, name, and form type. When updating, you specify the form ID. The cmdlet supports both simple property-based updates and complete FormXml replacement. Forms can be optionally published after creation/update.

## EXAMPLES

### Example 1: Create a new main form
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $formId = Set-DataverseForm -Connection $conn -Entity 'contact' -Name 'Custom Contact Form' -FormType 'Main' -PassThru
PS C:\> Write-Host "Created form with ID: $formId"
```

Creates a new main form for the contact entity with minimal configuration.

### Example 2: Create a form with description
```powershell
PS C:\> Set-DataverseForm -Connection $conn -Entity 'account' -Name 'Account Quick Create' -FormType 'QuickCreate' -Description 'Quick create form for accounts' -IsActive
```

Creates a new quick create form with a description and marks it as active.

### Example 3: Update an existing form
```powershell
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Set-DataverseForm -Connection $conn -Id $formId -Name 'Updated Form Name' -Description 'Updated description'
```

Updates the name and description of an existing form.

### Example 4: Create a form with custom FormXml
```powershell
PS C:\> $formXml = Get-Content -Path 'CustomForm.xml' -Raw
PS C:\> Set-DataverseForm -Connection $conn -Entity 'contact' -Name 'Advanced Form' -FormType 'Main' -FormXmlContent $formXml -Publish
```

Creates a new form using custom FormXml content and publishes it immediately.

### Example 5: Update form and publish
```powershell
PS C:\> Set-DataverseForm -Connection $conn -Id $formId -IsDefault -Publish
```

Sets a form as the default form for its entity and publishes the changes.

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -Description
Description of the form

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Entity
Logical name of the entity/table for the form

```yaml
Type: String
Parameter Sets: Update, UpdateWithXml
Aliases: EntityName, TableName, ObjectTypeCode

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Create, CreateWithXml
Aliases: EntityName, TableName, ObjectTypeCode

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormPresentation
Form presentation: ClassicForm (0), AirForm (1), ConvertedICForm (2)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: ClassicForm, AirForm, ConvertedICForm

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormType
Form type: Main, QuickCreate, QuickView, Card, Dashboard

```yaml
Type: String
Parameter Sets: Update, UpdateWithXml
Aliases:
Accepted values: Main, QuickCreate, QuickView, Card, Dashboard

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Create, CreateWithXml
Aliases:
Accepted values: Main, QuickCreate, QuickView, Card, Dashboard

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormXmlContent
Complete FormXml content

```yaml
Type: String
Parameter Sets: UpdateWithXml, CreateWithXml
Aliases: FormXml, Xml

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the form to update

```yaml
Type: Guid
Parameter Sets: Update, UpdateWithXml
Aliases: formid

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsActive
Whether the form is active (default: true)

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

### -IsDefault
Whether this form is the default form for the entity

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
Name of the form

```yaml
Type: String
Parameter Sets: Update, UpdateWithXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Create, CreateWithXml
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the form ID after creation/update

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

### -Publish
Publish the form after creation/update

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

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

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

### System.Guid
## OUTPUTS

### System.Guid
## NOTES

## RELATED LINKS
