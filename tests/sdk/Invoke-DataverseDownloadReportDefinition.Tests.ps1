. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDownloadReportDefinition Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DownloadReportDefinition SDK Cmdlet" {

        It "Invoke-DataverseDownloadReportDefinition executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DownloadReportDefinitionRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DownloadReportDefinitionRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DownloadReportDefinitionResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDownloadReportDefinition -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DownloadReportDefinitionRequest"
        }

    }
}
