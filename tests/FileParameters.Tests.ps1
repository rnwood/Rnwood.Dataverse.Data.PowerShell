. $PSScriptRoot/Common.ps1

Describe "SDK Cmdlets File Parameters" {

    Context "Import cmdlets with InFile parameter" {
        
        It "Invoke-DataverseImportSolution should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportSolution
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $inFileParam = $cmd.Parameters['InFile']
            $inFileParam.ParameterType | Should -Be ([string])
            
            # Check parameter sets
            $parameterSets = $inFileParam.ParameterSets.Keys
            $parameterSets | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseImportSolutionAsync should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportSolutionAsync
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseStageSolution should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseStageSolution
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseStageAndUpgrade should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseStageAndUpgrade
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseStageAndUpgradeAsync should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseStageAndUpgradeAsync
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseRetrieveMissingComponents should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseRetrieveMissingComponents
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseImportTranslation should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportTranslation
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseImportTranslationAsync should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportTranslationAsync
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseImportFieldTranslation should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportFieldTranslation
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseUploadBlock should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseUploadBlock
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "Invoke-DataverseImportSolutions should have InFile parameter" {
            $cmd = Get-Command Invoke-DataverseImportSolutions
            $cmd.Parameters.ContainsKey('InFile') | Should -Be $true
            $cmd.Parameters['InFile'].ParameterSets.Keys | Should -Contain 'FromFile'
        }
        
        It "InFile parameter should be in separate parameter set from byte[] parameter" {
            $cmd = Get-Command Invoke-DataverseImportSolution
            $customizationFileParam = $cmd.Parameters['CustomizationFile']
            $inFileParam = $cmd.Parameters['InFile']
            
            # CustomizationFile should be in Default parameter set
            $customizationFileParam.ParameterSets.Keys | Should -Contain 'Default'
            
            # InFile should be in FromFile parameter set
            $inFileParam.ParameterSets.Keys | Should -Contain 'FromFile'
            
            # They should not share the same parameter set (except common parameters)
            $customizationFileParam.ParameterSets.Keys | Should -Not -Contain 'FromFile'
            $inFileParam.ParameterSets.Keys | Should -Not -Contain 'Default'
        }
        
        It "InFile parameter should reject non-existent files" {
            $testDir = Join-Path $env:TEMP ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $testDir -Force | Out-Null
            
            try {
                $nonExistentFile = Join-Path $testDir "non-existent.zip"
                
                { 
                    Invoke-DataverseImportSolution -InFile $nonExistentFile -Connection (getMockConnection) -OverwriteUnmanagedCustomizations $false -PublishWorkflows $false
                } | Should -Throw "*does not exist*"
            } finally {
                if (Test-Path $testDir) {
                    Remove-Item -Path $testDir -Recurse -Force
                }
            }
        }
    }

    Context "Export cmdlets with OutFile parameter" {
        
        It "Invoke-DataverseExportSolution should have OutFile parameter" {
            $cmd = Get-Command Invoke-DataverseExportSolution
            $cmd.Parameters.ContainsKey('OutFile') | Should -Be $true
            $outFileParam = $cmd.Parameters['OutFile']
            $outFileParam.ParameterType | Should -Be ([string])
            
            # Check parameter sets
            $parameterSets = $outFileParam.ParameterSets.Keys
            $parameterSets | Should -Contain 'ToFile'
        }
        
        It "Invoke-DataverseExportTranslation should have OutFile parameter" {
            $cmd = Get-Command Invoke-DataverseExportTranslation
            $cmd.Parameters.ContainsKey('OutFile') | Should -Be $true
            $cmd.Parameters['OutFile'].ParameterSets.Keys | Should -Contain 'ToFile'
        }
        
        It "Invoke-DataverseExportFieldTranslation should have OutFile parameter" {
            $cmd = Get-Command Invoke-DataverseExportFieldTranslation
            $cmd.Parameters.ContainsKey('OutFile') | Should -Be $true
            $cmd.Parameters['OutFile'].ParameterSets.Keys | Should -Contain 'ToFile'
        }
        
        It "OutFile parameter should be in separate parameter set" {
            $cmd = Get-Command Invoke-DataverseExportSolution
            $outFileParam = $cmd.Parameters['OutFile']
            
            # OutFile should be in ToFile parameter set
            $outFileParam.ParameterSets.Keys | Should -Contain 'ToFile'
            
            # It should not be in Default parameter set
            $outFileParam.ParameterSets.Keys | Should -Not -Contain 'Default'
        }
    }

    Context "Parameter validation" {
        
        It "InFile should be mandatory in FromFile parameter set" {
            $cmd = Get-Command Invoke-DataverseImportSolution
            $inFileParam = $cmd.Parameters['InFile']
            $fromFileSet = $inFileParam.ParameterSets['FromFile']
            $fromFileSet.IsMandatory | Should -Be $true
        }
        
        It "OutFile should be mandatory in ToFile parameter set" {
            $cmd = Get-Command Invoke-DataverseExportSolution
            $outFileParam = $cmd.Parameters['OutFile']
            $toFileSet = $outFileParam.ParameterSets['ToFile']
            $toFileSet.IsMandatory | Should -Be $true
        }
        
        It "CustomizationFile should not be mandatory when InFile parameter set is used" {
            $cmd = Get-Command Invoke-DataverseImportSolution
            $customizationFileParam = $cmd.Parameters['CustomizationFile']
            
            # In Default parameter set, CustomizationFile might be mandatory or not depending on the cmdlet
            # But it should not be in the FromFile parameter set at all
            $customizationFileParam.ParameterSets.Keys | Should -Not -Contain 'FromFile'
        }
    }
}
