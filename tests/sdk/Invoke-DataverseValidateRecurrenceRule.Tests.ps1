. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseValidateRecurrenceRule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ValidateRecurrenceRule SDK Cmdlet" {

        It "Invoke-DataverseValidateRecurrenceRule executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ValidateRecurrenceRuleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ValidateRecurrenceRule"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ValidateRecurrenceRuleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseValidateRecurrenceRule -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ValidateRecurrenceRule"
        }

    }
}
