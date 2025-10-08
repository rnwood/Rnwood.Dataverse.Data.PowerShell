. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDependenciesForUninstall Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDependenciesForUninstall SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDependenciesForUninstall executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveDependenciesForUninstallRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDependenciesForUninstall"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveDependenciesForUninstallResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDependenciesForUninstall -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDependenciesForUninstall"
        }

    }
}
