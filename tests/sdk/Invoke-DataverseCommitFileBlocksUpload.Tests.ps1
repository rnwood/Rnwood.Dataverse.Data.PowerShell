. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCommitFileBlocksUpload Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CommitFileBlocksUpload SDK Cmdlet" {

        It "Invoke-DataverseCommitFileBlocksUpload executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CommitFileBlocksUploadRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CommitFileBlocksUpload"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CommitFileBlocksUploadResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCommitFileBlocksUpload -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CommitFileBlocksUpload"
        }

    }
}
