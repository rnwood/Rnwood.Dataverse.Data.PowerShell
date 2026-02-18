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
Set-DataverseForm -Id <Guid> [-Entity <String>] [-Name <String>] [-FormType <FormType>] [-Description <String>]
 [-IsActive] [-IsDefault] [-FormPresentation <FormPresentation>] [-PassThru] [-Publish]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### UpdateWithXml
```
Set-DataverseForm -Id <Guid> [-Entity <String>] [-Name <String>] [-FormType <FormType>]
 -FormXmlContent <String> [-Description <String>] [-IsActive] [-IsDefault]
 [-FormPresentation <FormPresentation>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Create
```
Set-DataverseForm -Entity <String> -Name <String> -FormType <FormType> [-Description <String>] [-IsActive]
 [-IsDefault] [-FormPresentation <FormPresentation>] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### CreateWithXml
```
Set-DataverseForm -Entity <String> -Name <String> -FormType <FormType> -FormXmlContent <String>
 [-Description <String>] [-IsActive] [-IsDefault] [-FormPresentation <FormPresentation>] [-PassThru] [-Publish]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseForm cmdlet creates or updates form definitions in a Dataverse environment. When creating a form, you must specify the entity, name, and form type. When updating, you specify the form ID. The cmdlet supports both simple property-based updates and complete FormXml replacement. Forms can be optionally published after creation/update.

Use this cmdlet for form creation and basic property updates. For detailed form structure manipulation (tabs, sections, controls), use the specialized form management cmdlets.

## EXAMPLES

### Example 1: Create a new main form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = Set-DataverseForm -Entity 'contact' -Name 'Custom Contact Form' -FormType 'Main' -PassThru
PS C:\> Write-Host "Created form with ID: $formId"
```

Creates a new main form for the contact entity with minimal configuration.

### Example 2: Create a form with description and set as active
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseForm -Entity 'account' -Name 'Account Quick Create' -FormType 'QuickCreate' -Description 'Quick create form for accounts' -IsActive -PassThru
```

Creates a new quick create form with a description and marks it as active.

### Example 3: Update an existing form properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Set-DataverseForm -Id $formId -Name 'Updated Form Name' -Description 'Updated description' -IsDefault
```

Updates the name, description, and sets the form as default for its entity.

### Example 4: Create a form with custom FormXml
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formXml = Get-Content -Path 'CustomForm.xml' -Raw
PS C:\> Set-DataverseForm -Entity 'contact' -Name 'Advanced Form' -FormType 'Main' -FormXmlContent $formXml -Publish
```

Creates a new form using custom FormXml content and publishes it immediately.

### Example 5: Update form with new FormXml and publish
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $newFormXml = Get-Content -Path 'UpdatedForm.xml' -Raw
PS C:\> Set-DataverseForm -Id $formId -FormXmlContent $newFormXml -Publish
```

Updates an existing form with new FormXml content and publishes the changes.

### Example 6: Create form and then customize with specialized cmdlets
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create basic form
PS C:\> $formId = Set-DataverseForm -Entity 'contact' -Name 'My Custom Form' -FormType 'Main' -PassThru

PS C:\> # Add a tab
PS C:\> $tabId = Set-DataverseFormTab -FormId $formId -Name 'CustomTab' -Label 'Custom Information' -PassThru

PS C:\> # Add a section to the tab
PS C:\> $sectionId = Set-DataverseFormSection -FormId $formId -TabName 'CustomTab' -Name 'CustomSection' -Label 'Additional Details' -PassThru

PS C:\> # Add controls to the section
PS C:\> Set-DataverseFormControl -FormId $formId -TabName 'CustomTab' -SectionName 'CustomSection' -DataField 'description' -Label 'Notes'

PS C:\> # Publish the form
PS C:\> Set-DataverseForm -Id $formId -Publish
```

Demonstrates creating a form and then customizing it with specialized form cmdlets.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
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
Form presentation type

```yaml
Type: FormPresentation
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
Form type

```yaml
Type: FormType
Parameter Sets: Update, UpdateWithXml
Aliases:
Accepted values: Dashboard, AppointmentBook, Main, MiniCampaignBO, Preview, MobileExpress, QuickViewForm, QuickCreate, Dialog, TaskFlowForm, InteractionCentricDashboard, Card, MainInteractiveExperience, ContextualDashboard, Other, MainBackup, AppointmentBookBackup, PowerBIDashboard

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: FormType
Parameter Sets: Create, CreateWithXml
Aliases:
Accepted values: Dashboard, AppointmentBook, Main, MiniCampaignBO, Preview, MobileExpress, QuickViewForm, QuickCreate, Dialog, TaskFlowForm, InteractionCentricDashboard, Card, MainInteractiveExperience, ContextualDashboard, Other, MainBackup, AppointmentBookBackup, PowerBIDashboard

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Guid
## NOTES

## RELATED LINKS
