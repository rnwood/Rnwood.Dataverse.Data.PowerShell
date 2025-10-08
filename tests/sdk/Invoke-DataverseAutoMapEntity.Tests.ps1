. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAutoMapEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AutoMapEntity SDK Cmdlet" {

        It "Invoke-DataverseAutoMapEntity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AutoMapEntityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "AutoMapEntity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.AutoMapEntityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseAutoMapEntity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "AutoMapEntity"
        }

    }
}
