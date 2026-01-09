set-strictmode -version 3.0

<#
.SYNOPSIS
Reads a folder of JSON files written out by Set-DataverseRecordFolder and converts back into a stream of PS objects.

.DESCRIPTION
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

File and image columns that were stored as separate binary files are automatically restored as byte arrays in the returned objects.

.PARAMETER InputPath
Path to folder to read JSON files from.

.PARAMETER deletions
If specified, reads from the 'deletions' subfolder instead of the main folder. This allows reading records that were present previously but have been deleted.

.EXAMPLE
Get-DataverseRecordsFolder -InputPath data/contacts | Set-DataverseRecord -connection $c

Reads files from `data/contacts` and uses them to create/update records in Dataverse using the existing connection `$c`.
See documentation for `Set-DataverseRecord` as there are option to control how/if existing records will be matched and updated.

.EXAMPLE
Connect-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
Get-DataverseRecordsFolder -InputPath data/contacts | Set-DataverseRecord

Reads files from `data/contacts` and uses them to create/update records in Dataverse using the default connection.

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
		
		$filesPath = Join-Path $InputPath "_files"
		
		get-childitem $path -filter *.json | foreach-object {
			$record = get-content $_.fullname -encoding utf8 | convertfrom-json
			
			# Collect properties that need to be restored from file references
			$filesToRestore = @()
			foreach ($prop in $record.PSObject.Properties) {
				$value = $prop.Value
				
				# Check if property is a file reference
				if ($null -ne $value -and 
				    $value -is [PSCustomObject] -and 
				    (Get-Member -InputObject $value -Name "__fileReference" -ErrorAction SilentlyContinue)) {
					
					$filesToRestore += @{
						PropertyName = $prop.Name
						FileReference = $value.__fileReference
					}
				}
			}
			
			# Restore byte arrays from file references
			foreach ($fileInfo in $filesToRestore) {
				$binaryFilePath = Join-Path $filesPath $fileInfo.FileReference
				
				if (Test-Path $binaryFilePath) {
					# Read binary file and restore as byte array
					[byte[]]$byteArray = [System.IO.File]::ReadAllBytes($binaryFilePath)
					# Remove the file reference property and add the byte array
					$record.PSObject.Properties.Remove($fileInfo.PropertyName)
					$record.PSObject.Properties.Add([PSNoteProperty]::new($fileInfo.PropertyName, $byteArray))
				} else {
					Write-Warning "Binary file not found: $binaryFilePath for property $($fileInfo.PropertyName)"
					$record.PSObject.Properties.Remove($fileInfo.PropertyName)
					$record.PSObject.Properties.Add([PSNoteProperty]::new($fileInfo.PropertyName, $null))
				}
			}
			
			$record
		}
	}

	end {

	}
}