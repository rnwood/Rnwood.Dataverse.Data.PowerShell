. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDownload Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Download SDK Cmdlet" {

        It "Invoke-DataverseDownload executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DownloadRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DownloadRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DownloadResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDownload -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DownloadRequest"
        }

    }
}
