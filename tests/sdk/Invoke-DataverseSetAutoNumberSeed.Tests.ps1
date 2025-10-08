. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetAutoNumberSeed Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetAutoNumberSeed SDK Cmdlet" {

        It "Invoke-DataverseSetAutoNumberSeed executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetAutoNumberSeedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetAutoNumberSeed"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetAutoNumberSeedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetAutoNumberSeed -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetAutoNumberSeed"
        }

    }
}
