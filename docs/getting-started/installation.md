
# Installation


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
