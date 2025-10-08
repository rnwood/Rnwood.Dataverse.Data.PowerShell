. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateRelationship Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateRelationship SDK Cmdlet" {

        It "Invoke-DataverseUpdateRelationship executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateRelationshipRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateRelationship"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateRelationshipResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateRelationship -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateRelationship"
        }

    }
}
