$ErrorActionPreference = "Stop"

Describe "RecordAccess E2E Tests" {

    BeforeAll {

        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder
    }

    It "Can grant, test, get, and remove record access comprehensively" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Stop"

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                Write-Host "Connecting to Dataverse..."
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                Write-Host "Getting current user identity..."
                $whoAmI = Get-DataverseWhoAmI -Connection $connection
                $currentUserId = $whoAmI.UserId
                Write-Host "Current user ID: $currentUserId"
                
                # Find or create a second user to share with
                Write-Host "Finding a second user to share with..."
                $allUsers = Get-DataverseRecord -Connection $connection -TableName systemuser -FilterValues @{isdisabled=$false} -Columns systemuserid, fullname
                $secondUser = $allUsers | Where-Object { $_.systemuserid -ne $currentUserId } | Select-Object -First 1
                
                if (-not $secondUser) {
                    throw "Could not find a second user to share with"
                }
                
                $secondUserId = $secondUser.systemuserid
                Write-Host "Second user ID: $secondUserId (Name: $($secondUser.fullname))"
                
                # Create a unique test contact
                $testIdentifier = [Guid]::NewGuid().ToString().Substring(0, 8)
                $testContactData = @{
                    firstname = "E2ETest"
                    lastname = "RecordAccess_$testIdentifier"
                    description = "E2E test for record access"
                }
                
                Write-Host "Creating test contact..."
                $testContact = $testContactData | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
                $contactId = $testContact.contactid
                Write-Host "Created test contact: $contactId"
                
                # Cleanup any existing access from prior failed attempts
                Write-Host "Cleaning up any existing shared access..."
                try {
                    $existingAccess = Get-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -ErrorAction SilentlyContinue
                    if ($existingAccess) {
                        foreach ($access in $existingAccess) {
                            if ($access.Principal.Id -ne $currentUserId) {
                                Write-Host "  Removing existing access for principal: $($access.Principal.Id)"
                                Remove-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $access.Principal.Id -Confirm:$false -ErrorAction SilentlyContinue
                            }
                        }
                    }
                } catch {
                    Write-Host "  No existing access to clean up: $_"
                }
                
                Write-Host "`n=== Testing Set-DataverseRecordAccess (additive) ==="
                Set-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId -AccessRights ([Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -Confirm:$false
                Write-Host "Granted ReadAccess"
                
                Set-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId -AccessRights ([Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -Confirm:$false
                Write-Host "Added WriteAccess (should have Read+Write now)"
                
                Write-Host "`n=== Testing Test-DataverseRecordAccess ==="
                $accessRights = Test-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId
                Write-Host "Access rights: $accessRights"
                
                $hasRead = ($accessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -ne 0
                $hasWrite = ($accessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
                
                if (-not $hasRead) { throw "Expected ReadAccess" }
                if (-not $hasWrite) { throw "Expected WriteAccess" }
                Write-Host "Verified: User has Read and Write (additive worked)"
                
                Write-Host "`n=== Testing Get-DataverseRecordAccess ==="
                $allAccess = Get-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId
                Write-Host "Found $($allAccess.Count) principals with access"
                
                $secondUserAccess = $allAccess | Where-Object { $_.Principal.Id -eq $secondUserId }
                if (-not $secondUserAccess) { throw "Second user not found in access list" }
                Write-Host "Verified: Second user in access list"
                
                Write-Host "`n=== Testing Set-DataverseRecordAccess with -Replace ==="
                Set-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId -AccessRights ([Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess) -Replace -Confirm:$false
                Write-Host "Replaced access with only DeleteAccess"
                
                $replacedAccessRights = Test-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId
                $hasDelete = ($replacedAccessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess) -ne 0
                $hasReadAfter = ($replacedAccessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -ne 0
                $hasWriteAfter = ($replacedAccessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
                
                if (-not $hasDelete) { throw "Expected DeleteAccess" }
                if ($hasReadAfter) { throw "Should not have ReadAccess after Replace" }
                if ($hasWriteAfter) { throw "Should not have WriteAccess after Replace" }
                Write-Host "Verified: Only DeleteAccess (Replace worked)"
                
                Write-Host "`n=== Testing Remove-DataverseRecordAccess ==="
                Remove-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId -Confirm:$false
                Write-Host "Removed access"
                
                $accessAfterRemoval = Test-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $secondUserId
                Write-Host "Access after removal: $accessAfterRemoval"
                
                Write-Host "`n=== Cleanup ==="
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false
                Write-Host "Deleted test contact"
                
                Write-Host "`n=== All tests passed! ==="
                
            } catch {
                $errorDetails = $_ | Format-List -Force * | Out-String
                Write-Host "ERROR: $errorDetails"
                throw "Test failed: " + $_.Exception.Message
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Test failed with exit code $LASTEXITCODE"
        }
    }

    It "Properly cleans up prior failed attempts" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Continue"

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                Write-Host "Connecting for cleanup..."
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                Write-Host "Looking for leftover E2E test contacts..."
                $oldTestContacts = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{firstname='E2ETest'} -Columns contactid, lastname, createdon -ErrorAction Continue
                
                if ($oldTestContacts) {
                    Write-Host "Found $($oldTestContacts.Count) leftover contact(s). Cleaning up..."
                    foreach ($contact in $oldTestContacts) {
                        $contactId = $contact.contactid
                        Write-Host "  Cleaning contact: $contactId"
                        
                        try {
                            $accessList = Get-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -ErrorAction SilentlyContinue
                            
                            if ($accessList) {
                                $whoAmI = Get-DataverseWhoAmI -Connection $connection
                                foreach ($access in $accessList) {
                                    if ($access.Principal.Id -ne $whoAmI.UserId) {
                                        Remove-DataverseRecordAccess -Connection $connection -TableName contact -Id $contactId -Principal $access.Principal.Id -Confirm:$false -ErrorAction SilentlyContinue
                                    }
                                }
                            }
                            
                            Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false -ErrorAction Continue
                            Write-Host "  Cleaned up: $contactId"
                        } catch {
                            Write-Host "  Warning: $contactId : $_"
                        }
                    }
                } else {
                    Write-Host "No leftover contacts found"
                }
                
                Write-Host "`n=== Cleanup test passed ==="
                
            } catch {
                Write-Host "Warning: $_"
            }
        }

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Warning: Cleanup exited with code $LASTEXITCODE"
        }
    }
}
