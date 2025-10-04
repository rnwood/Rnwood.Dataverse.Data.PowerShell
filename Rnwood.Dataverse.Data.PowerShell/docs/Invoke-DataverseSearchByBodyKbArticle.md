---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSearchByBodyKbArticle

## SYNOPSIS
Contains the data that is needed to search for knowledge base articles that contain the specified body text.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByBodyKbArticleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SearchByBodyKbArticleRequest)

## SYNTAX

```
Invoke-DataverseSearchByBodyKbArticle -Connection <ServiceClient> [-SearchText <String>] [-SubjectId <Guid>]
 [-UseInflection <Boolean>] [-QueryExpression <QueryBase>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to search for knowledge base articles that contain the specified body text.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSearchByBodyKbArticle -Connection <ServiceClient> -SearchText <String> -SubjectId <Guid> -UseInflection <Boolean> -QueryExpression <QueryBase>
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

### -QueryExpression
Gets or sets the query criteria to find knowledge base articles with specified body text. Required.

```yaml
Type: QueryBase
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SearchText
Gets or sets the text contained in the body of the article. Required.

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

### -SubjectId
Gets or sets the ID of the knowledge base article subject. Required.

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

### -UseInflection
Gets or sets a value that indicates whether to use inflectional stem matching when searching for knowledge base articles with a specified body text. Required.

```yaml
Type: Boolean
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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
