. $PSScriptRoot/Common.ps1

Describe 'Solution Cmdlets' {

    BeforeAll {
        # Create a minimal test solution.xml content
        $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>TestSolution</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Descriptions>
      <Description description="This is a test solution" languagecode="1033" />
    </Descriptions>
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>testpublisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description="Test Publisher" languagecode="1033" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
  </SolutionManifest>
</ImportExportXml>
"@

        # Create a minimal test solution zip file
        $tempDir = [IO.Path]::GetTempPath()
        $testSolutionPath = Join-Path $tempDir "TestSolution.zip"
        
        # Create zip with solution.xml
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        if (Test-Path $testSolutionPath) {
            Remove-Item $testSolutionPath -Force
        }
        
        $zip = [System.IO.Compression.ZipFile]::Open($testSolutionPath, [System.IO.Compression.ZipArchiveMode]::Create)
        $entry = $zip.CreateEntry("solution.xml")
        $writer = New-Object System.IO.StreamWriter($entry.Open())
        $writer.Write($solutionXml)
        $writer.Close()
        $zip.Dispose()
        
        Set-Variable -Name "TestSolutionPath" -Value $testSolutionPath -Scope Global
    }

    AfterAll {
        if (Test-Path $Global:TestSolutionPath) {
            Remove-Item $Global:TestSolutionPath -Force -ErrorAction SilentlyContinue
        }
    }

    Context 'Get-DataverseSolutionFile' {
        It "Parses solution file and returns metadata" {
            $result = Get-DataverseSolutionFile -Path $Global:TestSolutionPath
            
            $result | Should -Not -BeNullOrEmpty
            $result.UniqueName | Should -Be "TestSolution"
            $result.Name | Should -Be "Test Solution"
            $result.Description | Should -Be "This is a test solution"
            $result.Version | Should -Be ([Version]"1.0.0.0")
            $result.IsManaged | Should -Be $false
            $result.PublisherName | Should -Be "Test Publisher"
            $result.PublisherUniqueName | Should -Be "testpublisher"
            $result.PublisherPrefix | Should -Be "test"
        }

        It "Parses solution file from bytes" {
            $bytes = [System.IO.File]::ReadAllBytes($Global:TestSolutionPath)
            $result = $bytes | Get-DataverseSolutionFile
            
            $result | Should -Not -BeNullOrEmpty
            $result.UniqueName | Should -Be "TestSolution"
            $result.Name | Should -Be "Test Solution"
        }
    }

    Context 'Get-DataverseSolution' {
        It "Retrieves all solutions from environment" {
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Create mock response for RetrieveMultipleRequest
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    $solution1 = New-Object Microsoft.Xrm.Sdk.Entity "solution"
                    $solution1.Id = [Guid]::NewGuid()
                    $solution1["solutionid"] = $solution1.Id
                    $solution1["uniquename"] = "TestSolution1"
                    $solution1["friendlyname"] = "Test Solution 1"
                    $solution1["version"] = "1.0.0.0"
                    $solution1["ismanaged"] = $false
                    $solution1["description"] = "Test solution 1"
                    
                    $solution2 = New-Object Microsoft.Xrm.Sdk.Entity "solution"
                    $solution2.Id = [Guid]::NewGuid()
                    $solution2["solutionid"] = $solution2.Id
                    $solution2["uniquename"] = "TestSolution2"
                    $solution2["friendlyname"] = "Test Solution 2"
                    $solution2["version"] = "2.0.0.0"
                    $solution2["ismanaged"] = $true
                    $solution2["description"] = "Test solution 2"
                    
                    $collection.Entities.Add($solution1)
                    $collection.Entities.Add($solution2)
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                return $null
            }
            
            $result = Get-DataverseSolution -Connection $connection
            
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -Be 2
        }

        It "Filters by unique name" {
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Create mock response for RetrieveMultipleRequest
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    $solution = New-Object Microsoft.Xrm.Sdk.Entity "solution"
                    $solution.Id = [Guid]::NewGuid()
                    $solution["solutionid"] = $solution.Id
                    $solution["uniquename"] = "UniqueTestSolution"
                    $solution["friendlyname"] = "Unique Test Solution"
                    $solution["version"] = "1.0.0.0"
                    $solution["ismanaged"] = $false
                    
                    $collection.Entities.Add($solution)
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                return $null
            }
            
            $result = Get-DataverseSolution -Connection $connection -UniqueName "UniqueTestSolution"
            
            $result | Should -Not -BeNullOrEmpty
            $result.UniqueName | Should -Be "UniqueTestSolution"
        }
    }

    Context 'Publish-DataverseCustomizations' {
        It "Publishes all customizations" {
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Mock PublishAllXmlRequest
                if ($request -is [Microsoft.Crm.Sdk.Messages.PublishAllXmlRequest]) {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.PublishAllXmlResponse
                    return $response
                }
                
                return $null
            }
            
            $result = Publish-DataverseCustomizations -Connection $connection -Confirm:$false
            
            $result | Should -Be "Customizations published successfully."
        }

        It "Publishes specific entity customizations" {
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Mock PublishXmlRequest
                if ($request -is [Microsoft.Crm.Sdk.Messages.PublishXmlRequest]) {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.PublishXmlResponse
                    return $response
                }
                
                return $null
            }
            
            $result = Publish-DataverseCustomizations -Connection $connection -EntityName "contact" -Confirm:$false
            
            $result | Should -Be "Customizations published successfully."
        }
    }

    Context 'Set-DataverseSolution' {
        It "Updates solution properties" {
            $solutionId = [Guid]::NewGuid()
            $updated = $false
            
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Mock RetrieveMultipleRequest for finding solution
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    $solution = New-Object Microsoft.Xrm.Sdk.Entity "solution"
                    $solution.Id = $solutionId
                    $solution["solutionid"] = $solutionId
                    $solution["uniquename"] = "UpdateTestSolution"
                    $solution["friendlyname"] = if ($updated) { "Updated Name" } else { "Original Name" }
                    $solution["version"] = "1.0.0.0"
                    $solution["ismanaged"] = $false
                    $solution["description"] = if ($updated) { "Updated description" } else { "Original description" }
                    
                    $collection.Entities.Add($solution)
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                # Mock UpdateRequest
                if ($request -is [Microsoft.Xrm.Sdk.Messages.UpdateRequest]) {
                    $script:updated = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                    return $response
                }
                
                return $null
            }
            
            $result = Set-DataverseSolution -Connection $connection -UniqueName "UpdateTestSolution" -Name "Updated Name" -Description "Updated description" -Confirm:$false
            
            $result | Should -Be "Solution 'UpdateTestSolution' updated successfully."
        }

        It "Creates a new solution if not found" {
            $created = $false
            $publisherId = [Guid]::NewGuid()
            
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Mock RetrieveMultipleRequest for finding solution (return empty)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest] -and $request.Query.EntityName -eq "solution") {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    # Empty collection
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                # Mock RetrieveMultipleRequest for finding publisher
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest] -and $request.Query.EntityName -eq "publisher") {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    $publisher = New-Object Microsoft.Xrm.Sdk.Entity "publisher"
                    $publisher.Id = $publisherId
                    $publisher["publisherid"] = $publisherId
                    $publisher["uniquename"] = "TestPublisher"
                    
                    $collection.Entities.Add($publisher)
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                # Mock CreateRequest
                if ($request -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                    $script:created = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateResponse
                    $response.Results["id"] = [Guid]::NewGuid()
                    return $response
                }
                
                return $null
            }
            
            $result = Set-DataverseSolution -Connection $connection -UniqueName "NewTestSolution" -Name "New Solution" -Description "New description" -Version "2.0.0.0" -PublisherUniqueName "TestPublisher" -Confirm:$false
            
            $result | Should -Be "Solution 'NewTestSolution' created successfully."
        }
    }

    Context 'Remove-DataverseSolution' {
        It "Removes a solution" {
            $solutionId = [Guid]::NewGuid()
            $deleted = $false
            
            $connection = getMockConnection -RequestInterceptor {
                param($service, $request)
                
                # Mock RetrieveMultipleRequest for finding solution
                if ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    if (-not $deleted) {
                        $solution = New-Object Microsoft.Xrm.Sdk.Entity "solution"
                        $solution.Id = $solutionId
                        $solution["solutionid"] = $solutionId
                        $solution["uniquename"] = "DeleteTestSolution"
                        $solution["friendlyname"] = "Delete Test Solution"
                        $solution["version"] = "1.0.0.0"
                        $solution["ismanaged"] = $false
                        
                        $collection.Entities.Add($solution)
                    }
                    
                    $response.Results["EntityCollection"] = $collection
                    return $response
                }
                
                # Mock DeleteRequest
                if ($request -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest]) {
                    $script:deleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteResponse
                    return $response
                }
                
                return $null
            }
            
            $result = Remove-DataverseSolution -Connection $connection -UniqueName "DeleteTestSolution" -Confirm:$false
            
            $result | Should -Match "removed successfully"
        }
    }
}
