param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [string]$Owner = "rnwood",
    [string]$Repo = "Rnwood.Dataverse.Data.PowerShell",
    [string]$CategoryName = "Script gallery"
)

$headers = @{
    "Authorization" = "Bearer $GitHubToken"
    "Content-Type" = "application/json"
}

$graphqlUrl = "https://api.github.com/graphql"

function Invoke-GraphQL {
    param($Query, $Variables)
    
    $body = @{
        query = $Query
        variables = $Variables
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri $graphqlUrl -Method Post -Headers $headers -Body $body
        if ($response.errors) {
            Write-Error "GraphQL Error: $($response.errors | ConvertTo-Json -Depth 5)"
            return $null
        }
        return $response.data
    } catch {
        Write-Error "Request Failed: $_"
        return $null
    }
}

# 1. Get Repository ID and Category ID
Write-Host "Fetching repository info..."
$repoQuery = @"
query(`$owner: String!, `$repo: String!) {
    repository(owner: `$owner, name: `$repo) {
        id
        discussionCategories(first: 100) {
            nodes {
                id
                name
            }
        }
        labels(first: 100) {
            nodes {
                id
                name
            }
        }
    }
}
"@

$repoData = Invoke-GraphQL -Query $repoQuery -Variables @{ owner = $Owner; repo = $Repo }
if (-not $repoData) { exit }

$repoId = $repoData.repository.id
$categoryId = ($repoData.repository.discussionCategories.nodes | Where-Object { $_.name -eq $CategoryName }).id

if (-not $categoryId) {
    Write-Error "Category '$CategoryName' not found."
    exit
}

Write-Host "Repository ID: $repoId"
Write-Host "Category ID: $categoryId"

$existingLabels = @{} # Name -> ID
foreach ($label in $repoData.repository.labels.nodes) {
    $existingLabels[$label.name] = $label.id
}

# Function to ensure label exists
function Ensure-Label {
    param($Name)
    if ($existingLabels.ContainsKey($Name)) {
        return $existingLabels[$Name]
    }
    
    Write-Host "Creating label '$Name'..."
    $createLabelMutation = @"
mutation(`$repoId: ID!, `$name: String!) {
    createLabel(input: {repositoryId: `$repoId, name: `$name, color: "0366d6"}) {
        label {
            id
        }
    }
}
"@
    $res = Invoke-GraphQL -Query $createLabelMutation -Variables @{ repoId = $repoId; name = $Name }
    if ($res.createLabel.label.id) {
        $existingLabels[$Name] = $res.createLabel.label.id
        return $res.createLabel.label.id
    }
    return $null
}

# 2. Get existing discussions to check for duplicates
Write-Host "Fetching existing discussions..."
$discussionsQuery = @"
query(`$owner: String!, `$repo: String!, `$categoryId: ID!) {
    repository(owner: `$owner, name: `$repo) {
        discussions(first: 100, categoryId: `$categoryId) {
            nodes {
                title
                number
            }
            pageInfo {
                hasNextPage
                endCursor
            }
        }
    }
}
"@

# Simple pagination (fetch all)
$existingTitles = @{}
$cursor = $null
do {
    $vars = @{ owner = $Owner; repo = $Repo; categoryId = $categoryId }
    # Pagination logic omitted for simplicity, fetching first 100
    
    $discData = Invoke-GraphQL -Query $discussionsQuery -Variables $vars
    if (-not $discData) { break }
    
    foreach ($node in $discData.repository.discussions.nodes) {
        $existingTitles[$node.title] = $node.number
    }
    
    if ($discData.repository.discussions.pageInfo.hasNextPage) {
        Write-Warning "More than 100 discussions exist. Pagination not fully implemented in this script."
        break
    } else {
        break
    }
} while ($true)

Write-Host "Found $($existingTitles.Count) existing discussions."

# 3. Process files
$files = Get-ChildItem "examples-gallery/*.md"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Parse frontmatter
    if ($content -match '(?s)^---\r?\n(.+?)\r?\n---\r?\n(.+)') {
        $frontmatterRaw = $matches[1]
        $body = $matches[2].Trim()
        
        # Parse YAML-like frontmatter manually
        $title = ""
        $tags = @()
        
        if ($frontmatterRaw -match 'title:\s*"(.+?)"') {
            $title = $matches[1]
        }
        if ($frontmatterRaw -match 'tags:\s*\[(.+?)\]') {
            $tagsRaw = $matches[1]
            $tags = $tagsRaw -split ',' | ForEach-Object { $_.Trim().Trim("'").Trim('"') }
        }
        
        if (-not $title) {
            Write-Warning "Skipping $($file.Name): No title found."
            continue
        }
        
        # Check for duplicate
        if ($existingTitles.ContainsKey($title)) {
            Write-Host "Skipping '$title': Already exists (#$($existingTitles[$title]))"
            continue
        }
        
        Write-Host "Creating discussion: '$title'..."
        
        # Create discussion
        $createMutation = @"
mutation(`$repoId: ID!, `$categoryId: ID!, `$title: String!, `$body: String!) {
    createDiscussion(input: {repositoryId: `$repoId, categoryId: `$categoryId, title: `$title, body: `$body}) {
        discussion {
            id
            number
            url
        }
    }
}
"@
        
        $createData = Invoke-GraphQL -Query $createMutation -Variables @{ repoId = $repoId; categoryId = $categoryId; title = $title; body = $body }
        
        if ($createData.createDiscussion.discussion) {
            $discId = $createData.createDiscussion.discussion.id
            $discNum = $createData.createDiscussion.discussion.number
            Write-Host "Created discussion #$discNum"
            
            # Add labels
            if ($tags.Count -gt 0) {
                $labelIds = @()
                foreach ($tag in $tags) {
                    $labelName = "example-$tag"
                    $lid = Ensure-Label -Name $labelName
                    if ($lid) { $labelIds += $lid }
                }
                
                if ($labelIds.Count -gt 0) {
                    $addLabelsMutation = @"
mutation(`$id: ID!, `$labelIds: [ID!]!) {
    addLabelsToLabelable(input: {labelableId: `$id, labelIds: `$labelIds}) {
        clientMutationId
    }
}
"@
                    Invoke-GraphQL -Query $addLabelsMutation -Variables @{ id = $discId; labelIds = $labelIds } | Out-Null
                    Write-Host "Added labels: $($tags -join ', ')"
                }
            }
        }
        
        Start-Sleep -Seconds 1 # Rate limiting
    }
}
