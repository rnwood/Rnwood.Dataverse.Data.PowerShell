Set-StrictMode -version 3.0
$ErrorActionPreference = "Stop"

if ($PSVersionTable.PSEdition -eq "Core") {
	$fsroot = "$PSScriptRoot/net6.0"
} else {
	$fsroot = "$PSScriptRoot/net462"
}

import-module $fsroot/Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll
get-item $fsroot/*.psm1 | import-module 
