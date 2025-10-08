. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCompoundUpdateDuplicateDetectionRule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CompoundUpdateDuplicateDetectionRule SDK Cmdlet" {

        It "Invoke-DataverseCompoundUpdateDuplicateDetectionRule executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CompoundUpdateDuplicateDetectionRuleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "CompoundUpdateDuplicateDetectionRule"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.CompoundUpdateDuplicateDetectionRuleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseCompoundUpdateDuplicateDetectionRule -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CompoundUpdateDuplicateDetectionRule"
        }

    }
}
