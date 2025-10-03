# Invoke-DataverseIncrementKnowledgeArticleViewCount

## SYNOPSIS
Executes IncrementKnowledgeArticleViewCountRequest SDK message.

## SYNTAX

```
Invoke-DataverseIncrementKnowledgeArticleViewCount -Connection <ServiceClient> [-Source <object>] [-ViewDate <DateTime>] [-Location <Int32>] [-Count <Int32>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `IncrementKnowledgeArticleViewCountRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes IncrementKnowledgeArticleViewCountRequest SDK message.

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
### -Source
Parameter for the IncrementKnowledgeArticleViewCountRequest operation.

```yaml
Type: object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```
### -ViewDate
Parameter for the IncrementKnowledgeArticleViewCountRequest operation.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -Location
Parameter for the IncrementKnowledgeArticleViewCountRequest operation.

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
### -Count
Parameter for the IncrementKnowledgeArticleViewCountRequest operation.

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

### IncrementKnowledgeArticleViewCountResponse

Returns the response from the `IncrementKnowledgeArticleViewCountRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
