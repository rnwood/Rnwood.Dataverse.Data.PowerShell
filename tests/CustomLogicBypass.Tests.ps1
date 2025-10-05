. $PSScriptRoot/Common.ps1

Describe 'CustomLogicBypass (ApplyBypassBusinessLogicExecution)' {

    It "ApplyBypassBusinessLogicExecution sets and removes parameters correctly" {
        # Ensure module loaded so SetDataverseRecordCmdlet type is available
        $null = getMockConnection -Entities @('contact')
        # Create instance of SetDataverseRecordCmdlet to call protected method reflectively
        $cmdType = [Rnwood.Dataverse.Data.PowerShell.Commands.SetDataverseRecordCmdlet]
        $cmd = $cmdType::new()

        # Set bypass types and ids
    $enumType = [Rnwood.Dataverse.Data.PowerShell.Commands.CustomLogicBypassableOrganizationServiceCmdlet+BusinessLogicTypes]
        $cmd.BypassBusinessLogicExecution = @($enumType::CustomSync)
        $guid = [Guid]::NewGuid()
        $cmd.BypassBusinessLogicExecutionStepIds = @($guid)

    # Prepare a request (create instance from the Microsoft.Xrm.Sdk assembly that the module loaded)
    $sdkAssembly = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } | Select-Object -First 1
    $orgReqType = $sdkAssembly.GetType('Microsoft.Xrm.Sdk.OrganizationRequest')
    $request = [System.Activator]::CreateInstance($orgReqType, @('Create'))

    $mi = $cmd.GetType().GetMethod('ApplyBypassBusinessLogicExecution', [Reflection.BindingFlags] 'NonPublic, Instance')
    $null = $mi.Invoke($cmd, @($request))

        $request.Parameters.Keys | Should -Contain 'BypassBusinessLogicExecution'
        $request.Parameters['BypassBusinessLogicExecution'] | Should -Contain 'CustomSync'
        $request.Parameters.Keys | Should -Contain 'BypassBusinessLogicExecutionStepIds'
        $request.Parameters['BypassBusinessLogicExecutionStepIds'] | Should -Contain $guid.ToString()

        # Now clear the properties and ensure parameters removed
        $cmd.BypassBusinessLogicExecution = $null
        $cmd.BypassBusinessLogicExecutionStepIds = $null
    $null = $mi.Invoke($cmd, @($request))

        $request.Parameters.ContainsKey('BypassBusinessLogicExecution') | Should -BeFalse
        $request.Parameters.ContainsKey('BypassBusinessLogicExecutionStepIds') | Should -BeFalse
    }
}
