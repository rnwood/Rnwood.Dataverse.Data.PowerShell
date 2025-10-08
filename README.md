# Rnwood.Dataverse.Data.PowerShell

This is a module for PowerShell to connect to Microsoft Dataverse (used by many Dynamics 365 and Power Apps applications as well as others) and query and manipulate data.

This module works in PowerShell Desktop and PowerShell Core, so it should work on any platform where Core is supported.

Features:
- Creating, updating, upserting and deleting records including M:M records.
- Inputs and outputs simple PowerShell objects (instead of SDK Entity class). 
- Automatic type conversion using metadata to try and convert incoming values to the correct type. For example choice values can be mapped from their label.
- Automatic conversion for lookup type values in both input and output directions. You can use the name of the record to refer to a record you want to associate with as long as it's unique.
- On behalf of (delegation) support to create/update records on behalf of another user.
- Querying records using a variety of methods, with full support for returning the full result set across pages.
 - Querying records using a variety of methods, with full support for returning the full result set across pages. Supports concise hashtable-based filters including grouped logical expressions (and/or) with arbitrary nesting depth.
 - Querying records using a variety of methods, with full support for returning the full result set across pages. Supports concise hashtable-based filters including grouped logical expressions (and/or), negation (`not`) and exclusive-or (`xor`) with arbitrary nesting depth. Note: `xor` groups are limited to 8 items to avoid exponential expansion for certain exclusion scenarios.
- Batching support to create/update/upsert many records in a single request to service.
- Wide variety of auth options for both interactive and unattended use.

Non features:
- Support for connecting to on-premise environments.

# How to install

This module is not signed (donation of funds for code signing certificate are welcome). So PowerShell must be configured to allow loading unsigned scripts.

```powershell
Set-ExecutionPolicy –ExecutionPolicy RemoteSigned –Scope CurrentUser
```
To install:
```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

To update:
```
Update-Module Rnwood.Dataverse.Data.PowerShell -Force
```
# PowerShell Best Practices

## ⚠️ Always Use ErrorActionPreference Stop

**It is strongly recommended to set `$ErrorActionPreference = "Stop"` at the beginning of your scripts.** This ensures that errors are treated as terminating errors and will stop script execution, preventing cascading failures and data corruption.

```powershell
# Add this at the start of your scripts
$ErrorActionPreference = "Stop"
```

Without this setting, PowerShell's default behavior is to continue execution after non-terminating errors, which can lead to unexpected results when working with Dataverse data.

**Learn more:**
- [Microsoft Docs: about_Preference_Variables](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_preference_variables)
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)

# Quick Start and Examples
Get a connection to a target Dataverse environment using the `Get-DataverseConnection` cmdlet (also available as `Connect-DataverseConnection` alias).

*Example: Get a connection to MYORG... using interactive authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
```

*Example: Get a connection by selecting from available environments:*
```powershell
$c = Get-DataverseConnection -interactive
```

See the full documentation for `Get-DataverseConnection` for other non-interactive auth types.

### Default Connection

You can set a connection as the default, so you don't have to pass `-Connection` to every cmdlet:

*Example: Set a default connection and use it implicitly:*
```powershell
# Set a connection as default
Connect-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -SetAsDefault

# Now you can omit -Connection from all cmdlets
Get-DataverseRecord -tablename contact
Set-DataverseRecord -tablename contact @{firstname="John"; lastname="Doe"}

# You can retrieve the current default connection
$currentDefault = Get-DataverseConnection -GetDefault
```

This is especially useful in interactive sessions and scripts where you're working with a single environment.

### Authentication Methods

The module supports multiple authentication methods:
- **Interactive**: Browser-based authentication (good for development). Omit the URL to select from available environments.
- **Device Code**: Authentication via device code flow (good for remote/headless scenarios). Omit the URL to select from available environments.
- **Username/Password**: Basic credential authentication. Omit the URL to select from available environments.
- **Client Secret**: Service principal authentication (good for automation)
- **DefaultAzureCredential**: Automatic credential discovery in Azure environments (tries environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser). Omit the URL to select from available environments.
- **ManagedIdentity**: Azure managed identity authentication (system-assigned or user-assigned). Omit the URL to select from available environments.
- **Connection String**: Advanced scenarios using connection strings

*Example: Using DefaultAzureCredential in Azure environments:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -DefaultAzureCredential
```

*Example: Using Managed Identity on Azure VM/Functions/App Service:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -ManagedIdentity
```

