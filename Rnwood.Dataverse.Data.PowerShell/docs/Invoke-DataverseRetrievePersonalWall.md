---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrievePersonalWall

## SYNOPSIS
Contains the data that is needed to retrieve pages of posts, including comments for each post, for all records that the calling user is following.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePersonalWallRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrievePersonalWallRequest)

## SYNTAX

```
Invoke-DataverseRetrievePersonalWall [-PageNumber <Int32>] [-PageSize <Int32>] [-CommentsPerPost <Int32>]
 [-StartDate <DateTime>] [-EndDate <DateTime>] [-Type <OptionSetValue>] [-Source <OptionSetValue>]
 [-SortDirection <Boolean>] [-Keyword <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to retrieve pages of posts, including comments for each post, for all records that the calling user is following.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrievePersonalWall -Connection <ServiceClient> -PageNumber <Int32> -PageSize <Int32> -CommentsPerPost <Int32> -StartDate <DateTime> -EndDate <DateTime> -Type <OptionSetValue> -Source <OptionSetValue> -SortDirection <Boolean> -Keyword <String>
```

## PARAMETERS

### -CommentsPerPost
Gets or sets, for retrieval, the number of comments per post. Required.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -EndDate
Gets or sets the end date and time of the posts that you want to retrieve. Optional.

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

### -Keyword
Gets or sets the Keyword for the request.

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

### -PageNumber
Gets or sets, for retrieval, a specific page of posts that is designated by its page number. Required.

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
Gets or sets, for retrieval, the number of posts per page. Required.

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

### -SortDirection
Gets or sets the SortDirection for the request.

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

### -Source
Gets or sets a value that specifies the source of the post. Optional.

```yaml
Type: OptionSetValue
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StartDate
Gets or sets the start date and time of the posts that you want to retrieve. Optional.

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
Reserved for future use.

```yaml
Type: OptionSetValue
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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

### System.Object
## NOTES

## RELATED LINKS
