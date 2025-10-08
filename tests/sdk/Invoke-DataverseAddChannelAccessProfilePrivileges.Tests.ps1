. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddChannelAccessProfilePrivileges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddChannelAccessProfilePrivileges SDK Cmdlet" {

        It "Invoke-DataverseAddChannelAccessProfilePrivileges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "AddChannelAccessProfilePrivileges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseAddChannelAccessProfilePrivileges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "AddChannelAccessProfilePrivileges"
        }

    }
}
