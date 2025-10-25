# Authentication Methods

<!-- TOC -->
<!-- /TOC -->

The module supports multiple authentication methods for different scenarios:

## Interactive Authentication

Browser-based authentication (good for development). Omit the URL to select from available environments.

*Example: Get a connection to MYORG using interactive authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
```

*Example: Get a connection by selecting from available environments:*
```powershell
$c = Get-DataverseConnection -interactive
```

## PAC CLI Profile

Power Platform CLI (PAC) authentication profile (leverages existing PAC CLI authentication).

*Example: Using the current PAC CLI profile:*
```powershell
Get-DataverseConnection -FromPac -SetAsDefault
```

*Example: Using a specific PAC CLI profile by name or index:*
```powershell
$c = Get-DataverseConnection -FromPac -Profile "MyDevProfile"
# or by index
$c = Get-DataverseConnection -FromPac -Profile "0"
```

## Device Code

Authentication via device code flow (good for remote/headless scenarios).

*Example: Using device code authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -devicecode
```

## Username/Password

Basic credential authentication (not recommended)

*Example: Using username and password authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com" -password "mypassword"
```

## Client Secret

Service principal authentication (good for automation).

*Example: Using client secret authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

## Client Certificate

Certificate-based service principal authentication (good for secure automation).

*Example: Using client certificate from file:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatepath "C:\certs\mycert.pfx" -certificatepassword "P@ssw0rd"
```

*Example: Using client certificate from Windows certificate store:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatethumbprint "A1B2C3D4E5F6789012345678901234567890ABCD"
```

*Example: Using client certificate from LocalMachine store:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -certificatethumbprint "A1B2C3D4E5F6789012345678901234567890ABCD" -certificatestorelocation LocalMachine
```

## DefaultAzureCredential

Automatic credential discovery in Azure environments (tries environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser).

*Example: Using DefaultAzureCredential in Azure environments:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -DefaultAzureCredential
```

## ManagedIdentity

Azure managed identity authentication (system-assigned or user-assigned).

*Example: Using Managed Identity on Azure VM/Functions/App Service:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -ManagedIdentity
```

*Example: Using user-assigned managed identity:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -ManagedIdentity -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"
```

## Connection String

Advanced scenarios using connection strings.

*Example: Using a Dataverse SDK connection string:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -connectionstring "AuthType=ClientSecret;ClientId=3004eb1e-7a00-45e0-a1dc-6703735eac18;ClientSecret=itsasecret;Url=https://myorg.crm11.dynamics.com"
```

## See Also

- [Connection Management](../core-concepts/connections.md) - Learn about default connections, named connections, and advanced features
- [Get-DataverseConnection](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) - Full cmdlet documentation
