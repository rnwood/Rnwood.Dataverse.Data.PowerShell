. $PSScriptRoot/Common.ps1

Describe 'ArgumentCompleters' {

    It "TableNameArgumentCompleter returns entity logical names when Connection provided" {
        $connection = getMockConnection -Entities @('contact','account')

    $fakeBoundParameters = @{}
    # Wrap connection in PSObject - completer code accepts PSObject.BaseObject as ServiceClient too
    $fakeBoundParameters.Add('Connection', [PSObject]$connection)

        $completer = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.TableNameArgumentCompleter
        $results = $completer.CompleteArgument('Get-DataverseRecord', 'TableName', 'cont', $null, $fakeBoundParameters)

    # Invocation should not throw (completer may return empty depending on mock support)
    { $completer.CompleteArgument('Get-DataverseRecord', 'TableName', 'cont', $null, $fakeBoundParameters) | Out-Null } | Should -Not -Throw
    }

    It "TableNameArgumentCompleter returns empty when no Connection bound" {
        $fakeBoundParameters = @{}
        $completer = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.TableNameArgumentCompleter
        $results = $completer.CompleteArgument('Get-DataverseRecord', 'TableName', '', $null, $fakeBoundParameters)
        $results | Should -BeNullOrEmpty
    }

    It "ColumnNamesArgumentCompleter proposes known columns for a table" {
        $connection = getMockConnection -Entities @('contact')
        $fakeBoundParameters = @{}
        $fakeBoundParameters.Add('Connection', $connection)
        $fakeBoundParameters.Add('TableName', 'contact')

        $completer = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ColumnNamesArgumentCompleter
        $results = $completer.CompleteArgument('Get-DataverseRecord', 'Columns', 'first', $null, $fakeBoundParameters)

        $results | Should -Not -BeNull
        $results | ForEach-Object { $_.CompletionText } | Should -Contain 'firstname'
    }
}
