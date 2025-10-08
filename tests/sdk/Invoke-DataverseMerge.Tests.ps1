. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseMerge Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Merge SDK Cmdlet" {

        It "Invoke-DataverseMerge merges two records" {
            $targetId = [Guid]::NewGuid()
            $subordinateId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.MergeRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.MergeRequest"
                $request.Target | Should -Not -BeNull
                $request.Target | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                $request.SubordinateId | Should -BeOfType [System.Guid]
                $request.UpdateContent | Should -Not -BeNull
                $request.PerformParentingChecks | Should -BeOfType [System.Boolean]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.MergeResponse
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $targetId)
            $updateContent = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $updateContent["firstname"] = "Merged"
            
            $response = Invoke-DataverseMerge -Connection $script:conn -Target $target -SubordinateId $subordinateId -UpdateContent $updateContent -PerformParentingChecks $false
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.MergeResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Target.Id | Should -Be $targetId
            $proxy.LastRequest.SubordinateId | Should -Be $subordinateId
        }
    }

    }
}
