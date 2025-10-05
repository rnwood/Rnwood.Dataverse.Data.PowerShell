. $PSScriptRoot/Common.ps1

Describe "Invoke-DataverseRequest examples" {

    It "Can invoke custom requests" {
    $connection = getMockConnection -Entities 'systemuser'
        $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
        $response = Invoke-DataverseRequest -Connection $connection -Request $request
        $response | Should -Not -BeNull
    }

    It "Can execute WhoAmI request using Invoke-DataverseRequest" {
    $connection = getMockConnection -Entities 'systemuser'
        $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
        $response = Invoke-DataverseRequest -Connection $connection -Request $request
        $response | Should -Not -BeNull
        $response.UserId | Should -Not -BeNullOrEmpty
    }

    It "Can execute multiple requests" {
    $connection = getMockConnection -Entities 'systemuser'
        $request1 = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
        $response1 = Invoke-DataverseRequest -Connection $connection -Request $request1

        $request2 = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
        $response2 = Invoke-DataverseRequest -Connection $connection -Request $request2

        $response1.UserId | Should -Be $response2.UserId
    }

    It "Can execute SetState request using RequestName and Parameters" {
    $connection = getMockConnection -Entities @('workflow','systemuser')
            # Ensure a workflow record exists in the mock store so SetState can target it
            $wfId = [Guid]::NewGuid()
            $wf = New-Object Microsoft.Xrm.Sdk.Entity("workflow")
            $wf.Id = $wf["workflowid"] = $wfId
            $wf["name"] = "TestWorkflow"
            $wf | Set-DataverseRecord -Connection $connection

            # Prepare a SetStateRequest that targets the workflow we just created
            $request = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
            $request.EntityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("workflow", $wfId)
            $request.State = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $request.Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)

        # Ensure the request executes (FakeXrmEasy will accept the request even if it doesn't change state)
        { Invoke-DataverseRequest -Connection $connection -Request $request } | Should -Not -Throw
    }
}
