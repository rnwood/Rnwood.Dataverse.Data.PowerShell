$repo = 'C:\src\ps'
Set-Location $repo

# Backup current main branch
git branch -f backup-main-2025-10-04 main

# Get list of unmerged files
$conflicts = git diff --name-only --diff-filter=U

if (-not $conflicts) {
    Write-Output 'No merge conflicts detected.'
    exit 0
}

Write-Output "Conflicts: $($conflicts -join ', ')"

foreach ($f in $conflicts) {
    Write-Output "Resolving $f"
    # Prefer 'theirs' (incoming branch) version
    git checkout --theirs -- -- "$f" 2>$null

    if (Test-Path $f) {
        git add -- "$f"
    }
    else {
        # If file was deleted in 'theirs', stage the deletion
        git rm --cached -f -- "$f" 2>$null
        git rm -f -- "$f" 2>$null || Write-Output "Removed $f"
    }
}

# Commit merge result
git commit -m 'Finish merge: accept incoming for Markdown and generated cmdlets; keep SetDataverseRecord fixes'

Write-Output 'Merge resolution committed.'
git status -sb
git log --oneline --decorate -n 5
