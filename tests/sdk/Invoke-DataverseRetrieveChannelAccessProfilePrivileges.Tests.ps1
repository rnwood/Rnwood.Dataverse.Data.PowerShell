. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveChannelAccessProfilePrivileges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveChannelAccessProfilePrivileges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveChannelAccessProfilePrivileges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveChannelAccessProfilePrivileges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveChannelAccessProfilePrivileges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveChannelAccessProfilePrivileges"
        }

    }
}
