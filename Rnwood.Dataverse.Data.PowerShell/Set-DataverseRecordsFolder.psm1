set-strictmode -version 3.0

function Set-DataverseRecordsFolder {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory)][string] $OutputPath, 
		[Parameter(ValueFromPipeline=$true)] [PSObject] $InputObject,
		[switch] $withdeletions
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
		$name = $_.Id
		$filename = "${name}.json"
		$InputObject | convertto-json -depth 100 | out-file -encoding utf8 (join-path $OutputPath $filename)
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