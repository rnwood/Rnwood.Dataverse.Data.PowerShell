---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSearchByTitleKbArticle

## SYNOPSIS
Contains the data that is needed to search for knowledge base articles that contain the specified title.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleRequest)

## SYNTAX

```
Invoke-DataverseSearchByTitleKbArticle -Connection <ServiceClient> -SearchText <String> -SubjectId <Guid> -UseInflection <Boolean> -QueryExpression <QueryBase>
```

## DESCRIPTION
Contains the data that is needed to search for knowledge base articles that contain the specified title.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSearchByTitleKbArticle -Connection <ServiceClient> -SearchText <String> -SubjectId <Guid> -UseInflection <Boolean> -QueryExpression <QueryBase>
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

### -SearchText
Gets or sets the title in the articles. Required.

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
Gets or sets the ID of the subject. Required.

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
Gets or sets a value that indicates whether to use inflectional stem matching when searching for knowledge articles. Required.

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

### -QueryExpression
Gets or sets the query criteria to find knowledge articles with specified text. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleResponse)
## NOTES

## RELATED LINKS
