---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSearchByKeywordsKbArticle

## SYNOPSIS
Contains the data that is needed to search for knowledge base articles that contain the specified keywords.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleRequest)

## SYNTAX

```
Invoke-DataverseSearchByKeywordsKbArticle -Connection <ServiceClient> -SearchText <String> -SubjectId <Guid> -UseInflection <Boolean> -QueryExpression <QueryBase>
```

## DESCRIPTION
Contains the data that is needed to search for knowledge base articles that contain the specified keywords.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSearchByKeywordsKbArticle -Connection <ServiceClient> -SearchText <String> -SubjectId <Guid> -UseInflection <Boolean> -QueryExpression <QueryBase>
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
Gets or sets the keywords in the article. Required.

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
Gets or sets a value that indicates whether to use inflectional stem matching when searching for knowledge base articles with the specified keywords. Required.

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
Gets or sets the query criteria to find knowledge base articles with specified keywords. Required.

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

### Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleResponse)
## NOTES

## RELATED LINKS
