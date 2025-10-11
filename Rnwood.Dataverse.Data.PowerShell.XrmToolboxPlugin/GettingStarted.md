# Getting Started with PowerShell Console Plugin

Welcome to the PowerShell Console plugin for XrmToolbox! This plugin provides a powerful PowerShell console directly within XrmToolbox, pre-loaded with the Rnwood.Dataverse.Data.PowerShell module and automatically connected to your current Dataverse environment.

## Quick Start

### 1. Opening the Console

The PowerShell console is embedded directly in this XrmToolbox tab. When you open the plugin, it automatically:

- ✅ Loads the PowerShell module (bundled with the plugin)
- ✅ Connects to your current Dataverse environment
- ✅ Creates a `$connection` variable you can use immediately

### 2. Your First Command

Try querying some records:

```powershell
Get-DataverseRecord -Connection $connection -TableName account
```

### 3. Common Operations

#### Query Records
```powershell
# Get all accounts
Get-DataverseRecord -Connection $connection -TableName account

# Get specific columns
Get-DataverseRecord -Connection $connection -TableName contact -Columns name, emailaddress1

# Filter records
Get-DataverseRecord -Connection $connection -TableName account -Filter "name eq 'Contoso'"
```

#### Create Records
```powershell
$newAccount = @{ 
    name = "Contoso Ltd" 
    telephone1 = "555-1234"
}
Set-DataverseRecord -Connection $connection -TableName account -Record $newAccount
```

#### Update Records
```powershell
$update = @{ 
    accountid = "guid-here"
    telephone1 = "555-5678"
}
Set-DataverseRecord -Connection $connection -TableName account -Record $update
```

#### Delete Records
```powershell
Remove-DataverseRecord -Connection $connection -TableName account -Id "guid-here"
```

#### SQL Queries
```powershell
Invoke-DataverseSql -Connection $connection -Sql @"
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

The plugin automatically connects using your XrmToolbox credentials. The connection is available in the `$connection` variable.

### Manual Connection (if needed)

If automatic connection fails or you need to connect to a different environment:

```powershell
# Interactive authentication
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# Client secret authentication
$connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" `
    -ClientId "your-client-id" `
    -ClientSecret "your-client-secret"
```

### Check Current Connection

```powershell
$whoami = Get-DataverseWhoAmI -Connection $connection
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
Get-DataverseRecord -Connection $connection -TableName account | 
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
Set-DataverseRecord -Connection $connection -TableName contact -Record $newContact
```

### Batch Operations

```powershell
# The cmdlet automatically batches multiple records
$accounts = @(
    @{ name = "Account 1" },
    @{ name = "Account 2" },
    @{ name = "Account 3" }
)
Set-DataverseRecord -Connection $connection -TableName account -Record $accounts
```

### Use SQL for Complex Queries

```powershell
# SQL queries support joins, aggregates, and more
Invoke-DataverseSql -Connection $connection -Sql @"
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
