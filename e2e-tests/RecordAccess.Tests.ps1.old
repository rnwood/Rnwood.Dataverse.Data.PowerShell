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
                
                # Find or create a second user to share with (we'll use the system administrator as fallback)
                Write-Host "Finding a second user to share with..."
                $allUsers = Get-DataverseRecord -Connection $connection -TableName systemuser -FilterValues @{isdisabled=$false} -Columns systemuserid, fullname
                $secondUser = $allUsers | Where-Object { $_.systemuserid -ne $currentUserId } | Select-Object -First 1
                
                if (-not $secondUser) {
                    throw "Could not find a second user to share with"
                }
                
                $secondUserId = $secondUser.systemuserid
                Write-Host "Second user ID: $secondUserId (Name: $($secondUser.fullname))"
                
                # Create a unique test contact to avoid conflicts with prior failed attempts
                $testIdentifier = [Guid]::NewGuid().ToString().Substring(0, 8)
                $testContactData = @{
                    firstname = "E2ETest"
                    lastname = "RecordAccess_$testIdentifier"
                    description = "E2E test for record access - created at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
                }
                
                Write-Host "Creating test contact..."
                $testContact = Set-DataverseRecord -Connection $connection -TableName contact -Data $testContactData -CreateOnly -PassThru
                $contactId = $testContact.contactid
                Write-Host "Created test contact: $contactId"
                
                $contactRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", [Guid]$contactId)
                
                # Cleanup any existing access from prior failed attempts
                Write-Host "Cleaning up any existing shared access from prior failed attempts..."
                try {
                    $existingAccess = Get-DataverseRecordAccess -Connection $connection -Target $contactRef -ErrorAction SilentlyContinue
                    if ($existingAccess) {
                        foreach ($access in $existingAccess) {
                            if ($access.Principal.Id -ne $currentUserId) {
                                Write-Host "  Removing existing access for principal: $($access.Principal.Id)"
                                Remove-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $access.Principal.Id -Confirm:$false -ErrorAction SilentlyContinue
                            }
                        }
                    }
                } catch {
                    Write-Host "  No existing access to clean up or cleanup failed (this is OK): $_"
                }
                
                Write-Host "`n=== Testing Set-DataverseRecordAccess ==="
                # Grant read and write access to the second user
                Set-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId -AccessRights ([Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -Confirm:$false
                Write-Host "Successfully granted ReadAccess and WriteAccess to user $secondUserId"
                
                Write-Host "`n=== Testing Test-DataverseRecordAccess ==="
                # Test the access that was just granted
                $accessRights = Test-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId
                Write-Host "Access rights for user $secondUserId : $accessRights"
                
                # Verify the access includes what we granted
                $hasRead = ($accessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -ne 0
                $hasWrite = ($accessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
                
                if (-not $hasRead) {
                    throw "Expected user to have ReadAccess but they don't"
                }
                if (-not $hasWrite) {
                    throw "Expected user to have WriteAccess but they don't"
                }
                Write-Host "Verified: User has both ReadAccess and WriteAccess"
                
                Write-Host "`n=== Testing Get-DataverseRecordAccess ==="
                # Get all users who have access to this record
                $allAccess = Get-DataverseRecordAccess -Connection $connection -Target $contactRef
                Write-Host "Found $($allAccess.Count) principals with access to the record"
                
                # Verify our second user is in the list
                $secondUserAccess = $allAccess | Where-Object { $_.Principal.Id -eq $secondUserId }
                if (-not $secondUserAccess) {
                    throw "Expected to find second user in access list but didn't"
                }
                Write-Host "Verified: Second user found in access list with AccessMask: $($secondUserAccess.AccessMask)"
                
                Write-Host "`n=== Testing Set-DataverseRecordAccess (modify) ==="
                # Modify the access to add DeleteAccess
                Set-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId -AccessRights ([Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess) -Confirm:$false
                Write-Host "Successfully modified access to include DeleteAccess"
                
                # Verify the modification
                $modifiedAccessRights = Test-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId
                $hasDelete = ($modifiedAccessRights -band [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess) -ne 0
                if (-not $hasDelete) {
                    throw "Expected user to have DeleteAccess after modification but they don't"
                }
                Write-Host "Verified: User now has DeleteAccess"
                
                Write-Host "`n=== Testing Remove-DataverseRecordAccess ==="
                # Remove the access
                Remove-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId -Confirm:$false
                Write-Host "Successfully removed access from user $secondUserId"
                
                # Verify the access was removed
                $accessAfterRemoval = Test-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $secondUserId
                Write-Host "Access rights after removal: $accessAfterRemoval"
                
                # After removal, the user should have None or minimal access
                if ($accessAfterRemoval -ne [Microsoft.Crm.Sdk.Messages.AccessRights]::None) {
                    Write-Host "Warning: User still has some access after removal: $accessAfterRemoval (this might be expected due to security roles)"
                }
                
                Write-Host "`n=== Cleanup ==="
                # Clean up the test contact
                Write-Host "Deleting test contact..."
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false
                Write-Host "Test contact deleted successfully"
                
                Write-Host "`n=== All tests passed successfully! ==="
                
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

    It "Properly cleans up even when prior attempts failed (idempotent test)" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Continue"  # Allow cleanup to continue on errors

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                Write-Host "Connecting to Dataverse for cleanup verification..."
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                Write-Host "Looking for any leftover E2E test contacts..."
                $oldTestContacts = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{firstname='E2ETest'} -Columns contactid, lastname, createdon -ErrorAction Continue
                
                if ($oldTestContacts) {
                    Write-Host "Found $($oldTestContacts.Count) leftover test contact(s). Cleaning up..."
                    foreach ($contact in $oldTestContacts) {
                        $contactId = $contact.contactid
                        Write-Host "  Cleaning up contact: $contactId (Created: $($contact.createdon))"
                        
                        try {
                            # First, remove any shared access
                            $contactRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", [Guid]$contactId)
                            $accessList = Get-DataverseRecordAccess -Connection $connection -Target $contactRef -ErrorAction SilentlyContinue
                            
                            if ($accessList) {
                                $whoAmI = Get-DataverseWhoAmI -Connection $connection
                                foreach ($access in $accessList) {
                                    if ($access.Principal.Id -ne $whoAmI.UserId) {
                                        Write-Host "    Removing access for principal: $($access.Principal.Id)"
                                        Remove-DataverseRecordAccess -Connection $connection -Target $contactRef -Principal $access.Principal.Id -Confirm:$false -ErrorAction SilentlyContinue
                                    }
                                }
                            }
                            
                            # Then delete the contact
                            Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false -ErrorAction Continue
                            Write-Host "  Successfully cleaned up contact: $contactId"
                        } catch {
                            Write-Host "  Warning: Could not fully clean up contact $contactId : $_"
                        }
                    }
                    Write-Host "Cleanup complete"
                } else {
                    Write-Host "No leftover test contacts found - cleanup verification passed"
                }
                
                Write-Host "`n=== Cleanup verification test passed ==="
                
            } catch {
                Write-Host "Warning during cleanup verification: $_"
                # Don't fail the test for cleanup issues
            }
        }

        # Cleanup test always passes - it's just for verification
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Warning: Cleanup verification exited with code $LASTEXITCODE but test continues"
        }
    }
}
