param($projectdir)
$projectdir=$projectdir.Trim('""')
$ErrorActionPreference="Stop"

if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	install-module -scope CurrentUser PlatyPs -force
}

import-module $PSScriptRoot/Rnwood.Dataverse.Data.PowerShell.psd1

update-markdownhelp -path $projectdir/docs
