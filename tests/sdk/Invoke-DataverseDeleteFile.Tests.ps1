. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteFile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteFile SDK Cmdlet" {

        It "Invoke-DataverseDeleteFile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteFileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteFile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteFileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteFile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteFile"
        }

    }
}
