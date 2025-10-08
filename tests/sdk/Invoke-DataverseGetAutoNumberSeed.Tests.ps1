. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetAutoNumberSeed Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetAutoNumberSeed SDK Cmdlet" {

        It "Invoke-DataverseGetAutoNumberSeed executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetAutoNumberSeed"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetAutoNumberSeed -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetAutoNumberSeed"
        }

    }
}
