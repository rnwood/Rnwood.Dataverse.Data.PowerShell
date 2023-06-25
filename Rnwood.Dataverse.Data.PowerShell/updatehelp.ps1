param($projectdir)
$projectdir=$projectdir.Trim('""')
$ErrorActionPreference="Stop"
install-module -scope CurrentUser PlatyPs -force

import-module $PSScriptRoot/Rnwood.Dataverse.Data.PowerShell.psd1

update-markdownhelp -path $projectdir/docs
