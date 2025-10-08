. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseValidateSavedQuery Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ValidateSavedQuery SDK Cmdlet" {

        It "Invoke-DataverseValidateSavedQuery executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ValidateSavedQueryRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ValidateSavedQuery"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ValidateSavedQueryResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseValidateSavedQuery -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ValidateSavedQuery"
        }

    }
}
