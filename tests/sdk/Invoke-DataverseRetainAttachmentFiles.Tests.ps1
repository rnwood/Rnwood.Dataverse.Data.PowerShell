. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetainAttachmentFiles Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetainAttachmentFiles SDK Cmdlet" {

        It "Invoke-DataverseRetainAttachmentFiles executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetainAttachmentFilesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetainAttachmentFiles"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetainAttachmentFilesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetainAttachmentFiles -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetainAttachmentFiles"
        }

    }
}
