# Rnwood.Dataverse.Data.PowerShell

This is a module for PowerShell to connect to Microsoft Dataverse (used by many Dynamics 365 and Power Apps applications as well as others) and query and manipulate data.

This module works in PowerShell Desktop and PowerShell Core, so it should work on any platform where Core is supported.

Features:
- Creating, updating, upserting and deleting records including M:M records.
- Automatic conversion for lookup type values in both input and output directions. You can use the name of the record to refer to a record you want to associate with as long as it's unique.
- On behalf of (delegation) support.
- Querying records using a variety of methods, with full support for returning the full result set across pages.
- Batching support to create/update/upsert many records in a single request to service.

Non features:
- Support for connecting to on-premise environments.

# How to install

This module is not signed (donation of funds for code signing certificate are welcome). So PowerShell must be configured to allow loading unsigned scripts.

```
Set-ExecutionPolicy –ExecutionPolicy RemoteSigned –Scope CurrentUser
```
To install:
```
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

To update:
```
Update-Module Rnwood.Dataverse.Data.PowerShell -Force
```

# Documentation
You can see cmdlet documentation using the standard PowerShell help system.

```
get-help get-dataverseconnection
```

[You can also view the documentation for the latest development version here](Rnwood.Dataverse.Data.PowerShell/docs). Note that this may not match the version you are running. Use the above preferably.


## FAQ
### Why another module? What's wrong with `Microsoft.Xrm.Data.PowerShell`?
The primary reason is so that it can run in PowerShell Core as well as PowerShell Desktop. PowerShell Core allows it to run cross-platform and not just on Windows.

### Why not just script calls to the REST API?
Basic operations are easy using the REST API, but there are many features that are not straightforward to implement, or you might forget to handle (e.g. result paging). This module should be simpler to consume then the REST API directly.

This module also emits and consumes typed values (dates, numbers) instead of just strings. Doing this makes it easier to work with other PowerShell commands and modules.
