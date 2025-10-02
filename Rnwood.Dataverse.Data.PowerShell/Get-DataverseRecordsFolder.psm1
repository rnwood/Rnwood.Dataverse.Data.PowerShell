set-strictmode -version 3.0

<#
.SYNOPSIS
Reads a folder of JSON files written out by Set-DataverseRecordFolder and converts back into a stream of PS objects.

.DESCRIPTION
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

.PARAMETER InputPath
Path to folder to read JSON files from.

.PARAMETER deletions
If specified, reads from the 'deletions' subfolder instead of the main folder. This allows reading records that were present previously but have been deleted.

.EXAMPLE
Get-DataverseRecordsFolder -InputPath data/contacts | Set-DataverseRecord -connection $c

Reads files from `data/contacts` and uses them to create/update records in Dataverse using the existing connection `$c`.
See documentation for `Set-DataverseRecord` as there are option to control how/if existing records will be matched and updated.

.OUTPUTS
System.Management.Automation.PSObject

#>
function Get-DataverseRecordsFolder {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory)][string] $InputPath,
		[switch] $deletions
	)

	begin {
	}

	process {

		$path = $InputPath
		if ($deletions) {
			$path = "$InputPath/deletions"
		}
		get-childitem $path -filter *.json | foreach-object {
			get-content $_.fullname -encoding utf8 | convertfrom-json
		}
	}

	end {

	}
}