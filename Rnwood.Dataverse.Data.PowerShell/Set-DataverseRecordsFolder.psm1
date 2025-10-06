set-strictmode -version 3.0

<#
.SYNOPSIS
Writes a list of Dataverse records to a folder of JSON files.

.DESCRIPTION
Writes a list of Dataverse records to a folder where each file represents a single record. The files are named using the Id property (or properties specified via -idproperties).

.PARAMETER OutputPath
Path to write output to.

.PARAMETER InputObject
Dataverse record(s) to write. Generally should be piped in from the pipeline.

.PARAMETER withdeletions
Output a list of deletions (records that were there last time, but are no longer present in the inputs) to 'deletions' subfolder of output.

.PARAMETER idproperties
Specifies the list of properties that will be used to generate a unique name for each file. By default this is "Id".

.EXAMPLE
Get-DataverseRecord -connection $connection -tablename contact | Set-DataverseRecordsFolder data/contacts

Writes all contacts to the folder `data/contacts`.

.EXAMPLE
Connect-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
Get-DataverseRecord -tablename contact | Set-DataverseRecordsFolder data/contacts

Writes all contacts to the folder `data/contacts` using the default connection.

.INPUTS
System.Management.Automation.PSObject

.OUTPUTS
System.Object

#>
function Set-DataverseRecordsFolder {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory)][string] $OutputPath, 
		[Parameter(ValueFromPipeline=$true)] [PSObject] $InputObject,
		[switch] $withdeletions,
		[string[]]$idproperties = @("Id")
	)

	begin {
	
		if (-not (test-path $OutputPath)) {
			new-item -type directory $OutputPath | out-null
		}

		if ($withdeletions) {
			if (-not (test-path $OutputPath/deletions)) {
				new-item -type directory $OutputPath/deletions | out-null
			}
		} elseif (test-path $OutputPath/deletions) {
			remove-item -recurse $OutputPath/deletions
		}

		$newfiles = @()
	}

	process {
		$name = ($idproperties | ForEach-Object { $InputObject.$_ }) -join "-"
		# Replace invalid filenam chars
		$name = $name.Split([IO.Path]::GetInvalidFileNameChars()) -join '_'
		$filename = "${name}.json"
		$InputObject | convertto-json -depth 100 | out-file -encoding utf8 (join-path $OutputPath $filename)

		if ($newfiles -contains $filename) {
			throw "The properties $idproperties do not result in a unique filename. The value ''$filename' was generated more than once."
		}

		$newfiles += $filename
	}

	end {
		# Find files which we didn't just overwrite and create deletion
		get-item $OutputPath/*.json | Where-Object { $newfiles -notcontains $_.Name } | ForEach-Object {
			if ($withdeletions) {
				move-item $_ $OutputPath/deletions
			} else {
				remove-item $_
			}
		}

		if ($withdeletions) {
			# Find files that have been recreated and remove the deletion
			get-item $OutputPath/deletions/*.json | Where-Object { $newfiles -contains $_.Name } | Remove-Item

			if (-not (test-path "$OutputPath/deletions/*.json")) {
				"This file ensures the directory is kept when in source control system" | out-file $OutputPath/deletions/keep.me
			} elseif (test-path $OutputPath/deletions/keep.me) {
				remove-item $OutputPath/deletions/keep.me
			}
		}

		if (-not (test-path "$OutputPath/*.json")) {
				"This file ensures the directory is kept when in source control system" | out-file $OutputPath/keep.me
			} elseif (test-path $OutputPath/keep.me) {
				remove-item $OutputPath/keep.me
			}
		}
	
}