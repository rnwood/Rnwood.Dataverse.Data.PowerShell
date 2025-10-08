. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateAsyncJobToRevokeInheritedAccess Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateAsyncJobToRevokeInheritedAccess SDK Cmdlet" {

        It "Invoke-DataverseCreateAsyncJobToRevokeInheritedAccess executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateAsyncJobToRevokeInheritedAccessRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CreateAsyncJobToRevokeInheritedAccess"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CreateAsyncJobToRevokeInheritedAccessResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCreateAsyncJobToRevokeInheritedAccess -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateAsyncJobToRevokeInheritedAccess"
        }

    }
}
