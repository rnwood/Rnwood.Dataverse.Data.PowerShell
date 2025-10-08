# SDK Test Fix Guide

## Current Status

### Core Tests: ✅ ALL PASSING (103/105)
- Examples.Tests.ps1: 48 passed, 2 skipped
- DefaultConnection.Tests.ps1: 7 passed
- Get-DataverseRecord.Tests.ps1: 48 passed  
- Module.Tests.ps1: 1 passed

### SDK Tests: 7 Fixed, ~98 Remaining
- Total files: 105 (after removing invalid Cancel test)
- Fixed: 7 tests
- Need review: ~98 tests

## Test Failure Patterns & Solutions

### Pattern 1: Interactive Prompt Timeout
**Problem**: Cmdlet has `Mandatory=true` parameter, test doesn't provide it
```powershell
# WRONG - will prompt for Target
$response = Invoke-DataverseBook -Connection $conn -Confirm:$false
```

**Solution**: Provide all mandatory parameters
```powershell
# RIGHT - provides Target and TargetTableName
$target = [PSCustomObject]@{ contactid = [Guid]::NewGuid() }
$response = Invoke-DataverseBook -Connection $conn -Target $target -TargetTableName "contact" -Confirm:$false
```

**How to identify**:
1. Check cmdlet .cs file for `[Parameter(Mandatory = true`
2. Provide ALL mandatory parameters in test

### Pattern 2: Metadata Cache Error
**Problem**: Test uses entity type not in mock metadata cache
```powershell
# WRONG - "appointment" not in metadata
$target = New-Object Microsoft.Xrm.Sdk.EntityReference("appointment", [Guid]::NewGuid())
```
Error: `Entity 'appointment' is not found in the metadata cache`

**Solution**: Use "contact" entity (only one in mock metadata)
```powershell
# RIGHT - using contact entity
$contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
$contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
$contact | Set-DataverseRecord -Connection $conn -CreateOnly

$target = [PSCustomObject]@{ contactid = $contactId }
$response = Invoke-DataverseBook -Connection $conn -Target $target -TargetTableName "contact" -Confirm:$false
```

### Pattern 3: Null Entity Conversion
**Problem**: Cmdlet tries to convert null parameter to Entity
```powershell
# WRONG - OrderClose is null
$response = Invoke-DataverseCancelSalesOrder -Connection $conn -Confirm:$false
```
Error: `NullReferenceException: Object reference not set to an instance of an object`

**Solution**: Provide actual entity data
```powershell
# RIGHT - provides OrderClose entity
$orderClose = [PSCustomObject]@{
    salesorderid = [Guid]::NewGuid()
    actualend = (Get-Date)
    description = "Test cancellation"
}
$status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(4)
$response = Invoke-DataverseCancelSalesOrder -Connection $conn -OrderClose $orderClose -OrderCloseTableName "orderclose" -Status $status -Confirm:$false
```

## Fixed Test Examples

### Example 1: Invoke-DataverseBook.Tests.ps1
```powershell
It "Invoke-DataverseBook executes successfully" {
    $proxy = Get-ProxyService -Connection $script:conn
    $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BookRequest", {
        param($request)
        $response = New-Object Microsoft.Crm.Sdk.Messages.BookResponse
        return $response
    })
    
    # Create contact record (exists in mock metadata)
    $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
    $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
    $contact | Set-DataverseRecord -Connection $script:conn -CreateOnly
    
    # Use PSObject with contactid
    $target = [PSCustomObject]@{ contactid = $contactId }
    
    # Provide both mandatory parameters
    $response = Invoke-DataverseBook -Connection $script:conn -Target $target -TargetTableName "contact" -Confirm:$false
    
    $response | Should -Not -BeNull
    $proxy.LastRequest.GetType().FullName | Should -Match "BookRequest"
}
```

### Example 2: Invoke-DataverseCancelSalesOrder.Tests.ps1
```powershell
It "Invoke-DataverseCancelSalesOrder executes successfully" {
    $proxy = Get-ProxyService -Connection $script:conn
    $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CancelSalesOrderRequest", {
        param($request)
        $response = New-Object Microsoft.Crm.Sdk.Messages.CancelSalesOrderResponse
        return $response
    })
    
    # Provide entity data for conversion
    $orderClose = [PSCustomObject]@{
        salesorderid = [Guid]::NewGuid()
        actualend = (Get-Date)
        description = "Test cancellation"
    }
    $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(4)
    
    $response = Invoke-DataverseCancelSalesOrder -Connection $script:conn -OrderClose $orderClose -OrderCloseTableName "orderclose" -Status $status -Confirm:$false
    
    $response | Should -Not -BeNull
}
```

## Step-by-Step Fix Process

### For Each Failing Test:

1. **Run the test**
   ```powershell
   Invoke-Pester -Path tests/sdk/Invoke-DataverseXxx.Tests.ps1 -Output Detailed
   ```

2. **Identify the error pattern**
   - Interactive prompt → Missing mandatory parameter
   - "not found in metadata cache" → Wrong entity type
   - NullReferenceException → Missing entity data

3. **Check cmdlet signature**
   ```powershell
   # Find the cmdlet file
   ls Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/sdk/InvokeDataverseXxxCmdlet.cs
   
   # Check for mandatory parameters
   grep -A 1 "Mandatory = true" <file>
   ```

4. **Apply the fix**
   - Add missing mandatory parameters
   - Change entity types to "contact"
   - Provide actual entity data instead of null

5. **Verify the fix**
   ```powershell
   Invoke-Pester -Path tests/sdk/Invoke-DataverseXxx.Tests.ps1 -Output Detailed
   ```

## Common Issues & Quick Fixes

### Issue: Cmdlet not found
```
CommandNotFoundException: The term 'Invoke-DataverseXxx' is not recognized
```
**Fix**: Delete the test file - cmdlet doesn't exist

### Issue: ArgumentNullException on dictionary key
```
ArgumentNullException: Value cannot be null. (Parameter 'key')
```
**Fix**: Usually means entity type not in metadata - use "contact" instead

### Issue: Table does not contain column LogicalName
```
ArgumentException: Table contact does not contain a column with the name LogicalName
```
**Fix**: Use proper PSObject structure with entity-specific ID column (e.g., `contactid`)

## Testing Commands

```powershell
# Set up environment
dotnet build -c Release
$env:TESTMODULEPATH = "$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0"

# Test single SDK test
Invoke-Pester -Path tests/sdk/Invoke-DataverseBook.Tests.ps1 -Output Detailed

# Test all SDK tests (long running)
Invoke-Pester -Path tests/sdk/ -Output Detailed

# Test core tests (should all pass)
Invoke-Pester -Path tests/Examples.Tests.ps1,tests/DefaultConnection.Tests.ps1,tests/Get-DataverseRecord.Tests.ps1,tests/Module.Tests.ps1 -Output Normal
```

## Key Takeaways

1. **Always use "contact" entity** - it's the only one in mock metadata
2. **Check for mandatory parameters** - provide ALL of them
3. **Create actual records** when needed - use `Set-DataverseRecord -CreateOnly`
4. **Use PSObject format** for entity parameters when appropriate
5. **Stub responses** for operations not supported by FakeXrmEasy
6. **Test individually** before committing - each test should pass independently