Every command that needs a connection to a Dataverse environment exposes a `-Connection` parameter. This parameter is now optional - if not provided, the cmdlet will use the default connection (if one has been set with `-SetAsDefault`).

*Example: Get all `contact` records using explicit connection:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
Get-DataverseRecord -connection $c -tablename contact
```

*Example: Get all `contact` records using default connection:*
```powershell
Connect-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -SetAsDefault
Get-DataverseRecord -tablename contact
```

*Example: Exclude records where firstname='Rob' or lastname='Smith':*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -ExcludeFilterValues @{
  'or' = @(
    @{ firstname = 'Rob' },
    @{ lastname = 'Smith' }
  )
}
```

*Example: Include lastnames One or Two, but exclude firstname Rob:* 
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'or' = @(
  @{
    lastname = 'One'
  },
  @{
    lastname = 'Two'
  }
  )
} -ExcludeFilterValues @{
  'xor' = @(
    @{ emailaddress1 = @{
      operator = 'NotNull'
    } },
    @{ mobilephone = @{
      operator = 'NotNull'
    } }
  )
}
```

*Example: Get contacts where (firstname = 'Rob' OR firstname = 'Joe') AND lastname = 'One':*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'and' = @(
  @{ 'or' = @(
    @{
      firstname = 'Rob'
    },
    @{
      firstname = 'Joe'
    }
  ) },
  @{
    lastname = 'One'
  }
  )
}
```

*Example: Negation (NOT) — get contacts that do NOT have firstname 'Rob':*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'not' = @{
      firstname = 'Rob'
  }
}
```

*Example: Exclusive-or (XOR) — exactly one of the subfilters matches:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{
  'xor' = @(
  @{
    firstname = 'Rob'
  },
  @{
    firstname = 'Joe'
  }
  )
}
```

Note: `xor` can cause combinatorial expansion when negated or used in complex exclude filters; the cmdlet enforces a limit of 8 items in an `xor` group. Use smaller XOR groups, FetchXML, or SQL for large conditions.

The cmdlets input and output normal PowerShell objects to/from the pipeline, so you can combine multiple command easily. You can also create multiple connections allowing you to work with more than one environment in the same script.

*Example: Copy from one environment to another updating any records that already exist if a match with the same name already exists*
```powershell
$c1 = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
$c2 = Get-DataverseConnection -url https://anotherorg.crm11.dynamics.com -interactive

Get-DataverseRecord -connection $c1 -tablename contact |
   Set-DataverseRecord -connection $c2 -matchon fullname
```

You can also use other commands.

*Example: Get all `contact` records and convert to JSON:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive

Get-DataverseRecord -connection $c -tablename contact | 
   ConvertTo-JSON
```

### Linking Related Tables

The `-Links` parameter supports joining related tables in queries using a simplified hashtable syntax. This is useful for filtering or selecting data from related entities.

*Example: Get contacts and join to their parent account:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
}
```

*Example: Left outer join with an alias:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
    type = 'LeftOuter'
    alias = 'parentAccount'
}
```

*Example: Join with filter on linked entity (only include contacts from accounts starting with 'Contoso'):*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @{
    'contact.accountid' = 'account.accountid'
    filter = @{
        name = @{ operator = 'Like'; value = 'Contoso%' }
        statecode = @{ operator = 'Equal'; value = 0 }
    }
}
```

*Example: Multiple joins:*
```powershell
Get-DataverseRecord -Connection $c -TableName contact -Links @(
    @{ 'contact.accountid' = 'account.accountid'; type = 'LeftOuter' },
    @{ 'contact.ownerid' = 'systemuser.systemuserid' }
)
```

The simplified syntax supports:
- **Link specification**: `'fromTable.fromAttribute' = 'toTable.toAttribute'`
- **type** (optional): `'Inner'` (default) or `'LeftOuter'`
- **alias** (optional): String alias for the linked entity
- **filter** (optional): Hashtable with filter conditions (same format as `-FilterValues`)

## Error Handling

When working with batch operations, errors don't stop processing - all records are attempted and errors are collected. You can correlate errors back to the specific input records that failed.

*Example: Handle errors in batch operations:*
```powershell
$records = @(
    @{ firstname = "John"; lastname = "Doe" }
    @{ firstname = "Jane"; lastname = "Smith" }
)

$errors = @()
$records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly `
    -ErrorVariable +errors -ErrorAction SilentlyContinue

