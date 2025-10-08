. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCompoundCreate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CompoundCreate SDK Cmdlet" {

        It "Invoke-DataverseCompoundCreate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CompoundCreateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CompoundCreate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CompoundCreateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCompoundCreate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CompoundCreate"
        }

    }
}
