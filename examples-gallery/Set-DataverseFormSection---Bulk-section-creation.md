---
title: "Set-DataverseFormSection - Bulk section creation"
tags: ['Metadata']
source: "Set-DataverseFormSection.md"
---
Creates multiple sections in specific columns with error handling.

```powershell
$sections = @(
    @{ Name = 'BasicInfo'; Label = 'Basic Information'; Columns = 2; ColumnIndex = 0 }
    @{ Name = 'ContactDetails'; Label = 'Contact Details'; Columns = 1; ColumnIndex = 1 }
    @{ Name = 'Preferences'; Label = 'User Preferences'; Columns = 2; ColumnIndex = 0 }
)

foreach ($section in $sections) {
    try {
        $sectionId = Set-DataverseFormSection -FormId $formId -TabName 'General' `
           -Name $section.Name -Label $section.Label -Columns $section.Columns `
           -ColumnIndex $section.ColumnIndex -PassThru
        Write-Host "Created section: $($section.Name) with ID: $sectionId"
    }
    catch {
        Write-Warning "Failed to create section $($section.Name): $($_.Exception.Message)"
    }
}

```

