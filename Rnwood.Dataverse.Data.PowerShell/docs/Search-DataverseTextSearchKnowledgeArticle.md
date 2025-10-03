# Search-DataverseTextSearchKnowledgeArticle

## SYNOPSIS
Executes FullTextSearchKnowledgeArticleRequest SDK message.

## SYNTAX

```
Search-DataverseTextSearchKnowledgeArticle -Connection <ServiceClient> [-SearchText <String>] [-UseInflection <Boolean>] [-RemoveDuplicates <Boolean>] [-StateCode <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `FullTextSearchKnowledgeArticleRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes FullTextSearchKnowledgeArticleRequest SDK message.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

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
Parameter for the FullTextSearchKnowledgeArticleRequest operation.

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
### -UseInflection
Parameter for the FullTextSearchKnowledgeArticleRequest operation.

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
### -RemoveDuplicates
Parameter for the FullTextSearchKnowledgeArticleRequest operation.

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
### -StateCode
Parameter for the FullTextSearchKnowledgeArticleRequest operation.

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### FullTextSearchKnowledgeArticleResponse

Returns the response from the `FullTextSearchKnowledgeArticleRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