# Process errors - each error's TargetObject contains the input that failed
foreach ($err in $errors) {
    Write-Host "Failed: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

The `Exception.Message` contains full server response including ErrorCode, Message, TraceText, and InnerFault details for troubleshooting.

To stop on first error instead, use `-BatchSize 1` with `-ErrorAction Stop`.

See full documentation: [Set-DataverseRecord Error Handling](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-12-handle-errors-in-batch-operations) | [Remove-DataverseRecord Error Handling](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md#example-3-handle-errors-in-batch-delete-operations)

## Getting IDs of Created Records

Use the `-PassThru` parameter to get the IDs of newly created records, which is useful for linking records or tracking what was created.

*Example: Capture IDs of created records:*
```powershell
$contacts = @(
    @{ firstname = "John"; lastname = "Doe" }
    @{ firstname = "Jane"; lastname = "Smith" }
)

$created = $contacts | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru

foreach ($record in $created) {
    Write-Host "Created: $($record.firstname) $($record.lastname) with ID: $($record.Id)"
}
```

*Example: Link records using PassThru:*
```powershell
# Create parent and capture its ID
$account = @{ name = "Contoso Ltd" } | 
    Set-DataverseRecord -Connection $c -TableName account -CreateOnly -PassThru

# Create child linked to parent
$contact = @{ 
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = $account.Id
} | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru
```

See full documentation: [Set-DataverseRecord PassThru Examples](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-15-get-ids-of-created-records-using-passthru)

## Batch Operations

By default, `Set-DataverseRecord` and `Remove-DataverseRecord` automatically batch operations when processing multiple records (default batch size is 100). This improves performance by reducing round trips to the server.

Key behaviors:
- Batching uses `ExecuteMultipleRequest` with `ContinueOnError = true` - all records are attempted even if some fail
- Errors include the original input object as `TargetObject` for correlation
- Use `-BatchSize 1` to disable batching and stop on first error
- Use `-BatchSize <number>` to control batch size for performance tuning

*Example: Control batch size:*
```powershell
# Create 500 records in batches of 50
$records = 1..500 | ForEach-Object {
    @{ name = "Account $_" }
}

$records | Set-DataverseRecord -Connection $c -TableName account -BatchSize 50 -CreateOnly
```

See full documentation: [Set-DataverseRecord Batching](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md#example-7-control-batch-size) | [Remove-DataverseRecord Batching](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md)

## Main Cmdlets

[Get-DataverseConnection](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) - Creates a connection to a Dataverse environment.

[Get-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) - Query for existing records

[Set-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) - Create or update records.

[Remove-DataverseRecord](Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) - Delete existing records.

[Invoke-DataverseRequest](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) - Allows you to execute arbitrary Dataverse API requests.

[Invoke-DataverseSql](Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) - Execute SQL queries against Dataverse using Sql4Cds. `CREATE`, `DELETE`, `INSERT` and `UPDATE` are all supported.


[Set-DataverseRecordsFolder](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md) - Helper function to write stream of records to a folder each as individual JSON files. 

[Get-DataverseRecordsFolder](Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecordsFolder.md) - Helper function to read  stream of records from a folder containing each as individual JSON files. 

## Migration from Microsoft.Xrm.Data.PowerShell

If you're migrating from `Microsoft.Xrm.Data.PowerShell`, see the [Examples Comparison Guide](Examples-Comparison.md) which shows side-by-side examples of common operations in both modules.

# Full Documentation
You can see documentation using the standard PowerShell help and autocompletion systems.

To see a complete list of cmdlets:
```powershell
get-command -Module Rnwood.Dataverse.Data.PowerShell
```

To see the help for an individual cmdlet (e.g `get-dataverseconnection`):
```powershell
get-help get-dataverseconnection -detailed
```

You can also simply enter the name of the cmdlet at the PS prompt and then press `F1`. 

Pressing `tab` completes parameter names (press again to see more suggestions), and you can press `F1` to jump to the help for those one you have the complete name.

[You can also view the documentation for the latest development version here](Rnwood.Dataverse.Data.PowerShell/docs). Note that this may not match the version you are running. Use the above preferably to get the correct and matching help for the version you are running.

## Using PowerShell Standard Features

This module follows PowerShell conventions and supports all standard PowerShell features. Here's how to use them effectively with Dataverse operations.

### WhatIf and Confirm - Preview Changes Before Execution

The `-WhatIf` parameter lets you preview what would happen without actually making changes. The `-Confirm` parameter prompts for confirmation before each operation.

**Supported cmdlets:** `Set-DataverseRecord`, `Remove-DataverseRecord`, and all `Invoke-Dataverse*` cmdlets that modify data.

*Example: Preview record creation with -WhatIf:*
```powershell
# See what would be created without actually creating it
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "John"
    lastname = "Doe"
} -CreateOnly -WhatIf
```

*Example: Get confirmation prompt before deleting:*
```powershell
# Prompt for confirmation before each delete
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = "TestUser" } |
    Remove-DataverseRecord -Connection $c -Confirm
```

*Example: Batch operations with WhatIf:*
```powershell
# Preview all updates that would be made
$records = Get-DataverseRecord -Connection $c -TableName account -Top 10
$records | ForEach-Object { $_.name = "$($_.name) - Updated" }
$records | Set-DataverseRecord -Connection $c -WhatIf
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: Supporting -WhatIf and -Confirm](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/how-to-request-confirmations)

### Verbose Output - See Detailed Operation Information

Use the `-Verbose` parameter to see detailed information about what the cmdlet is doing. This is especially useful for troubleshooting and understanding complex operations.

*Example: See detailed information during record creation:*
```powershell
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "Jane"
    lastname = "Smith"
} -CreateOnly -Verbose
```

*Example: Monitor batch operations with verbose output:*
```powershell
# See progress as records are processed in batches
$records = 1..50 | ForEach-Object { @{ name = "Account $_" } }
$records | Set-DataverseRecord -Connection $c -TableName account -BatchSize 10 -Verbose
```

**Learn more:**
- [Microsoft Docs: Write-Verbose](https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/write-verbose)
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)

### Error Handling - Control How Errors Are Handled

PowerShell provides powerful error handling through `-ErrorAction`, `-ErrorVariable`, and the `$ErrorActionPreference` preference variable.

*Example: Stop on first error:*
```powershell
# Stop immediately if any error occurs
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
    firstname = "Test"
} -ErrorAction Stop
```

*Example: Continue on errors and collect them:*
```powershell
# Continue processing and collect errors for review
$errors = @()
$records = Get-DataverseRecord -Connection $c -TableName contact -Top 10
$records | Set-DataverseRecord -Connection $c -ErrorVariable +errors -ErrorAction SilentlyContinue

# Review what failed
foreach ($err in $errors) {
    Write-Host "Failed: $($err.TargetObject.Id) - $($err.Exception.Message)"
}
```

*Example: Use Try-Catch with Stop:*
```powershell
try {
    Set-DataverseRecord -Connection $c -TableName contact -InputObject @{
        firstname = "John"
        lastname = "Doe"
    } -CreateOnly -ErrorAction Stop
    Write-Host "Record created successfully"
} catch {
    Write-Host "Failed to create record: $_"
}
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: about_Preference_Variables](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_preference_variables)
- [Microsoft Docs: about_Try_Catch_Finally](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_try_catch_finally)

