. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetValidManyToMany Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetValidManyToMany SDK Cmdlet" {

        It "Invoke-DataverseGetValidManyToMany executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetValidManyToManyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetValidManyToMany"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetValidManyToManyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetValidManyToMany -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetValidManyToMany"
        }

    }
}
