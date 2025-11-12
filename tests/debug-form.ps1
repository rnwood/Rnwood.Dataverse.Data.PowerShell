. "$PSScriptRoot\Common.ps1"

# Simple test to debug form XML handling
$connection = getMockConnection -Entities @("systemform")

# Create a test form
$form = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
$formId = [System.Guid]::NewGuid()
$form["systemformid"] = $form.Id = $formId
$form["name"] = "Test Form"
$form["objecttypecode"] = "contact"
$form["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)

# Set simple form XML WITH SystemForm wrapper  
$simpleFormXml = '<SystemForm><form><tabs><tab name="general"><columns><column><sections><section name="test"><rows><row><cell><control id="test" datafieldname="firstname" /></cell></row></rows></section></sections></column></columns></tab></tabs></form></SystemForm>'
$form["formxml"] = [string]$simpleFormXml

Write-Host "Creating form with FormXML:"
Write-Host $form["formxml"]

try {
    $connection.Create($form)
    Write-Host "Form created successfully"
    
    # Try to retrieve it
    $retrievedForm = Get-DataverseRecord -Connection $connection -TableName "systemform" -Id $formId
    Write-Host "Retrieved form:"
    Write-Host "FormXML: $($retrievedForm.formxml)"
    
    # Try to call the cmdlet
    Write-Host "Calling Get-DataverseFormControl..."
    $result = Get-DataverseFormControl -Connection $connection -FormId $formId
    Write-Host "Success! Got $($result.Count) controls"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.ScriptStackTrace)"
}