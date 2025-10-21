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
Invoke-DataverseRequest -Request <OrganizationRequest> [-BatchSize <UInt32>] [-Retries <Int32>]
 [-InitialRetryDelay <Int32>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### NameAndInputs
```
Invoke-DataverseRequest [-RequestName] <String> [[-Parameters] <Hashtable>] [-BatchSize <UInt32>]
 [-Retries <Int32>] [-InitialRetryDelay <Int32>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### REST
```
Invoke-DataverseRequest [-Method] <HttpMethod> [-Path] <String> [[-Body] <PSObject>]
 [-CustomHeaders <Hashtable>] [-BatchSize <UInt32>] [-Retries <Int32>] [-InitialRetryDelay <Int32>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet allows you to execute any Dataverse request message.

Three parameter sets are supported:
1. **Request** - Pass an OrganizationRequest object from the SDK
2. **NameAndInputs** - Specify request name and parameters as a hashtable (simpler)
3. **REST** - Execute raw REST API calls with custom HTTP method, path, headers and body

This is useful for:
- Executing custom actions/API messages
- Calling Dataverse SDK messages not wrapped by specific cmdlets
- Making raw REST API calls for advanced scenarios

The response from the request is returned to the pipeline.

**Retry Logic:**

This cmdlet supports automatic retries for transient failures using exponential backoff:
- Use `-Retries` parameter to specify the number of retry attempts (default is 0 - no retries)
- Use `-InitialRetryDelay` to set the initial delay in seconds before the first retry (default is 5s)
- Each subsequent retry doubles the delay time (exponential backoff)
- After all retries are exhausted, the final exception is thrown
- Retries are particularly useful for transient errors like throttling or temporary connectivity issues

**Note:** Retry logic only applies to the **Request** and **NameAndInputs** parameter sets. REST API calls do not currently support retries.

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
PS C:\> $response = Invoke-DataverseRequest -connection $c myapi_EscalateCase @{
	Target = $Target
	Priority = $Priority
}
```

Invokes `myapi_EscalateCase` by using just the request name and parameters using existing connection `$c` and storing the response into a variable.

### Example 4

```powershell
PS C:\> invoke-dataverserequest -connection $c -method POST myapi_Example \
	-CustomHeaders @{
		foo = "bar"
	} \
	-Body @{
		a = 1
		b = 3
	}
```

Invokes the `GET` `myapi_Example` REST API using custom headers and body

### Example 5: Using retry logic for transient failures

```powershell
PS C:\> $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
PS C:\> $response = Invoke-DataverseRequest -Connection $c -Request $request -Retries 3 -InitialRetryDelay 500 -Verbose
```

Invokes a WhoAmI request with automatic retry on transient failures. Failed requests will be retried up to 3 times with delays of 5s, 10s, and 20s respectively. The `-Verbose` flag shows retry attempts and wait times.

## PARAMETERS

### -Body
Body of the REST API request. Can be a string (JSON) or a PSObject which will be converted to JSON.

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

### -CustomHeaders
Hashtable of custom HTTP headers to include in the REST API request.

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

### -InitialRetryDelay
Initial delay in seconds before first retry. Subsequent retries use exponential backoff (delay doubles each time). Default is 5s.

Only applies to Request and NameAndInputs parameter sets. REST API calls do not support retries.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 5
Accept pipeline input: False
Accept wildcard characters: False
```

### -Method
HTTP method to use for the REST API call (e.g., GET, POST, PATCH, DELETE).

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
Hashtable of parameters to pass to the request. Keys are parameter names and values are parameter values.

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
Path portion of the REST API URL (e.g., 'api/data/v9.2/contacts' or 'myapi_Example').

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
Name of the Dataverse request to execute. This should be the message name (e.g., WhoAmI, RetrieveMultiple, or a custom action name like myapi_EscalateCase).

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

### -Retries
Number of times to retry the request on failure. Default is 0 (no retries). Each retry uses exponential backoff based on the `-InitialRetryDelay` parameter.

Only applies to Request and NameAndInputs parameter sets. REST API calls do not support retries.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
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

### -BatchSize
Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.

```yaml
Type: UInt32
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

### Microsoft.Xrm.Sdk.OrganizationRequest

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
