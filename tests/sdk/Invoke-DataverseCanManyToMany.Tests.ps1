. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCanManyToMany Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CanManyToMany SDK Cmdlet" {

        It "Invoke-DataverseCanManyToMany executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CanManyToManyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CanManyToMany"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CanManyToManyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCanManyToMany -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CanManyToMany"
        }

    }
}
