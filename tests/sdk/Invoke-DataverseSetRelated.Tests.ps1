. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetRelated Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetRelated SDK Cmdlet" {

        It "Invoke-DataverseSetRelated executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetRelatedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetRelated"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetRelatedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetRelated -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetRelated"
        }

    }
}
