. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetLocLabels Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetLocLabels SDK Cmdlet" {

        It "Invoke-DataverseSetLocLabels executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetLocLabelsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetLocLabels"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetLocLabelsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetLocLabels -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetLocLabels"
        }

    }
}
