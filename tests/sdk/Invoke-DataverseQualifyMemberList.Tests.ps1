. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQualifyMemberList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QualifyMemberList SDK Cmdlet" {

        It "Invoke-DataverseQualifyMemberList executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QualifyMemberListRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "QualifyMemberList"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.QualifyMemberListResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseQualifyMemberList -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "QualifyMemberList"
        }

    }
}
