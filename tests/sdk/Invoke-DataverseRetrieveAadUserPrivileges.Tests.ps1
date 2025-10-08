. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAadUserPrivileges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAadUserPrivileges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAadUserPrivileges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAadUserPrivilegesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAadUserPrivileges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAadUserPrivilegesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAadUserPrivileges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAadUserPrivileges"
        }

    }
}
