. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUploadReportDefinition Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UploadReportDefinition SDK Cmdlet" {

        It "Invoke-DataverseUploadReportDefinition executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UploadReportDefinitionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UploadReportDefinition"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UploadReportDefinitionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUploadReportDefinition -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UploadReportDefinition"
        }

    }
}
