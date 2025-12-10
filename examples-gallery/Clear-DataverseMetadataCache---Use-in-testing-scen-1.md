---
title: "Clear-DataverseMetadataCache - Use in testing scenarios"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Uses cache clearing in Pester tests for isolation.

```powershell
Describe "Metadata Tests" {
    BeforeEach {
        # Clear cache before each test
        Clear-DataverseMetadataCache
    }
    
    It "Should retrieve entity metadata" {
        $metadata = Get-DataverseEntityMetadata -EntityName account
        $metadata | Should -Not -BeNullOrEmpty
    }
}

```
