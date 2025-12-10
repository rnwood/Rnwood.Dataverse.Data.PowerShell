---
title: "Set-DataverseOptionSetMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Creates or updates global option sets.

```powershell
# Create a new global option set
$options = @(
    @{Value=100; Label='Bronze'; Description='Bronze tier'}
    @{Value=200; Label='Silver'; Description='Silver tier'}
    @{Value=300; Label='Gold'; Description='Gold tier'}
    @{Value=400; Label='Platinum'; Description='Platinum tier'}
)

Set-DataverseOptionSetMetadata -Name new_customertier `
   -DisplayName "Customer Tier" `
   -Description "Customer membership tiers" `
   -Options $options `
   -PassThru

# Update an existing global option set
$updatedOptions = @(
    @{Value=100; Label='Bronze Level'}
    @{Value=200; Label='Silver Level'}
    @{Value=300; Label='Gold Level'}
    @{Value=400; Label='Platinum Level'}
    @{Value=500; Label='Diamond Level'} # New option
)

Set-DataverseOptionSetMetadata -Name new_customertier `
   -DisplayName "Customer Tier (Updated)" `
   -Options $updatedOptions `
   -Force `
   -PassThru

```
