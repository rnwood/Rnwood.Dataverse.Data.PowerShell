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

# Documentation
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
