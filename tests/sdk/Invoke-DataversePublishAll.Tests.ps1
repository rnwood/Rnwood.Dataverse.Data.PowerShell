. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishAll Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishAll SDK Cmdlet" {

        It "Invoke-DataversePublishAll executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishAllRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PublishAll"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PublishAllResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePublishAll -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PublishAll"
        }

    }
}
