# Quick Reference: Using Minimal Metadata in Tests

This guide shows how to use the new minimal metadata generation feature for testing.

## Basic Usage

### Creating a Mock Connection with Additional Entities

```powershell
# Create a mock connection with contact entity (from contact.xml) plus solution entity
$connection = getMockConnection -AdditionalEntities @("solution")

# Create with multiple entities
$connection = getMockConnection -AdditionalEntities @("solution", "systemuser", "workflow")
```

## Common Patterns

### Pattern 1: Query Validation
Test that a query pattern works without requiring data:

```powershell
It "Can query for solutions" {
    $connection = getMockConnection -AdditionalEntities @("solution")
    
    # This validates the query works (returns empty result, doesn't throw)
    { Get-DataverseRecord -Connection $connection -TableName solution } | Should -Not -Throw
}
```

### Pattern 2: Cmdlet Parameter Validation
Verify a specialized cmdlet exists and has expected parameters:

```powershell
It "Can use specialized cmdlet" {
    $connection = getMockConnection -AdditionalEntities @("list")
    
    $cmd = Get-Command Invoke-DataverseAddMemberList -ErrorAction SilentlyContinue
    $cmd | Should -Not -BeNull
    $cmd.Parameters.ContainsKey("ListId") | Should -Be $true
}
```

### Pattern 3: Full CRUD with contact.xml
For entities with full metadata, do complete testing:

```powershell
It "Can create and retrieve contact" {
    # contact.xml is automatically loaded by getMockConnection
    $connection = getMockConnection
    
    $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
    $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
    $contact["firstname"] = "Test"
    $contact | Set-DataverseRecord -Connection $connection
    
    $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId
    $retrieved.firstname | Should -Be "Test"
}
```

## When to Use What

### Use Minimal Metadata When:
- ✅ Testing query patterns
- ✅ Validating cmdlet existence
- ✅ Checking parameter availability
- ✅ Testing connection initialization
- ✅ You don't need to create/modify records

### Use Full Metadata When:
- ✅ Testing CRUD operations with data
- ✅ Testing complex relationships
- ✅ Testing option sets / choice fields
- ✅ Testing lookup fields
- ✅ You need comprehensive attribute information

### Generate Full Metadata:
```powershell
# 1. Connect to real Dataverse
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# 2. Run metadata generator
./tests/generate-all-metadata.ps1 -Connection $conn

# 3. XML files created in tests/ directory
# 4. getMockConnection automatically loads them
```

## Entities Available

### With Full Metadata (from XML files):
- `contact` (always available - 2.2MB file in repo)

### With Minimal Metadata (on-demand):
- `solution`
- `systemuser`
- `workflow`
- `asyncoperation`
- `organization`
- `processstage`
- `savedquery`
- `userquery`
- `list`
- `duplicaterule`
- Any other entity you specify in `AdditionalEntities`

## Examples

### Test Multiple Entities
```powershell
It "Can work with multiple entities" {
    $connection = getMockConnection -AdditionalEntities @(
        "solution",
        "systemuser",
        "team"
    )
    
    { Get-DataverseRecord -Connection $connection -TableName solution } | Should -Not -Throw
    { Get-DataverseRecord -Connection $connection -TableName systemuser } | Should -Not -Throw
    { Get-DataverseRecord -Connection $connection -TableName team } | Should -Not -Throw
}
```

### Mix Full and Minimal Metadata
```powershell
It "Can use both contact and solution" {
    # contact.xml loaded automatically, solution added with minimal metadata
    $connection = getMockConnection -AdditionalEntities @("solution")
    
    # Full CRUD works with contact (has full metadata)
    $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
    $contact.Id = [Guid]::NewGuid()
    $contact["firstname"] = "Test"
    $contact | Set-DataverseRecord -Connection $connection
    
    # Query pattern works with solution (minimal metadata)
    { Get-DataverseRecord -Connection $connection -TableName solution } | Should -Not -Throw
}
```

## Troubleshooting

### Error: "Nullable object must have a value"
**Problem**: Trying to create records with minimal metadata
**Solution**: Use full metadata or just validate query pattern

```powershell
# ❌ This fails with minimal metadata
$solution = New-Object Microsoft.Xrm.Sdk.Entity("solution")
$solution | Set-DataverseRecord -Connection $connection

# ✅ This works - validates pattern only
{ Get-DataverseRecord -Connection $connection -TableName solution } | Should -Not -Throw
```

### Error: "Entity not found"
**Problem**: Entity not included in metadata
**Solution**: Add to AdditionalEntities parameter

```powershell
# ❌ This fails if 'account' not in metadata
Get-DataverseRecord -Connection $connection -TableName account

# ✅ This works
$connection = getMockConnection -AdditionalEntities @("account")
Get-DataverseRecord -Connection $connection -TableName account
```

## See Also

- **tests/README.md** - Complete test infrastructure documentation
- **tests/SOLUTION_SUMMARY.md** - Overview of changes and results
- **tests/Common.ps1** - Implementation details
- **tests/generate-all-metadata.ps1** - Full metadata generation script
