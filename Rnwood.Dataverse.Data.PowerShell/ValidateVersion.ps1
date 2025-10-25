# Validate PowerShell version requirements based on edition
if ($PSEdition -eq 'Core') {
    $requiredVersion = [Version]'7.4.0'
    if ($PSVersionTable.PSVersion -lt $requiredVersion) {
        throw "PowerShell Core $requiredVersion or later is required for this module. Current version is $($PSVersionTable.PSVersion). Please upgrade PowerShell or use an earlier version of this module."
    }
}
