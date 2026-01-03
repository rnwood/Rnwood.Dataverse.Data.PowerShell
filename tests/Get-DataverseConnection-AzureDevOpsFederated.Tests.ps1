. $PSScriptRoot/Common.ps1

Describe "Get-DataverseConnection - Azure DevOps Federated Authentication" {
    
    It "AzureDevOpsFederated parameter set requires TenantId" {
        # This test validates that the parameter set definition requires TenantId
        $cmd = Get-Command Get-DataverseConnection
        $parameterSet = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate with Azure DevOps federated identity" }
        
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $tenantIdParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "TenantId" }
        $tenantIdParam.IsMandatory | Should -Be $true
    }
    
    It "AzureDevOpsFederated parameter set requires ClientId" {
        $cmd = Get-Command Get-DataverseConnection
        $parameterSet = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate with Azure DevOps federated identity" }
        
        $clientIdParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "ClientId" }
        $clientIdParam.IsMandatory | Should -Be $true
    }
    
    It "AzureDevOpsFederated parameter set requires Url" {
        $cmd = Get-Command Get-DataverseConnection
        $parameterSet = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate with Azure DevOps federated identity" }
        
        $urlParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Url" }
        $urlParam.IsMandatory | Should -Be $true
    }
    
    It "AzureDevOpsFederated parameter set has ServiceConnectionId as optional" {
        $cmd = Get-Command Get-DataverseConnection
        $parameterSet = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate with Azure DevOps federated identity" }
        
        $serviceConnParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "ServiceConnectionId" }
        $serviceConnParam | Should -Not -BeNullOrEmpty
        $serviceConnParam.IsMandatory | Should -Be $false
    }
    
    It "AzureDevOpsFederated switch parameter is available" {
        $cmd = Get-Command Get-DataverseConnection
        $param = $cmd.Parameters["AzureDevOpsFederated"]
        
        $param | Should -Not -BeNullOrEmpty
        $param.ParameterType.Name | Should -Be "SwitchParameter"
    }
    
    It "Errors when AzureDevOpsFederated is used without Azure DevOps pipeline context" {
        # This test verifies error handling when environment variables are not set
        # Run in a clean process without pipeline environment variables
        $output = pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = "$env:TESTMODULEPATH;$env:PSModulePath"
            Import-Module Rnwood.Dataverse.Data.PowerShell
            $ErrorActionPreference = "Continue"
            $connection = Get-DataverseConnection `
                -AzureDevOpsFederated `
                -ClientId "12345678-1234-1234-1234-123456789abc" `
                -TenantId "87654321-4321-4321-4321-cba987654321" `
                -Url "https://test.crm.dynamics.com" `
                -ErrorVariable err
            if ($err) {
                Write-Output "ERROR: $($err[0].Exception.Message)"
            } else {
                Write-Output "SUCCESS"
            }
        } 2>&1
        
        # Should error because required Azure DevOps environment variables are not set
        $output | Should -Match "ERROR|ServiceConnectionId|Azure DevOps|pipeline"
    }
    
    It "Supports Name parameter for saving connection metadata" {
        $cmd = Get-Command Get-DataverseConnection
        $parameterSet = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate with Azure DevOps federated identity" }
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.IsMandatory | Should -Be $false
    }
}
