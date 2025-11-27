param(
    [Parameter(Mandatory)]
    [string]$NupkgPath,
    [Parameter(Mandatory)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

Write-Host "Adding XrmToolBox dependency to: $NupkgPath"

# Load the nupkg (which is a zip file)
Add-Type -AssemblyName System.IO.Compression.FileSystem

# Create temp directory
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tempDir | Out-Null

try {
    # Extract
    [System.IO.Compression.ZipFile]::ExtractToDirectory($NupkgPath, $tempDir)
    
    # Find and modify nuspec
    $nuspecFile = Get-ChildItem -Path $tempDir -Filter "*.nuspec" | Select-Object -First 1
    if (-not $nuspecFile) {
        throw "No nuspec file found in nupkg"
    }
    
    Write-Host "Modifying nuspec: $($nuspecFile.FullName)"
    
    $xml = [xml](Get-Content $nuspecFile.FullName)
    $nsUri = $xml.DocumentElement.NamespaceURI
    
    $ns = New-Object Xml.XmlNamespaceManager($xml.NameTable)
    $ns.AddNamespace('nu', $nsUri)
    
    $group = $xml.SelectSingleNode("//nu:dependencies/nu:group", $ns)
    
    if ($group) {
        $dep = $xml.CreateElement('dependency', $nsUri)
        $dep.SetAttribute('id', 'XrmToolBox')
        $dep.SetAttribute('version', $Version)
        $group.AppendChild($dep) | Out-Null
        $xml.Save($nuspecFile.FullName)
        Write-Host "Successfully added XrmToolBox dependency"
    } else {
        throw "Could not find dependency group in nuspec"
    }
    
    # Recreate the nupkg
    Remove-Item $NupkgPath
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $NupkgPath)
    Write-Host "Successfully recreated nupkg"
}
finally {
    # Cleanup
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
