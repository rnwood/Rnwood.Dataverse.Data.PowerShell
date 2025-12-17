param(
    [Parameter(Mandatory)]
    [string]$NupkgPath,
    [Parameter(Mandatory)]
    [string]$Version,
    [Parameter()]
    [string]$AssemblyVersion
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
    
    # Update package version to 4-part format to match assembly version exactly
    # XrmToolbox plugin store does exact version comparison
    if ($AssemblyVersion) {
        $versionNode = $xml.SelectSingleNode("//nu:version", $ns)
        if ($versionNode) {
            Write-Host "Updating package version from '$($versionNode.InnerText)' to '$AssemblyVersion'"
            $versionNode.InnerText = $AssemblyVersion
        }
    }
    
    # Find the dependencies element
    $dependencies = $xml.SelectSingleNode("//nu:dependencies", $ns)
    if (-not $dependencies) {
        throw "Could not find dependencies element in nuspec"
    }
    
    # Check if XrmToolBox dependency already exists (to prevent duplicates if script runs multiple times)
    $existingDep = $xml.SelectSingleNode("//nu:dependencies/nu:dependency[@id='XrmToolBox']", $ns)
    if ($existingDep) {
        Write-Host "XrmToolBox dependency already exists, updating version"
        $existingDep.SetAttribute('version', $Version)
    } else {
        # Add the XrmToolBox dependency as a global dependency (directly under dependencies, not in a group)
        # This is required for XrmToolBox plugin store validation
        $dep = $xml.CreateElement('dependency', $nsUri)
        $dep.SetAttribute('id', 'XrmToolBox')
        $dep.SetAttribute('version', $Version)
        $dependencies.AppendChild($dep) | Out-Null
    }
    
    # Remove any empty group elements (groups with no child dependencies)
    $groups = $xml.SelectNodes("//nu:dependencies/nu:group", $ns)
    foreach ($group in $groups) {
        if (-not $group.HasChildNodes) {
            Write-Host "Removing empty dependency group: $($group.GetAttribute('targetFramework'))"
            $group.ParentNode.RemoveChild($group) | Out-Null
        }
    }
    
    $xml.Save($nuspecFile.FullName)
    Write-Host "Successfully added XrmToolBox dependency as global dependency"
    
    # Recreate the nupkg
    Remove-Item $NupkgPath
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $NupkgPath)
    Write-Host "Successfully recreated nupkg"
}
finally {
    # Cleanup
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
