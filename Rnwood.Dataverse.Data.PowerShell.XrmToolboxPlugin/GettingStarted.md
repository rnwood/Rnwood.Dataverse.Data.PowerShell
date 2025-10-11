# Getting Started with PowerShell Console Plugin

Welcome to the PowerShell Console plugin for XrmToolbox! This plugin provides a powerful PowerShell console directly within XrmToolbox, pre-loaded with the Rnwood.Dataverse.Data.PowerShell module and automatically connected to your current Dataverse environment.

## Quick Start

### 1. Opening the Console

The PowerShell console is embedded directly in this XrmToolbox tab. When you open the plugin, it automatically:

- ✅ Loads the PowerShell module (bundled with the plugin)
- ✅ Connects to your current Dataverse environment
- ✅ **Sets the connection as default** - you can omit `-Connection` parameter!
- ✅ Creates a `$connection` variable for explicit use when needed

### 2. Your First Command

Try querying some records - **no `-Connection` parameter needed**:

```powershell
# Simple syntax - uses default connection
Get-DataverseRecord -TableName account

# Or use explicit connection variable
Get-DataverseRecord -Connection $connection -TableName account
```

### 3. Common Operations

#### Query Records
```powershell
# Get all accounts (simplified - no -Connection needed!)
Get-DataverseRecord -TableName account

# Get specific columns
Get-DataverseRecord -TableName contact -Columns name, emailaddress1

# Filter records
Get-DataverseRecord -TableName account -Filter "name eq 'Contoso'"
```

#### Create Records
```powershell
$newAccount = @{ 
    name = "Contoso Ltd" 
    telephone1 = "555-1234"
}
# No -Connection parameter needed
Set-DataverseRecord -TableName account -Record $newAccount
```

#### Update Records
```powershell
$update = @{ 
    accountid = "guid-here"
    telephone1 = "555-5678"
}
Set-DataverseRecord -TableName account -Record $update
```

#### Delete Records
```powershell
Remove-DataverseRecord -TableName account -Id "guid-here"
```

#### SQL Queries
```powershell
Invoke-DataverseSql -Sql @"
SELECT TOP 10 
    name, 
    accountnumber, 
    revenue
FROM account
WHERE statecode = 0
ORDER BY createdon DESC
"@
```

## Getting Help

### In-Console Help
```powershell
# Get help for a specific cmdlet
Get-Help Get-DataverseRecord -Full

# List all available cmdlets
Get-Command -Module Rnwood.Dataverse.Data.PowerShell

# Get examples
Get-Help Get-DataverseRecord -Examples
```

### Online Resources

- **Full Documentation**: [GitHub Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
- **Cmdlet Reference**: [README.md](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/README.md)
- **Report Issues**: [GitHub Issues](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/issues)

## Connection Management

### Automatic Connection

The plugin automatically connects using your XrmToolbox credentials. The connection is:

- ✅ **Set as default** - no need to pass `-Connection` to cmdlets
- ✅ Available in the `$connection` variable for explicit use

**What does "default connection" mean?**

When a connection is set as default, you can omit the `-Connection` parameter:

```powershell
# Simplified syntax (recommended)
Get-DataverseRecord -TableName account

# Explicit syntax (also works)
Get-DataverseRecord -Connection $connection -TableName account
```

### Manual Connection (if needed)

If automatic connection fails or you need to connect to a different environment:

```powershell
# Interactive authentication (set as default)
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive -SetAsDefault

# Client secret authentication
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" `
    -ClientId "your-client-id" `
    -ClientSecret "your-client-secret" `
    -SetAsDefault
```

### Working with Multiple Connections

Need to work with multiple Dataverse environments? Here's how:

```powershell
# Primary connection (already connected by plugin)
# This is the default connection

# Connect to a secondary environment (don't set as default)
$conn2 = Get-DataverseConnection -Url "https://org2.crm.dynamics.com" -Interactive

# Use default connection (primary) - no -Connection parameter
Get-DataverseRecord -TableName account

# Use secondary connection explicitly
Get-DataverseRecord -Connection $conn2 -TableName contact

# Copy data between environments
$accounts = Get-DataverseRecord -TableName account -Filter "name eq 'Contoso'"
foreach ($account in $accounts) {
    Set-DataverseRecord -Connection $conn2 -TableName account -Record $account
}
```

For more multi-environment scenarios, see the [Common Use-Cases](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell#common-use-cases) section in the main README.

### Check Current Connection

```powershell
# Uses default connection
$whoami = Get-DataverseWhoAmI
$whoami | Format-List
```

## Troubleshooting

### Execution Policy Errors

If you see warnings about execution policy, run PowerShell as Administrator:

```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Module Not Loading

The module is bundled with the plugin. If you see module errors:

1. Check the plugin's `PSModule` directory exists
2. Optionally install system-wide: `Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser`

### Connection Issues

If automatic connection fails:

1. Ensure you're connected to an environment in XrmToolbox
2. Try manual connection (see Connection Management section)
3. Check your credentials and permissions

### Restricted Language Mode

If you see an error about Restricted Language Mode, contact your IT administrator. This is a security policy that prevents PowerShell modules from loading.

## Tips & Tricks

### Save Results to CSV

```powershell
# No -Connection parameter needed
Get-DataverseRecord -TableName account | 
    Export-Csv -Path "accounts.csv" -NoTypeInformation
```

### Work with Lookups

```powershell
# Create record with lookup (by name)
$newContact = @{
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = "Contoso"  # Looks up account by name
}
Set-DataverseRecord -TableName contact -Record $newContact
```

### Batch Operations

```powershell
# The cmdlet automatically batches multiple records
$accounts = @(
    @{ name = "Account 1" },
    @{ name = "Account 2" },
    @{ name = "Account 3" }
)
Set-DataverseRecord -TableName account -Record $accounts
```

### Use SQL for Complex Queries

```powershell
# SQL queries support joins, aggregates, and more
Invoke-DataverseSql -Sql @"
SELECT 
    c.fullname,
    a.name as accountname,
    COUNT(o.opportunityid) as opportunity_count
FROM contact c
LEFT JOIN account a ON c.parentcustomerid = a.accountid
LEFT JOIN opportunity o ON o.parentcontactid = c.contactid
GROUP BY c.fullname, a.name
HAVING COUNT(o.opportunityid) > 0
"@
```

## Keyboard Shortcuts

- **Ctrl+C**: Copy selected text
- **Ctrl+V**: Paste
- **Up/Down arrows**: Command history
- **Tab**: Auto-complete (PowerShell)

## Need More Help?

Visit the [full project documentation](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell) for comprehensive guides, examples, and API reference.

---

**Happy scripting!** 🚀
