set-strictmode -version 3.0

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