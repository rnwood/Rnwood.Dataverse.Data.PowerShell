$ErrorActionPreference = "Stop"

Describe "Invoke-DataverseSql with AdditionalConnections" -Skip {

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

    It "Can execute basic SQL query with single connection" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                $results = Invoke-DataverseSql -Connection $connection -Sql "SELECT TOP 5 fullname FROM systemuser"
                
                if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
                    throw "Query returned no results"
                }
                
                Write-Host "Query returned $(($results | Measure-Object).Count) records"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can execute SQL query with AdditionalConnections using same environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Create primary connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create additional connections to the same environment with different names
                # This simulates having multiple named data sources even though they're the same backend
                $connection2 = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                $connection3 = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create hashtable with additional connections
                $additionalConnections = @{
                    "secondary" = $connection2
                    "tertiary" = $connection3
                }
                
                # Execute query that references data sources
                # Since all connections point to the same environment, we'll query from primary
                # but verify the connections are registered by checking that the query executes without error
                $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql "SELECT TOP 3 fullname FROM systemuser"
                
                if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
                    throw "Query returned no results"
                }
                
                Write-Host "Query with additional connections returned $(($results | Measure-Object).Count) records"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can execute cross-datasource query using AdditionalConnections" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Create primary connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create additional connection to the same environment
                # In a real scenario, this would be a different environment, but for testing we use the same one
                $connection2 = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create hashtable with additional connections
                $additionalConnections = @{
                    "secondary" = $connection2
                }
                
                # Get the primary data source name - it's based on the organization unique name
                $orgName = $connection.ConnectedOrgUniqueName
                
                # Execute a cross-datasource query
                # This queries from both the primary and secondary data sources (which happen to be the same environment)
                # SQL4CDS allows referencing other data sources using DataSourceName.TableName syntax
                $sql = "SELECT TOP 2 u1.fullname AS primary_name, u2.fullname AS secondary_name 
                        FROM $orgName.systemuser u1 
                        CROSS JOIN secondary.systemuser u2"
                
                $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql $sql
                
                if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
                    throw "Cross-datasource query returned no results"
                }
                
                # Verify that results have both columns from different data sources
                $firstResult = $results | Select-Object -First 1
                if (-not $firstResult.PSObject.Properties["primary_name"] -or -not $firstResult.PSObject.Properties["secondary_name"]) {
                    throw "Cross-datasource query did not return expected columns"
                }
                
                Write-Host "Cross-datasource query returned $(($results | Measure-Object).Count) records"
                Write-Host "Sample result: primary_name=$($firstResult.primary_name), secondary_name=$($firstResult.secondary_name)"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can execute query selecting from additional data source by name" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Create primary connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create additional connection
                $connection2 = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create hashtable with additional connections
                $additionalConnections = @{
                    "backup" = $connection2
                }
                
                # Query directly from the additional data source
                $sql = "SELECT TOP 3 fullname FROM backup.systemuser"
                
                $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql $sql
                
                if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
                    throw "Query from additional data source returned no results"
                }
                
                Write-Host "Query from additional data source 'backup' returned $(($results | Measure-Object).Count) records"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Throws error when AdditionalConnections contains invalid value type" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Create primary connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create hashtable with invalid connection type
                $additionalConnections = @{
                    "invalid" = "not a connection"
                }
                
                # This should throw an error
                try {
                    Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql "SELECT TOP 1 fullname FROM systemuser"
                    throw "Expected an error when passing invalid connection type, but none was thrown"
                } catch {
                    if ($_.Exception.Message -notlike "*must be a ServiceClient or IOrganizationService instance*") {
                        throw "Expected specific error message about invalid type, got: $($_.Exception.Message)"
                    }
                    Write-Host "Correctly threw error for invalid connection type"
                }
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can use DataSourceName parameter to override default data source name" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Create primary connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create additional connection
                $connection2 = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create hashtable with additional connections
                $additionalConnections = @{
                    "secondary" = $connection2
                }
                
                # Use explicit DataSourceName instead of default org unique name
                $sql = "SELECT TOP 2 u1.fullname AS primary_name, u2.fullname AS secondary_name 
                        FROM primary.systemuser u1 
                        CROSS JOIN secondary.systemuser u2"
                
                $results = Invoke-DataverseSql -Connection $connection -DataSourceName "primary" -AdditionalConnections $additionalConnections -Sql $sql
                
                if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
                    throw "Query with DataSourceName returned no results"
                }
                
                # Verify that results have both columns
                $firstResult = $results | Select-Object -First 1
                if (-not $firstResult.PSObject.Properties["primary_name"] -or -not $firstResult.PSObject.Properties["secondary_name"]) {
                    throw "Query with DataSourceName did not return expected columns"
                }
                
                Write-Host "Query with DataSourceName='primary' returned $(($results | Measure-Object).Count) records"
                Write-Host "Sample result: primary_name=$($firstResult.primary_name), secondary_name=$($firstResult.secondary_name)"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
