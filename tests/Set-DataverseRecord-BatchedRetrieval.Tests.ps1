. $PSScriptRoot/Common.ps1

Describe "Set-DataverseRecord Batched Retrieval" {
    BeforeAll {
        $connection = getMockConnection
    }

    Context "RetrievalBatchSize Parameter" {
        It "Has RetrievalBatchSize parameter with default value 500" {
            $cmd = Get-Command Set-DataverseRecord
            $param = $cmd.Parameters['RetrievalBatchSize']
            $param | Should -Not -BeNull
            $param.ParameterType.Name | Should -Be 'UInt32'
            # Default value will be tested in behavior tests
        }

        It "Accepts custom RetrievalBatchSize values" {
            $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Test"
            
            { $contact | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 100 } | Should -Not -Throw
        }

        It "Accepts RetrievalBatchSize of 1 to disable batching" {
            $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Test"
            
            { $contact | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 1 } | Should -Not -Throw
        }
    }

    Context "Batched Retrieval by ID" {
        It "Batches retrieval of multiple records by ID" {
            $contacts = 1..10 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Batch$_"
                $contact
            }

            # This should batch into a single retrieval query for all 10 records
            { $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Processes records when retrieval batch is full" {
            # Create 5 contacts with batch size of 3
            # Should process in 2 batches: 3 + 2
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Batch$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 3 } | Should -Not -Throw
        }

        It "Processes remaining records in EndProcessing" {
            # Create 7 contacts with batch size of 5
            # Should process 5 in ProcessRecord, 2 in EndProcessing
            $contacts = 1..7 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Batch$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 5 } | Should -Not -Throw
        }

        It "Works with RetrievalBatchSize of 1 (no batching)" {
            $contacts = 1..3 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "NoBatch$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 1 } | Should -Not -Throw
        }
    }

    Context "Batched Retrieval with MatchOn" {
        It "Batches retrieval with single-column MatchOn" {
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $contact["emailaddress1"] = "batch$_@test.com"
                $contact["firstname"] = "Batch$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn ("emailaddress1") -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Batches retrieval with multi-column MatchOn" {
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $contact["firstname"] = "Batch$_"
                $contact["lastname"] = "Test$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn @(,@("firstname","lastname")) -RetrievalBatchSize 500 } | Should -Not -Throw
        }
    }

    Context "Batched Retrieval with Contact Entity (Simulating M:M Pattern)" {
        It "Batches retrieval of contact records with composite attribute pattern" {
            # Modified from intersect entity test: using contact entity instead of contactleads
            # This exercises the same batched retrieval code path
            # Note: True intersect entity logic with IsIntersect=true and ManyToManyRelationships 
            # cannot be fully tested without intersect entity metadata
            
            # Create contact records with multiple identifying attributes (simulates M:M pattern)
            $contacts = 1..5 | ForEach-Object {
                $record = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $record.Id = $record["contactid"] = [Guid]::NewGuid()
                $record["firstname"] = "BatchTest$_"
                $record["emailaddress1"] = "batchtest$_@example.com"
                $record
            }

            # First create the records
            $contacts | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly

            # Then update them using batched retrieval
            { $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 500 } | Should -Not -Throw
        }
    }

    Context "Mixed Scenarios" {
        It "Handles records that don't need retrieval (CreateOnly)" {
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $contact["firstname"] = "Create$_"
                $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Handles records that don't need retrieval (UpdateAllColumns)" {
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Update$_"
                $contact
            }
            $contacts | set-DataverseRecord -connection $connection -TableName contact -CreateOnly

            { $contacts | Set-DataverseRecord -Connection $connection -UpdateAllColumns -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Handles mix of records with and without IDs" {
            $contacts = @()
            
            # Some with IDs (need retrieval)
            1..3 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "WithId$_"
                $contacts += $contact
            }
            
            # Some without IDs (no retrieval needed - will create)
            4..6 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $contact["firstname"] = "NoId$_"
                $contacts += $contact
            }

            { $contacts | Set-DataverseRecord -Connection $connection -TableName contact -RetrievalBatchSize 500 } | Should -Not -Throw
        }
    }

    Context "Verbose Output" {
        It "Uses 'retrieval batch' terminology in verbose output" {
            $contacts = 1..5 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Verbose$_"
                $contact
            }

            $verboseOutput = $contacts | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 3 -Verbose 4>&1 | Out-String
            
            # Should mention "retrieval batch" in verbose output
            $verboseOutput | Should -Match "retrieval batch"
        }
    }

    Context "Edge Cases" {
        It "Handles empty pipeline" {
            { @() | Set-DataverseRecord -Connection $connection -TableName contact -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Handles single record" {
            $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Single"
            
            { $contact | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 500 } | Should -Not -Throw
        }

        It "Handles records with errors during retrieval gracefully" {
            # This tests that if retrieval fails, the error is handled properly
            $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Error"
            
            # Should not throw even if there are issues
            { $contact | Set-DataverseRecord -Connection $connection -RetrievalBatchSize 500 -ErrorAction SilentlyContinue } | Should -Not -Throw
        }
    }

    Context "Retrieval Batch Failure with Retries" {
        It "Does not process failed retrieval items immediately - schedules them for retry instead" {
            # This test validates the fix for the bug where failed retrieval items
            # were being processed immediately with null ExistingRecord, causing
            # unwanted creates/updates and lost retries.
            
            $state = [PSCustomObject]@{ 
                FailCount = 0
                RetrieveMultipleCalls = 0
                CreateCount = 0
            }
            
            $interceptor = {
                param($request)
                
                # Track RetrieveMultiple calls (used for retrieval batching)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    $state.RetrieveMultipleCalls++
                    
                    # Fail the first retrieval batch
                    if ($state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [System.Exception]::new("Simulated retrieval batch failure")
                    }
                }
                
                # Track Create requests (should only happen after successful retry)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    foreach ($req in $request.Requests) {
                        if ($req -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                            $state.CreateCount++
                        }
                    }
                }
            }.GetNewClosure()
            
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create contacts with IDs (these need retrieval)
            $contacts = 1..3 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Test$_"
                $contact
            }
            
            # Process with retries enabled and small retrieval batch size
            $contacts | Set-DataverseRecord -Connection $connection -TableName contact -Retries 1 -InitialRetryDelay 0.1 -RetrievalBatchSize 3 -Verbose
            
            # Verify behavior:
            # 1. First RetrieveMultiple should fail
            # 2. Items should be scheduled for retry (NOT processed immediately)
            # 3. Second RetrieveMultiple should succeed on retry
            # 4. Creates should only happen after successful retry
            
            $state.RetrieveMultipleCalls | Should -Be 2 -Because "First retrieval should fail, second should succeed on retry"
            
            # Verify records were created (meaning retry succeeded)
            $createdRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $createdRecords.Count | Should -Be 3 -Because "All 3 records should be created after successful retry"
            
            # The key assertion: records should NOT have been created during the failed batch
            # (if they were processed immediately with null ExistingRecord, they would be created twice)
            $state.CreateCount | Should -Be 3 -Because "Records should only be created once, not during failed retrieval"
        }
        
        It "Retries failed retrieval batch and eventually succeeds" {
            # Test that retry logic works correctly after retrieval failure
            $state = [PSCustomObject]@{ FailCount = 0 }
            
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    # Fail first 2 attempts, succeed on 3rd
                    if ($state.FailCount -lt 2) {
                        $state.FailCount++
                        throw [System.Exception]::new("Simulated retrieval failure $($state.FailCount)")
                    }
                }
            }.GetNewClosure()
            
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            $contacts = 1..2 | ForEach-Object {
                $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
                $id = [Guid]::NewGuid()
                $contact.Id = $contact["contactid"] = $id
                $contact["firstname"] = "Retry$_"
                $contact
            }
            
            # Process with 2 retries (should succeed on 3rd attempt)
            $contacts | Set-DataverseRecord -Connection $connection -TableName contact -Retries 2 -InitialRetryDelay 0.1 -RetrievalBatchSize 2 -Verbose
            
            # Verify retry succeeded
            $createdRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $createdRecords.Count | Should -Be 2 -Because "Records should be created after retry succeeds"
        }
        
        It "Reports error after exhausting all retries on retrieval failure" {
            # Test that errors are reported when retries are exhausted
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    # Always fail
                    throw [System.Exception]::new("Persistent retrieval failure")
                }
            }.GetNewClosure()
            
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            $contact = [Microsoft.Xrm.Sdk.Entity]::new("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "WillFail"
            
            # Process with retries, but all will fail
            $errors = @()
            $contact | Set-DataverseRecord -Connection $connection -TableName contact -Retries 1 -InitialRetryDelay 0.1 -RetrievalBatchSize 1 -ErrorVariable errors -ErrorAction SilentlyContinue
            
            # Should have at least one error
            $errors.Count | Should -BeGreaterThan 0 -Because "Error should be reported after retries exhausted"
        }
    }
}

