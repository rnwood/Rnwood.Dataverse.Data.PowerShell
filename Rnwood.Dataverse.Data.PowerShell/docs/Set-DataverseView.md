---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseView

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### Simple
```
Set-DataverseView [-Id <Guid>] [-Name <String>] [-TableName <String>] [-SystemView] [-Description <String>]
 [-Columns <Object[]>] [-AddColumns <Object[]>] [-RemoveColumns <String[]>] [-UpdateColumns <Hashtable[]>]
 [-FilterValues <Hashtable[]>] [-LayoutXml <String>] [-IsDefault] [-QueryType <Int32>] [-NoUpdate] [-NoCreate]
 [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### FetchXml
```
Set-DataverseView [-Id <Guid>] [-Name <String>] [-TableName <String>] [-SystemView] [-Description <String>]
 [-AddColumns <Object[]>] [-RemoveColumns <String[]>] [-UpdateColumns <Hashtable[]>] [-FetchXml <String>]
 [-LayoutXml <String>] [-IsDefault] [-QueryType <Int32>] [-NoUpdate] [-NoCreate] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -AddColumns
Columns to add to the view.
Can be an array of column names or hashtables with column configuration (name, width, etc.)

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
Columns to include in the view.
Can be an array of column names or hashtables with column configuration (name, width, etc.)

```yaml
Type: Object[]
Parameter Sets: Simple
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
Description of the view

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

### -FetchXml
FetchXml query to use for the view

```yaml
Type: String
Parameter Sets: FetchXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterValues
One or more hashtables to filter records.
Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.
Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name).
Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'.
Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.

```yaml
Type: Hashtable[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the view to update.
If not specified or if the view doesn't exist, a new view is created.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsDefault
Set this view as the default view for the table

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

### -LayoutXml
Layout XML for the view.
If not specified when creating, a default layout will be generated from Columns

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

### -Name
Name of the view.
Required when creating a new view.

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

### -NoCreate
If specified, then no view will be created even if no existing view matching the ID is found

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

### -NoUpdate
If specified, existing views matching the ID will not be updated

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

### -PassThru
If specified, returns the ID of the created or updated view

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

### -QueryType
View type: 0=OtherView, 1=PublicView, 2=AdvancedFind, 4=SubGrid, 8=Dashboard, 16=MobileClientView, 64=LookupView, 128=MainApplicationView, 256=QuickFindSearch, 512=Associated, 1024=CalendarView, 2048=InteractiveExperience.
Default is 1 (PublicView)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoveColumns
Columns to remove from the view

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SystemView
Work with a system view (savedquery) instead of a personal view (userquery)

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

### -TableName
Logical name of the table this view is for.
Required when creating a new view.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UpdateColumns
Columns to update in the view.
Hashtables with column configuration (name, width, etc.)

```yaml
Type: Hashtable[]
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
