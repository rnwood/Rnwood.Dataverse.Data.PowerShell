---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAppModule

## SYNOPSIS
Retrieves app module (model-driven app) information from a Dataverse environment.

## SYNTAX

```
Get-DataverseAppModule [[-Id] <Guid>] [-UniqueName <String>] [-Name <String>] [-Raw] [-Unpublished]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseAppModule cmdlet retrieves model-driven app definitions from Dataverse environments. App modules define the structure, navigation, and components of model-driven applications.

By default, the cmdlet returns parsed app module information with key properties like Id, UniqueName, Name, Description, and PublishedOn. Use the -Raw parameter to return all attribute values from the appmodule record.

App modules can be filtered by ID, unique name, or display name (with wildcard support).

## EXAMPLES

### Example 1: Get a specific app module by ID
```powershell
PS C:\> Get-DataverseAppModule -Connection $c -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific app module by its ID with parsed key properties.

### Example 2: Get an app module by UniqueName
```powershell
PS C:\> Get-DataverseAppModule -Connection $c -UniqueName "msdyn_SalesHub"
```

Retrieves the Sales Hub app module using its unique name.

### Example 3: Get all app modules
```powershell
PS C:\> Get-DataverseAppModule -Connection $c
```

Retrieves all app modules in the environment.

### Example 4: Find app modules by name with wildcards
```powershell
PS C:\> Get-DataverseAppModule -Connection $c -Name "Sales*"
```

Finds all app modules whose names start with "Sales" using wildcard pattern matching.

### Example 5: Get app module with raw values
```powershell
PS C:\> Get-DataverseAppModule -Connection $c -Id $appId -Raw
```

Retrieves an app module with all raw attribute values instead of parsed properties.

### Example 6: Get app module and its components
```powershell
PS C:\> $app = Get-DataverseAppModule -Connection $c -UniqueName "myapp"
PS C:\> $components = Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $app.Id
```

Gets an app module and then retrieves all its components.

## PARAMETERS

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

### -Id
The ID of the app module to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
The name of the app module to retrieve.
Supports wildcards.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Raw
Return raw values instead of display values

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

### -UniqueName
The unique name of the app module to retrieve.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Unpublished
Allows unpublished records to be retrieved instead of the default published

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

### System.Guid
### System.String
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Default Behavior:**
- Returns parsed app module information with key properties
- Automatically pages through results if multiple app modules match criteria
- Supports filtering by ID, UniqueName, or Name (with wildcards)

**Common Properties:**
- Id: The unique identifier (GUID) of the app module
- UniqueName: The schema name used to reference the app
- Name: The display name shown in the app launcher
- Description: Optional description of the app's purpose
- PublishedOn: When the app was last published
- Url: The relative URL for the app
- WebResourceId: The web resource ID for the app icon
- FormFactor: The form factor (1=Main, 2=Quick, 3=Preview, 4=Dashboard)
- ClientType: The client type for the app
- NavigationType: The navigation type (SingleSession or MultiSession)
- IsFeatured: Whether the app is a featured app

## RELATED LINKS
