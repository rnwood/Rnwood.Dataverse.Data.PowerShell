. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetReportRelated Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetReportRelated SDK Cmdlet" {

        It "Invoke-DataverseSetReportRelated executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetReportRelatedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetReportRelated"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetReportRelatedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetReportRelated -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetReportRelated"
        }

    }
}
