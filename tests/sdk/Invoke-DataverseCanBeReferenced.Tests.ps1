. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCanBeReferenced Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CanBeReferenced SDK Cmdlet" {

        It "Invoke-DataverseCanBeReferenced executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CanBeReferencedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CanBeReferenced"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CanBeReferencedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCanBeReferenced -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CanBeReferenced"
        }

    }
}
