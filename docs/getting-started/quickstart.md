# Quick Start Guide

<!-- TOC -->
- [PowerShell Best Practices](#powershell-best-practices)
- [Getting a Connection](#getting-a-connection)
  - [Simple Interactive Connection](#simple-interactive-connection)
  - [Default Connection](#default-connection)
- [Basic Operations](#basic-operations)
  - [Query Records](#query-records)
  - [Create a Record](#create-a-record)
  - [Update a Record](#update-a-record)
  - [Delete a Record](#delete-a-record)
- [Next Steps](#next-steps)
<!-- /TOC -->

## PowerShell Best Practices

> [!IMPORTANT]
> Set `$ErrorActionPreference = "Stop"` at the beginning of your scripts. This turns non-terminating errors into terminating errors so scripts stop immediately on failure:
>
> ```powershell
> # Add this at the start of your scripts
> $ErrorActionPreference = "Stop"
> ```

Without this setting, PowerShell's default behavior is to continue execution after non-terminating errors, which can lead to unexpected results - cascading failures and accidental data corruption.

## Getting a Connection

Get a connection to a target Dataverse environment using the [`Get-DataverseConnection`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) cmdlet (also available as `Connect-DataverseConnection` alias).

Each cmdlet that interacts with Dataverse requires a `-Connection` parameter to specify which environment to use. You typically provide the connection object (e.g., `$c`) returned by [`Get-DataverseConnection`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md).

### Simple Interactive Connection

*Example: Get a connection to MYORG using interactive authentication:*
```powershell
$c = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive
```

*Example: Get a connection by selecting from available environments:*
```powershell
$c = Get-DataverseConnection -interactive
```

### Default Connection

You can set a connection as the default, so you don't have to pass [`-Connection`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md#-connection) to every cmdlet:

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

## Basic Operations

### Query Records

```powershell
# Get all contacts
Get-DataverseRecord -Connection $c -TableName contact

# Get contacts with filtering
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'Smith' }

# Get specific columns
Get-DataverseRecord -Connection $c -TableName contact -Columns firstname,lastname,emailaddress1
```

### Create a Record

```powershell
# Create a single contact
Set-DataverseRecord -Connection $c -TableName contact -InputObject @{ 
    firstname = 'John'
    lastname = 'Doe'
    emailaddress1 = 'john.doe@example.com'
} -CreateOnly

# Create and return the created record (capture Id)
$created = @{ name = 'Contoso Ltd' } | Set-DataverseRecord -Connection $c -TableName account -CreateOnly -PassThru
Write-Host "Created account with Id: $($created.Id)"
```

### Update a Record

```powershell
# Update an existing record by Id
Set-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000' -InputObject @{ 
    description = 'Updated description' 
}

# Upsert using match-on (create if not exists, update if exists)
@{ fullname = 'Jane Smith'; emailaddress1 = 'jane.smith@contoso.com' } |
  Set-DataverseRecord -Connection $c -TableName contact -MatchOn fullname -PassThru
```

### Delete a Record

```powershell
# Delete a single record by Id
Remove-DataverseRecord -Connection $c -TableName contact -Id '00000000-0000-0000-0000-000000000000'

# Delete records returned from a query (prompt for confirmation)
Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ lastname = 'TestUser' } |
  Remove-DataverseRecord -Connection $c -Confirm
```

## Next Steps

- Learn about [authentication methods](authentication.md) for different scenarios
- Explore [connection management](../core-concepts/connections.md) features
- Read about [querying records](../core-concepts/querying.md) in detail
- Understand [creating and updating records](../core-concepts/creating-updating.md)
- Check the [full cmdlet documentation](../../Rnwood.Dataverse.Data.PowerShell/docs/)
