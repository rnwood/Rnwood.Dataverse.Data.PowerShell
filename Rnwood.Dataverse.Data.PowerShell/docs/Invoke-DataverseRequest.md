---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRequest

## SYNOPSIS
Invokes an arbitrary Dataverse request and returns the response.

## SYNTAX

### Request
```
Invoke-DataverseRequest -Connection <ServiceClient> -Request <OrganizationRequest>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### NameAndInputs
```
Invoke-DataverseRequest -Connection <ServiceClient> [-RequestName] <String> [[-Parameters] <Hashtable>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### REST
```
Invoke-DataverseRequest -Connection <ServiceClient> [-Method] <HttpMethod> [-Path] <String>
 [[-Body] <PSObject>] [-CustomHeaders <Hashtable>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```powershell
PS C:\> $request = new-object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
PS C:\> $response = Invoke-DataverseRequest -connection $c -request $request
```

Invokes `WhoAmIRequest` using the type from the Dataverse SDK using existing connection `$c` and storing the response into a variable.

### Example 2

```powershell
PS C:\> $request = new-object Microsoft.Xrm.Sdk.OrganizationRequest "myapi_EscalateCase"
PS C:\> $request["Target"] = new-object Microsoft.Xrm.Sdk.EntityReference "incident", "{DC66FE5D-B854-4F9D-BA63-4CEA4257A8E9}"
PS C:\> $request["Priority"] = new-object Microsoft.Xrm.Sdk.OptionSetValue 1
PS C:\> $response = Invoke-DataverseRequest -connection $c -request $request
```

Invokes `myapi_EscalateCase` using without using a request type from the Dataverse SDK using existing connection `$c` and storing the response into a variable.

### Example 3

```powershell
PS C:\> $Target = new-object Microsoft.Xrm.Sdk.EntityReference "incident", "{DC66FE5D-B854-4F9D-BA63-4CEA4257A8E9}"
PS C:\> $Priority = new-object Microsoft.Xrm.Sdk.OptionSetValue 1
PS C:\> $response = Invoke-DataverseRequest -connection $c myapi_EscalateCase @{"Target"=$Target; "Priority=$Priority}
```

Invokes `myapi_EscalateCase` by using just the request name and parameters using existing connection `$c` and storing the response into a variable.

### Example 4

```powershell
PS C:\>invoke-dataverserequest -connection $c -method POST myapi_Example -CustomHeaders @{"foo"="bar"} -Body @{"a"=1; "b"=3}
```

Invokes the `GET` `myapi_Example` REST API using custom headers and body

## PARAMETERS

### -Body
{{ Fill Body Description }}

```yaml
Type: PSObject
Parameter Sets: REST
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnnection cmdlet

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

### -CustomHeaders
{{ Fill CustomHeaders Description }}

```yaml
Type: Hashtable
Parameter Sets: REST
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Method
{{ Fill Method Description }}

```yaml
Type: HttpMethod
Parameter Sets: REST
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Parameters
{{ Fill Parameters Description }}

```yaml
Type: Hashtable
Parameter Sets: NameAndInputs
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
{{ Fill Path Description }}

```yaml
Type: String
Parameter Sets: REST
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Request
Request to execute using the `OrganizationRequest` class or subclass from the SDK.

See
https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.organizationrequest?view=dataverse-sdk-latest

```yaml
Type: OrganizationRequest
Parameter Sets: Request
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -RequestName
{{ Fill RequestName Description }}

```yaml
Type: String
Parameter Sets: NameAndInputs
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
See standard PS docs.

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

### Microsoft.Xrm.Sdk.OrganizationRequest

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
