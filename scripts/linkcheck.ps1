function Get-MarkdownAnchors {
    param([string]$filePath)
    $anchors = @{}
    $content = Get-Content $filePath -Raw
    $lines = $content -split "`n"
    foreach ($line in $lines) {
        if ($line -match '^#{1,6}\s+(.+)$') {
            $heading = $matches[1].Trim()
            # GitHub anchor generation: lowercase, remove punctuation, replace spaces with -
            $anchor = $heading.ToLower() -replace '[^\w\s-]', '' -replace '\s+', '-'
            $anchors["#$anchor"] = $true
        }
    }
    return $anchors
}

$mdFiles = Get-ChildItem -Path . -Recurse -Filter *.md | Select-Object FullName
$brokenLinks = @()
$externalLinks = @()

# Cache anchors
$anchorsCache = @{}

foreach ($file in $mdFiles) {
    $fullPath = Resolve-Path $file.FullName
    $anchorsCache[$fullPath] = Get-MarkdownAnchors -filePath $file.FullName
}

foreach ($file in $mdFiles) {
    Write-Host "Processing $($file.FullName)"
    $content = Get-Content $file.FullName -Raw
    $links = [regex]::Matches($content, '\[.*?\]\((.*?)\)')
    foreach ($link in $links) {
        $url = $link.Groups[1].Value
        if ($url -match '^https?://') {
            # external link - collect for parallel processing
            $externalLinks += @{File=$file.FullName; Url=$url}
        } elseif ($url -match '^#(.+)$') {
            # anchor in same file
            $anchor = "#$($matches[1])"
            $fullFilePath = Resolve-Path $file.FullName
            if ($anchorsCache.ContainsKey($fullFilePath) -and -not $anchorsCache[$fullFilePath].ContainsKey($anchor)) {
                $brokenLinks += "Broken anchor in $($file.FullName): $url"
            }
        } elseif ($url -match '(.+?)#(.+)') {
            # file with anchor
            $filePart = $matches[1]
            $anchor = "#$($matches[2])"
            $resolvedPath = Join-Path (Split-Path $file.FullName) $filePart
            if (-not (Test-Path $resolvedPath)) {
                $brokenLinks += "Broken link in $($file.FullName): $url (file not found)"
            } else {
                $fullResolvedPath = Resolve-Path $resolvedPath
                if ($anchorsCache.ContainsKey($fullResolvedPath) -and -not $anchorsCache[$fullResolvedPath].ContainsKey($anchor)) {
                    $brokenLinks += "Broken anchor in $($file.FullName): $url"
                }
            }
        } else {
            # relative file link
            $resolvedPath = Join-Path (Split-Path $file.FullName) $url
            if (-not (Test-Path $resolvedPath)) {
                $brokenLinks += "Broken link in $($file.FullName): $url"
            }
        }
    }
}

# Process external links in parallel
$brokenExternal = $externalLinks | ForEach-Object -Parallel {
    $file = $_.File
    $url = $_.Url
    try {
        $response = Invoke-WebRequest -Uri $url -Method Head -TimeoutSec 3 -ErrorAction Stop
        if ($response.StatusCode -ne 200) {
            return ("Broken external link in {0}: {1} (status {2})" -f $file, $url, $response.StatusCode)
        }
    } catch {
        return ("Broken external link in {0}: {1} ({2})" -f $file, $url, $_.Exception.Message)
    }
    return $null
} -ThrottleLimit 5 | Where-Object { $_ -ne $null }

$brokenLinks += $brokenExternal

if ($brokenLinks) {
    Write-Host "Broken links/anchors found:"
    $brokenLinks | ForEach-Object { Write-Host $_ }
    exit 1
} else {
    Write-Host "All links and anchors are valid."
}