### Warning Messages - Control Warning Output

Use `-WarningAction` to control how warning messages are handled.

*Example: Suppress warnings:*
```powershell
# Run without showing warnings
Get-DataverseRecord -Connection $c -TableName contact -WarningAction SilentlyContinue
```

*Example: Treat warnings as errors:*
```powershell
# Stop execution if a warning occurs
Get-DataverseRecord -Connection $c -TableName contact -WarningAction Stop
```

**Learn more:**
- [Microsoft Docs: about_CommonParameters](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters)
- [Microsoft Docs: Write-Warning](https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/write-warning)

### Tab Completion - Speed Up Command Entry

PowerShell provides intelligent tab completion for cmdlet names, parameter names, and even parameter values in many cases.

**How to use:**
- Type a few letters of a cmdlet name and press `Tab` to cycle through matching cmdlets
- Type a parameter name (with `-`) and press `Tab` to complete it
- Press `Tab` after `-TableName` to see available table names (when connected)
- Press `Ctrl+Space` to see all available completions

*Example workflow:*
```powershell
# Type "Get-Dat" and press Tab -> completes to "Get-DataverseRecord"
# Add "-Tab" and press Tab -> completes to "-TableName"
# Add "-Col" and press Tab -> completes to "-Columns"

Get-DataverseRecord -Connection $c -TableName contact -Columns firstname, lastname
```

