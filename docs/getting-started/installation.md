<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Installation](#installation)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Installation

<!-- TOC -->
<!-- /TOC -->

This module is not signed (donation of funds for code signing certificate are welcome). So PowerShell must be configured to allow loading unsigned scripts that you install from remote sources (the Powershell gallery).

```powershell
Set-ExecutionPolicy –ExecutionPolicy RemoteSigned –Scope CurrentUser
```

To install:

```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

> [!NOTE]
> Pinning the specific version you have tested is recommended for important script.
> Then you can test and move forwards in a controlled way.

Add `-AllowPrerelease` to install the bleeding edge development build.

To install a specific version:

```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -RequiredVersion 100.0.0 -Scope CurrentUser
```

To update:

```powershell
Update-Module Rnwood.Dataverse.Data.PowerShell -Force
```

Add `-AllowPrerelease` to install the bleeding edge development build.
