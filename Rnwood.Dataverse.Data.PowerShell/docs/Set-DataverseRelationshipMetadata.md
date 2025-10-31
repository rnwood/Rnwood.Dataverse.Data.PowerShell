---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRelationshipMetadata

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Set-DataverseRelationshipMetadata [-SchemaName] <String> -RelationshipType <String> -ReferencedEntity <String>
 -ReferencingEntity <String> [-LookupAttributeSchemaName <String>] [-LookupAttributeDisplayName <String>]
 [-LookupAttributeDescription <String>] [-LookupAttributeRequiredLevel <String>]
 [-IntersectEntityName <String>] [-CascadeAssign <String>] [-CascadeShare <String>] [-CascadeUnshare <String>]
 [-CascadeReparent <String>] [-CascadeDelete <String>] [-CascadeMerge <String>] [-IsHierarchical]
 [-IsSearchable] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
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

### -CascadeAssign
Cascade behavior for Assign: NoCascade, Cascade, Active, UserOwned, RemoveLink

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeDelete
Cascade behavior for Delete: NoCascade, RemoveLink, Restrict, Cascade

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, RemoveLink, Restrict, Cascade

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeMerge
Cascade behavior for Merge: NoCascade, Cascade

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeReparent
Cascade behavior for Reparent: NoCascade, Cascade, Active, UserOwned, RemoveLink

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeShare
Cascade behavior for Share: NoCascade, Cascade, Active, UserOwned

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeUnshare
Cascade behavior for Unshare: NoCascade, Cascade, Active, UserOwned

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

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

### -IntersectEntityName
Schema name of the intersect entity for ManyToMany relationships (e.g., 'new_project_contact').
If not specified, generated automatically.

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

### -IsHierarchical
Whether the relationship is searchable

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

### -IsSearchable
Whether the lookup attribute is searchable (OneToMany only)

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

### -LookupAttributeDescription
Description of the lookup attribute (OneToMany only)

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

### -LookupAttributeDisplayName
Display name of the lookup attribute (OneToMany only)

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

### -LookupAttributeRequiredLevel
Required level of the lookup attribute (OneToMany only): None, SystemRequired, ApplicationRequired, Recommended

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: None, SystemRequired, ApplicationRequired, Recommended

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LookupAttributeSchemaName
Schema name of the lookup attribute to create on the referencing entity (OneToMany only, e.g., 'new_ProjectId')

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

### -PassThru
Return the created or updated relationship metadata

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

### -ReferencedEntity
Primary/referenced entity name (OneToMany) or first entity name (ManyToMany)

```yaml
Type: String
Parameter Sets: (All)
Aliases: PrimaryEntity, Entity1

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReferencingEntity
Related/referencing entity name (OneToMany) or second entity name (ManyToMany)

```yaml
Type: String
Parameter Sets: (All)
Aliases: RelatedEntity, Entity2

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RelationshipType
Type of relationship: OneToMany or ManyToMany

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: OneToMany, ManyToMany

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SchemaName
Schema name of the relationship (e.g., 'new_project_contact')

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### None
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.RelationshipMetadataBase
## NOTES

## RELATED LINKS
