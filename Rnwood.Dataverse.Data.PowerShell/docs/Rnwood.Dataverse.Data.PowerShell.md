---
Module Name: Rnwood.Dataverse.Data.PowerShell
Module Guid: {{ Update Module Guid }}
Download Help Link: {{ Update Download Link }}
Help Version: {{ Update Help Version }}
Locale: {{ Update Locale }}
---

# Rnwood.Dataverse.Data.PowerShell Module
## Description
Cross-platform PowerShell module for Microsoft Dataverse data manipulation and form management. This module provides comprehensive cmdlets for CRUD operations on Dataverse records, form structure manipulation, and metadata management. Supports both PowerShell Desktop (5.1+) and PowerShell Core (7+) on Windows, Linux, and macOS.

## Rnwood.Dataverse.Data.PowerShell Cmdlets
### [Clear-DataverseMetadataCache](Clear-DataverseMetadataCache.md)
Clears the metadata cache for the Dataverse connection.

### [Compare-DataverseSolutionComponents](Compare-DataverseSolutionComponents.md)
Compares the components of two solutions in the Dataverse environment.

### [Export-DataverseSolution](Export-DataverseSolution.md)
Exports a solution from the Dataverse environment.

### [Get-DataverseAppModule](Get-DataverseAppModule.md)
Retrieves information about app modules in the Dataverse environment.

### [Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)
Retrieves information about components of an app module in the Dataverse environment.

### [Get-DataverseAttributeMetadata](Get-DataverseAttributeMetadata.md)
Retrieves metadata for attributes (columns) in Dataverse entities.

### [Get-DataverseConnection](Get-DataverseConnection.md)
Gets a connection to a Dataverse environment either interactively or silently and returns it.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

### [Get-DataverseConnectionReference](Get-DataverseConnectionReference.md)
Retrieves information about connection references in the Dataverse environment.

### [Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)
Retrieves metadata for entities (tables) in the Dataverse environment.

### [Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
Retrieves information about environment variable definitions in the Dataverse environment.

### [Get-DataverseEnvironmentVariableValue](Get-DataverseEnvironmentVariableValue.md)
Retrieves the value of an environment variable in the Dataverse environment.

### [Get-DataverseForm](Get-DataverseForm.md)
Retrieves Dataverse form metadata, including form XML structure and properties.

### [Get-DataverseFormControl](Get-DataverseFormControl.md)
Retrieves control information from a Dataverse form section, including properties and configuration.

### [Get-DataverseFormSection](Get-DataverseFormSection.md)
Retrieves section information from a Dataverse form tab, including controls and layout properties.

### [Get-DataverseFormTab](Get-DataverseFormTab.md)
Retrieves tab information from a Dataverse form, including layout and section details.

### [Get-DataverseOptionSetMetadata](Get-DataverseOptionSetMetadata.md)
Retrieves metadata for option sets (choices) in the Dataverse environment.

### [Get-DataverseRecord](Get-DataverseRecord.md)
Retrieves records from Dataverse tables using a variety of strategies to specify what should be retrieved.

### [Get-DataverseRecordsFolder](Get-DataverseRecordsFolder.md)
Reads a folder of JSON files written out by `Set-DataverseRecordFolder` and converts back into a stream of PS objects.
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

### [Get-DataverseRelationshipMetadata](Get-DataverseRelationshipMetadata.md)
Retrieves metadata for relationships between entities in the Dataverse environment.

### [Get-DataverseSolution](Get-DataverseSolution.md)
Retrieves information about solutions in the Dataverse environment.

### [Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
Retrieves information about components of a solution in the Dataverse environment.

### [Get-DataverseSolutionFile](Get-DataverseSolutionFile.md)
Retrieves solution files from the Dataverse environment.

### [Get-DataverseSolutionFileComponent](Get-DataverseSolutionFileComponent.md)
Retrieves information about components of a solution file in the Dataverse environment.

### [Get-DataverseSitemap](Get-DataverseSitemap.md)
Retrieves the sitemap configuration for the Dataverse environment.

### [Get-DataverseSitemapEntry](Get-DataverseSitemapEntry.md)
Retrieves information about sitemap entries in the Dataverse environment.

### [Get-DataverseView](Get-DataverseView.md)
Retrieves information about views (saved queries) in the Dataverse environment.

### [Get-DataverseWhoAmI](Get-DataverseWhoAmI.md)
Retrieves details about the current Dataverse user and organization specified by the connection provided.

### [Import-DataverseSolution](Import-DataverseSolution.md)
Imports a solution into the Dataverse environment.

### [Invoke-DataverseRequest](Invoke-DataverseRequest.md)
Invokes an arbitrary Dataverse request and returns the response.

### [Invoke-DataverseSql](Invoke-DataverseSql.md)
Invokes a Dataverse SQL query using Sql4Cds and writes any resulting rows to the pipeline.

