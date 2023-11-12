set-strictmode -version 3.0

function Set-DataverseRecordsFolder {
	[CmdletBinding()]
	param([Parameter(Mandatory)][string] $OutputPath, [Parameter(ValueFromPipeline=$true)] [PSObject] $InputObject)

	begin {
		if (test-path $OutputPath) {
			remove-item $OutputPath/* -recurse -force
		} else {
			new-item -type directory $OutputPath
		}
	}

	process {
		$name = $_.Id
		$InputObject | convertto-json -depth 100 | out-file -encoding utf8 (join-path $OutputPath "$name.json")
	}

	end {

	}
}