**Learn more:**
- [Microsoft Docs: about_Tab_Expansion](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_tab_expansion)
- [Microsoft Docs: TabCompletion](https://learn.microsoft.com/powershell/scripting/learn/shell/tab-completion)

### Contextual Help - Get Help Without Leaving the Console

PowerShell provides built-in help that you can access without leaving the command line.

**Get help for a cmdlet:**
```powershell
# Basic help
Get-Help Get-DataverseRecord

# Detailed help with examples
Get-Help Get-DataverseRecord -Detailed

# Full help including technical details
Get-Help Get-DataverseRecord -Full

# Just show examples
Get-Help Get-DataverseRecord -Examples

# Online version (opens in browser)
Get-Help Get-DataverseRecord -Online
```

**Use F1 for instant help:**
- Type a cmdlet name and press `F1` to open help
- Complete a parameter name and press `F1` to jump to that parameter's documentation

**Search for help topics:**
```powershell
# Find all cmdlets in this module
Get-Command -Module Rnwood.Dataverse.Data.PowerShell

# Search help for keywords
Get-Help *Dataverse* 

# Find cmdlets that work with records
Get-Command *Record* -Module Rnwood.Dataverse.Data.PowerShell
```

**Learn more:**
- [Microsoft Docs: Get-Help](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/get-help)
- [Microsoft Docs: about_Comment_Based_Help](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_comment_based_help)
- [Microsoft Docs: Updatable Help System](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_updatable_help)

### Command History - Reuse and Search Previous Commands

PowerShell keeps a history of commands you've run, making it easy to recall and modify them.

**Basic history navigation:**
- Press `↑` (up arrow) to recall previous commands
- Press `↓` (down arrow) to move forward through history
- Press `F8` after typing a few letters to search history for matching commands

**View and search history:**
```powershell
# View recent command history
Get-History

# Get a specific command by ID
Get-History -Id 10

# Search for commands containing "Dataverse"
Get-History | Where-Object { $_.CommandLine -like "*Dataverse*" }

# Re-run a command by ID
Invoke-History -Id 10
```

**Advanced history with PSReadLine (PowerShell 5.1+ / Core):**
- Press `Ctrl+R` for interactive reverse search
- Type to search, press `Ctrl+R` again to see older matches
- Press `Enter` to execute or `Esc` to edit

**Save and load history:**
```powershell
# Save history to a file
Get-History | Export-Clixml -Path ./dataverse-history.xml

# Load history from a file
Import-Clixml -Path ./dataverse-history.xml | Add-History
```

**Learn more:**
- [Microsoft Docs: about_History](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_history)
- [Microsoft Docs: PSReadLine Module](https://learn.microsoft.com/powershell/module/psreadline/)
- [GitHub: PSReadLine Key Bindings](https://github.com/PowerShell/PSReadLine#usage)

### Additional Resources

**General PowerShell Learning:**
- [Microsoft Learn: PowerShell Scripting](https://learn.microsoft.com/powershell/scripting/overview)
- [PowerShell Documentation](https://learn.microsoft.com/powershell/)
- [PowerShell Gallery: Find More Modules](https://www.powershellgallery.com/)

**Video Tutorials:**
- [Microsoft Learn: PowerShell for Beginners](https://learn.microsoft.com/shows/powershell-for-beginners/)
- [PowerShell.org: YouTube Channel](https://www.youtube.com/powershellorg)

**Community and Blogs:**
- [PowerShell.org Community](https://powershell.org/)
- [PowerShell Team Blog](https://devblogs.microsoft.com/powershell/)
- [Reddit: r/PowerShell](https://www.reddit.com/r/PowerShell/)

## FAQ
### Why another module? What's wrong with `Microsoft.Xrm.Data.PowerShell`?
`Microsoft.Xrm.Data.PowerShell` is a popular module by a Microsoft employee - note that is it not official or supported by MS, although it is released under the open source MS-PL license.

It has the following drawbacks/limitations:
- It is only supported on the .NET Framework, not .NET Core and .NET 5.0 onwards. This means officially it works on Windows only and in PowerShell Desktop not PowerShell Core. For example you can not use it on Linux-based CI agents.
  **`Rnwood.Dataverse.Data.PowerShell` works on both .NET Framework and .NET Core and so will run cross-platform.**
- It is a thin wrapper around the Dataverse SDK and doesn't use idiomatic PowerShell conventions. This makes it harder to use. For example the caller is expected to understand and implement paging or it might get incomplete results.
  **`Rnwood.Dataverse.Data.PowerShell` tries to fit in with PowerShell conventions to make it easier to write scripts. `WhatIf`, `Confirm` and `Verbose` are implemented. Paging is automatic.

### Why not just script calls to the REST API?
Basic operations are easy using the REST API, but there are many features that are not straightforward to implement, or you might forget to handle (e.g. result paging). This module should be simpler to consume then the REST API directly.

This module also emits and consumes typed values (dates, numbers) instead of just strings. Doing this makes it easier to work with other PowerShell commands and modules.
