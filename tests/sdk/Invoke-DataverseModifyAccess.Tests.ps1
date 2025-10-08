. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseModifyAccess Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ModifyAccess SDK Cmdlet" {

        It "Invoke-DataverseModifyAccess executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ModifyAccessRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ModifyAccess"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ModifyAccessResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseModifyAccess -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ModifyAccess"
        }

    }
}
