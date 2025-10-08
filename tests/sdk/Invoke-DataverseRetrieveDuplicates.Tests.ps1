. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDuplicates Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDuplicates SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDuplicates executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDuplicates"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDuplicates -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDuplicates"
        }

    }
}
