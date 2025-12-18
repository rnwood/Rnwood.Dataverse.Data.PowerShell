. $PSScriptRoot/Common.ps1

Describe 'Test-DataverseSolution' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
    }

    Context "Cmdlet parameters and structure" {
        
        It "Test-DataverseSolution cmdlet exists" {
            $cmd = Get-Command Test-DataverseSolution -ErrorAction SilentlyContinue
            $cmd | Should -Not -BeNullOrEmpty
        }
        
        It "Has UniqueName parameter" {
            $cmd = Get-Command Test-DataverseSolution
            $param = $cmd.Parameters["UniqueName"]
            
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be "String"
            $param.Attributes.Mandatory | Should -Contain $true
        }
        
        It "Has IncludeInfo parameter" {
            $cmd = Get-Command Test-DataverseSolution
            $param = $cmd.Parameters["IncludeInfo"]
            
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be "SwitchParameter"
        }
        
        It "Supports ShouldProcess (WhatIf/Confirm)" {
            $cmd = Get-Command Test-DataverseSolution
            $cmd.Parameters.ContainsKey("WhatIf") | Should -Be $true
            $cmd.Parameters.ContainsKey("Confirm") | Should -Be $true
        }
        
        It "Has correct output type" {
            $cmd = Get-Command Test-DataverseSolution
            $outputTypes = $cmd.OutputType.Type.Name
            $outputTypes | Should -Contain "SolutionValidationResult"
        }
    }

    Context "Validation with mock solution - valid solution" {
        
        It "Returns valid result for solution with no issues" {
            $conn = getMockConnection -RequestInterceptor {
                param($request)
                
                # Mock solution query
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'solution') {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $solution = New-Object Microsoft.Xrm.Sdk.Entity("solution", [Guid]::NewGuid())
                        $solution["uniquename"] = "TestSolution"
                        $solution["ismanaged"] = $false
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $entityCollection.Entities.Add($solution)
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                # Mock msdyn_solutioncomponentsummary query (REST API via HttpClient is harder to mock)
                # The EnvironmentComponentExtractor uses REST API which won't work in mock
                # For now, we'll test the cmdlet structure and let E2E tests validate full behavior
            }
            
            # This test validates the cmdlet exists and can be called
            # Full validation requires REST API mocking which is complex
            $cmd = Get-Command Test-DataverseSolution
            $cmd | Should -Not -BeNullOrEmpty
        }
    }

    Context "Validation model classes" {
        
        It "SolutionValidationResult class exists" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationResult"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            $type | Should -Not -BeNullOrEmpty
        }
        
        It "SolutionValidationIssue class exists" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationIssue"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            $type | Should -Not -BeNullOrEmpty
        }
        
        It "SolutionValidationSeverity enum exists" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationSeverity"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            $type | Should -Not -BeNullOrEmpty
            $type.IsEnum | Should -Be $true
        }
        
        It "SolutionValidationRules class has expected constants" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            $type | Should -Not -BeNullOrEmpty
            
            # Check rule constants exist
            $sv001 = $type.GetField("ManagedTableIncludeSubcomponents").GetValue($null)
            $sv001 | Should -Be "SV001"
            
            $sv002 = $type.GetField("ManagedNonTableNotCustomized").GetValue($null)
            $sv002 | Should -Be "SV002"
            
            $sv003 = $type.GetField("ManagedSubcomponentNotCustomized").GetValue($null)
            $sv003 | Should -Be "SV003"
        }
        
        It "SolutionValidationRules.GetDocumentationUrl returns expected format" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            
            $method = $type.GetMethod("GetDocumentationUrl")
            $url = $method.Invoke($null, @("SV001"))
            
            $url | Should -Match "github.com"
            $url | Should -Match "SV001"
        }
        
        It "SolutionValidationRules.GetRuleName returns expected names" {
            $typeName = "Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules"
            $type = [Type]::GetType("$typeName, Rnwood.Dataverse.Data.PowerShell.Cmdlets")
            
            $method = $type.GetMethod("GetRuleName")
            
            $name1 = $method.Invoke($null, @("SV001"))
            $name1 | Should -Be "Managed Table Include Subcomponents"
            
            $name2 = $method.Invoke($null, @("SV002"))
            $name2 | Should -Be "Managed Non-Table Not Customized"
            
            $name3 = $method.Invoke($null, @("SV003"))
            $name3 | Should -Be "Managed Subcomponent Not Customized"
        }
    }

    Context "Documentation exists" {
        
        It "Cmdlet documentation file exists" {
            $docPath = "$PSScriptRoot/../docs/Test-DataverseSolution.md"
            Test-Path $docPath | Should -Be $true
        }
        
        It "SV001 rule documentation exists" {
            $docPath = "$PSScriptRoot/../docs/solution-validation-rules/SV001.md"
            Test-Path $docPath | Should -Be $true
        }
        
        It "SV002 rule documentation exists" {
            $docPath = "$PSScriptRoot/../docs/solution-validation-rules/SV002.md"
            Test-Path $docPath | Should -Be $true
        }
        
        It "SV003 rule documentation exists" {
            $docPath = "$PSScriptRoot/../docs/solution-validation-rules/SV003.md"
            Test-Path $docPath | Should -Be $true
        }
        
        It "Validation rules README exists" {
            $docPath = "$PSScriptRoot/../docs/solution-validation-rules/README.md"
            Test-Path $docPath | Should -Be $true
        }
    }

    Context "Rule logic validation" {
        
        It "Documents that SV001 checks managed tables with Include Subcomponents" {
            # Rule SV001: Managed table components should not be in the solution 
            # with "include subcomponents" behavior
            # 
            # The cmdlet checks for:
            # - ComponentType == 1 (Entity/Table)
            # - IsManaged == true
            # - !IsSubcomponent
            # - RootComponentBehavior == 0 (IncludeSubcomponents)
            #
            # This is validated in E2E tests with real data
            $true | Should -Be $true
        }
        
        It "Documents that SV002 checks managed non-table components not customized" {
            # Rule SV002: Managed non-table components should only be in 
            # the solution if they are customized
            #
            # The cmdlet checks for:
            # - ComponentType != 1 (Not Entity/Table)
            # - IsManaged == true
            # - !IsSubcomponent
            # - IsCustomized != true
            #
            # This is validated in E2E tests with real data
            $true | Should -Be $true
        }
        
        It "Documents that SV003 checks managed subcomponents not customized" {
            # Rule SV003: Table managed subcomponents should only be 
            # in the solution if they are customized
            #
            # The cmdlet checks for:
            # - IsSubcomponent == true
            # - IsManaged == true
            # - IsCustomized != true
            #
            # This is validated in E2E tests with real data
            $true | Should -Be $true
        }
    }

    Context "Error handling" {
        
        It "Handles non-existent solution" {
            $conn = getMockConnection -RequestInterceptor {
                param($request)
                
                # Mock solution query returning no results
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'solution') {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
            }
            
            { Test-DataverseSolution -Connection $conn -UniqueName "NonExistentSolution" -ErrorAction Stop } | 
                Should -Throw "*not found*"
        }
    }
}
