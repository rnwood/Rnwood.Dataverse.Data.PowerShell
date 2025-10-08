. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMailboxTrackingFolders Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMailboxTrackingFolders SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMailboxTrackingFolders executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveMailboxTrackingFoldersRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveMailboxTrackingFolders"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveMailboxTrackingFoldersResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveMailboxTrackingFolders -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMailboxTrackingFolders"
        }

    }
}
