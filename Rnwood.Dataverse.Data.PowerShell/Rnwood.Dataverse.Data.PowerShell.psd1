@{
# Script module or binary module file associated with this manifest.
RootModule = ""

# Version number of this module.
ModuleVersion = '100.0.0'

# ID used to uniquely identify this module
GUID = 'CF2F6EF2-D649-4DBE-909F-CF3C6FED6112'

# Author of this module
Author = 'Rob Wood <rob@rnwood.co.uk>'

# Company or vendor of this module
CompanyName = 'Rob Wood'

Description = 'Dataverse data manipulation cmdlets'

# Copyright statement for this module
Copyright = '(c) 2024 Robert Wood <rob@rnwood.co.uk>. See licence.'

CompatiblePSEditions = @("Core", "Desktop")

# Description of the functionality provided by this module
# Description = ''

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = @()

RequiredModules = @()

PrivateData = @{
	PSData = @{
		ExternalModuleDependencies = @()
	}
}

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = 
@(if ($PSEdition -eq 'Core') {
	@(
		"net8.0/Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll",
		"Get-DataverseRecordsFolder.psm1",
		"Set-DataverseRecordsFolder.psm1"
	)
}
 else {
	 @(
		"net462/Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll",
		"Get-DataverseRecordsFolder.psm1",
		"Set-DataverseRecordsFolder.psm1"
	 )
 }
)


# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = '*'

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'

# List of all modules packaged with this module
ModuleList = @()

# List of all files packaged with this module
# FileList = @()


# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''
}