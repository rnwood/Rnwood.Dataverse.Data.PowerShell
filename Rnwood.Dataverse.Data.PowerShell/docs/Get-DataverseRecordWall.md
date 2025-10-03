# Get-DataverseRecordWall

## SYNOPSIS
Executes RetrieveRecordWallRequest SDK message.

## SYNTAX

```
Get-DataverseRecordWall -Connection <ServiceClient> [-Entity <object>] [-PageNumber <Int32>] [-PageSize <Int32>] [-CommentsPerPost <Int32>] [-StartDate <DateTime>] [-EndDate <DateTime>] [-Type <object>] [-Source <object>] [-SortDirection <Boolean>] [-Keyword <String>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet wraps the `RetrieveRecordWallRequest` SDK message. It executes the operation through the Dataverse Organization Service.

Executes RetrieveRecordWallRequest SDK message.

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
### -Entity
Parameter for the RetrieveRecordWallRequest operation.

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
### -PageNumber
Parameter for the RetrieveRecordWallRequest operation.

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
### -PageSize
Parameter for the RetrieveRecordWallRequest operation.

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
### -CommentsPerPost
Parameter for the RetrieveRecordWallRequest operation.

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
### -StartDate
Parameter for the RetrieveRecordWallRequest operation.

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
### -EndDate
Parameter for the RetrieveRecordWallRequest operation.

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
### -Type
Parameter for the RetrieveRecordWallRequest operation.

```yaml
Type: object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -Source
Parameter for the RetrieveRecordWallRequest operation.

```yaml
Type: object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
### -SortDirection
Parameter for the RetrieveRecordWallRequest operation.

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
### -Keyword
Parameter for the RetrieveRecordWallRequest operation.

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
### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### RetrieveRecordWallResponse

Returns the response from the `RetrieveRecordWallRequest` operation.

## NOTES

This cmdlet is auto-generated and wraps the Dataverse SDK message.

## RELATED LINKS

[Invoke-DataverseRequest](Invoke-DataverseRequest.md)

[Get-DataverseConnection](Get-DataverseConnection.md)
