Describe "Remove-DataverseRecord" {
    BeforeAll {
        $modulePath = $env:TESTMODULEPATH
        if (-not $modulePath) {
            throw "TESTMODULEPATH environment variable not set"
        }
        $script:tempModulePath = Copy-ModuleToTemp -ModulePath $modulePath
        Add-Type -Path (Join-Path $modulePath "cmdlets\net6.0\Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll")
        Import-Module (Join-Path $script:tempModulePath "Rnwood.Dataverse.Data.PowerShell.psd1")

        $metadata = $null;
        if (-not $metadata) {
            Add-Type -AssemblyName "System.Runtime.Serialization"
            $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
            get-item $PSScriptRoot/*.xml | foreach-object {
                $stream = [IO.File]::OpenRead($_.FullName)
                $metadata += $serializer.ReadObject($stream)
                $stream.Close();
            }
        }

        function getMockConnection($failNextExecuteMultiple = $false, $failExecuteMultipleIndices = @(), $failExecuteMultipleTimes = 0) {
            $mockService = get-dataverseconnection -url https://fake.crm.dynamics.com/ -mock $metadata
            $innerService = $mockService.OrganizationService
            
            if ($failNextExecuteMultiple -or $failExecuteMultipleIndices.Count -gt 0 -or $failExecuteMultipleTimes -gt 0) {
                $type = [Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithFailures]
                $constructor = $type.GetConstructor([Microsoft.Xrm.Sdk.IOrganizationService])
                $wrapper = $constructor.Invoke(@($innerService))
                $wrapper.FailNextExecuteMultiple = $failNextExecuteMultiple
                foreach ($index in $failExecuteMultipleIndices) {
                    $wrapper.FailExecuteMultipleIndices.Add($index)
                }
                $wrapper.FailExecuteMultipleTimes = $failExecuteMultipleTimes
                $service = New-Object Microsoft.PowerPlatform.Dataverse.Client.ServiceClient -ArgumentList $wrapper
            } else {
                $service = $mockService
            }
            
            return $service
        }
    }

    Context "Basic Removal" {
        It "Removes a single record" {
            $connection = getMockConnection
            $record = @{ firstname = "Test"; lastname = "User" }
            $created = $record | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Id | Should -Not -BeNullOrEmpty

            $created | Remove-DataverseRecord -Connection $connection -TableName contact

            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $created.Id
            $retrieved | Should -BeNullOrEmpty
        }

        It "Removes multiple records in batch" {
            $connection = getMockConnection
            $records = @(
                @{ firstname = "Test1"; lastname = "User1" },
                @{ firstname = "Test2"; lastname = "User2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Count | Should -Be 2

            $created | Remove-DataverseRecord -Connection $connection -TableName contact

            $remaining = Get-DataverseRecord -Connection $connection -TableName contact
            $remaining.Count | Should -Be 0
        }
    }

    Context "Retries" {
        It "Retries whole batch on ExecuteMultiple failure" {
            $connection = getMockConnection -failNextExecuteMultiple $true
            
            # Create records first
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $ids = $created | Select-Object -ExpandProperty Id

            # Now remove with failure
            $ids | ForEach-Object { @{ Id = $_; TableName = "contact" } } | Remove-DataverseRecord -Connection $connection -Retries 1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact
            $remaining.Count | Should -Be 0
        }

        It "Retries individual failed items in batch" {
            $connection = getMockConnection -failExecuteMultipleIndices @(0)
            
            # Create records first
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $ids = $created | Select-Object -ExpandProperty Id

            # Now remove with failure on first item
            $ids | ForEach-Object { @{ Id = $_; TableName = "contact" } } | Remove-DataverseRecord -Connection $connection -Retries 1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact
            $remaining.Count | Should -Be 0
        }

        It "Emits errors for all records when batch retries are exceeded" {
            $connection = getMockConnection -failExecuteMultipleTimes 3
            
            # Create records first
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $ids = $created | Select-Object -ExpandProperty Id

            # Now remove with failure exceeding retries
            $errors = @()
            $ids | ForEach-Object { @{ Id = $_; TableName = "contact" } } | Remove-DataverseRecord -Connection $connection -Retries 1 -ErrorVariable +errors -ErrorAction SilentlyContinue

            $errors.Count | Should -Be 2

            # Verify records are still there
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact
            $remaining.Count | Should -Be 2
        }
    }
}}}}}}}}