### [Invoke-DataverseParallel](Invoke-DataverseParallel.md)
Processes input objects in parallel using chunked batches with cloned Dataverse connections.

### [Publish-DataverseCustomizations](Publish-DataverseCustomizations.md)
Publishes customizations to the Dataverse environment.

### [Remove-DataverseAppModule](Remove-DataverseAppModule.md)
Removes an app module from the Dataverse environment.

### [Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)
Removes a component from an app module in the Dataverse environment.

### [Remove-DataverseAttributeMetadata](Remove-DataverseAttributeMetadata.md)
Removes metadata for attributes (columns) from Dataverse entities.

### [Remove-DataverseConnectionReference](Remove-DataverseConnectionReference.md)
Removes a connection reference from the Dataverse environment.

### [Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)
Removes metadata for entities (tables) from the Dataverse environment.

### [Remove-DataverseEnvironmentVariableDefinition](Remove-DataverseEnvironmentVariableDefinition.md)
Removes an environment variable definition from the Dataverse environment.

### [Remove-DataverseEnvironmentVariableValue](Remove-DataverseEnvironmentVariableValue.md)
Removes the value of an environment variable in the Dataverse environment.

### [Remove-DataverseForm](Remove-DataverseForm.md)
Removes a Dataverse form from the system, with safety checks and backup recommendations.

### [Remove-DataverseFormControl](Remove-DataverseFormControl.md)
Removes a control from a Dataverse form section.

### [Remove-DataverseFormSection](Remove-DataverseFormSection.md)
Removes a section from a Dataverse form tab, including all contained controls.

### [Remove-DataverseFormTab](Remove-DataverseFormTab.md)
Removes a tab from a Dataverse form, including all contained sections and controls.

### [Remove-DataverseRecord](Remove-DataverseRecord.md)
Deletes an existing Dataverse record, including M:M association records.

### [Remove-DataverseRelationshipMetadata](Remove-DataverseRelationshipMetadata.md)
Removes metadata for relationships between entities from the Dataverse environment.

### [Remove-DataverseSolution](Remove-DataverseSolution.md)
Removes a solution from the Dataverse environment.

### [Remove-DataverseSitemap](Remove-DataverseSitemap.md)
Removes the sitemap configuration from the Dataverse environment.

### [Remove-DataverseSitemapEntry](Remove-DataverseSitemapEntry.md)
Removes a sitemap entry from the Dataverse environment.

### [Remove-DataverseView](Remove-DataverseView.md)
Removes a view (saved query) from the Dataverse environment.

### [Set-DataverseAppModule](Set-DataverseAppModule.md)
Creates or updates an app module in the Dataverse environment.

### [Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)
Creates or updates a component of an app module in the Dataverse environment.

### [Set-DataverseAttributeMetadata](Set-DataverseAttributeMetadata.md)
Creates or updates metadata for attributes (columns) in Dataverse entities.

### [Set-DataverseConnectionAsDefault](Set-DataverseConnectionAsDefault.md)
Sets the specified connection as the default connection for the session.

### [Set-DataverseConnectionReference](Set-DataverseConnectionReference.md)
Creates or updates a connection reference in the Dataverse environment.

### [Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)
Creates or updates metadata for entities (tables) in the Dataverse environment.

### [Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
Creates or updates an environment variable definition in the Dataverse environment.

### [Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
Sets the value of an environment variable in the Dataverse environment.

### [Set-DataverseForm](Set-DataverseForm.md)
Creates or updates Dataverse form properties and metadata.

### [Set-DataverseFormControl](Set-DataverseFormControl.md)
Creates or updates controls within Dataverse form sections, supporting all control types and properties.

### [Set-DataverseFormSection](Set-DataverseFormSection.md)
Creates or updates sections within Dataverse form tabs, including layout and control organization.

### [Set-DataverseFormTab](Set-DataverseFormTab.md)
Creates or updates tabs in a Dataverse form, including layout and positioning.

### [Set-DataverseOptionSetMetadata](Set-DataverseOptionSetMetadata.md)
Creates or updates metadata for option sets (choices) in the Dataverse environment.

### [Set-DataverseRecord](Set-DataverseRecord.md)
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

### [Set-DataverseRecordsFolder](Set-DataverseRecordsFolder.md)
Writes a list of Dataverse records to a folder of JSON files.

### [Set-DataverseRelationshipMetadata](Set-DataverseRelationshipMetadata.md)
Creates or updates metadata for relationships between entities in the Dataverse environment.

### [Set-DataverseSitemap](Set-DataverseSitemap.md)
Creates or updates the sitemap configuration for the Dataverse environment.

### [Set-DataverseSitemapEntry](Set-DataverseSitemapEntry.md)
Creates or updates a sitemap entry in the Dataverse environment.

### [Set-DataverseSolution](Set-DataverseSolution.md)
Creates or updates a solution in the Dataverse environment.

### [Set-DataverseView](Set-DataverseView.md)
Creates or updates a view (saved query) in the Dataverse environment.

