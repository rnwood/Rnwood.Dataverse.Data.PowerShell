. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCanBeReferencing Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CanBeReferencing SDK Cmdlet" {

        It "Invoke-DataverseCanBeReferencing executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CanBeReferencingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CanBeReferencing"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CanBeReferencingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCanBeReferencing -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CanBeReferencing"
        }

    }
}
