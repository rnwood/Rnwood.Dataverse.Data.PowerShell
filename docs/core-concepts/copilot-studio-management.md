# Copilot Studio Management

PowerShell cmdlets for managing Microsoft Copilot Studio bots and components via Dataverse. These cmdlets provide full CRUD (Create, Read, Update, Delete) operations for bots, bot components (topics, skills, actions), conversation transcript access, and complete bot backup/restore functionality.

## Overview

The Copilot Studio management cmdlets enable you to:
- **Create and manage bots** - Set up new bots or update existing ones
- **Manage bot components** - Create, update, and delete topics, skills, and other bot components
- **Clone components** - Duplicate existing components with custom names
- **Access conversation history** - Retrieve and analyze conversation transcripts
- **Backup and restore bots** - Export complete bots with all components and restore them

## Prerequisites

- PowerShell 5.1 or PowerShell Core 7+
- Access to a Dataverse environment with Copilot Studio
- Appropriate permissions to read/write bot data

## Connection

First, establish a connection to your Dataverse environment:

```powershell
$conn = Get-DataverseConnection `
    -Url "https://yourorg.crm.dynamics.com" `
    -ClientId "your-client-id" `
    -ClientSecret "your-client-secret" `
    -SetAsDefault
```

## Bot Management Cmdlets

### Get-DataverseBot

Retrieves Copilot Studio bots from Dataverse.

**Parameters:**
- `-BotId` (Guid) - Filter by bot ID
- `-Name` (String) - Filter by bot name (exact match)
- `-SchemaName` (String) - Filter by bot schema name (exact match)
- `-Top` (Int) - Maximum number of bots to return

**Examples:**

```powershell
# List all bots
Get-DataverseBot

# Get a specific bot by ID
Get-DataverseBot -BotId "ff636ba1-4764-4824-80a4-c469868f2e96"

# Get a bot by name
Get-DataverseBot -Name "Customer Service Bot"

# Get bots with a limit
Get-DataverseBot -Top 10
```

### Set-DataverseBot

Creates a new bot or updates an existing bot.

**Parameters:**
- `-BotId` (Guid) - Bot ID for updates (omit for new bot)
- `-Name` (String, Required) - Bot name
- `-SchemaName` (String) - Schema name (required for new bots)
- `-Language` (Int) - Language code (default: 1033 for English US)
- `-Configuration` (String) - Bot configuration JSON
- `-AuthenticationMode` (Int) - Authentication mode (0=None, 1=Generic, 2=Integrated)
- `-RuntimeProvider` (Int) - Runtime provider (0=PowerVirtualAgents)
- `-Template` (String) - Template name
- `-PassThru` (Switch) - Return the bot after operation

**Examples:**

```powershell
# Create a new bot
$newBot = Set-DataverseBot `
    -Name "Customer Service Bot" `
    -SchemaName "customer_service_bot" `
    -Language 1033 `
    -Configuration '{"$kind": "BotConfiguration"}' `
    -PassThru

# Update an existing bot
Set-DataverseBot `
    -BotId $newBot.botid `
    -Name "Customer Service Bot v2" `
    -Configuration '{"$kind": "BotConfiguration", "settings": {}}' `
    -PassThru
```

### Remove-DataverseBot

Deletes a bot from Dataverse.

**Parameters:**
- `-BotId` (Guid, Required) - Bot ID to delete

**Examples:**

```powershell
# Delete a bot (with confirmation prompt)
Remove-DataverseBot -BotId "ff636ba1-4764-4824-80a4-c469868f2e96"

# Delete without confirmation
Remove-DataverseBot -BotId $bot.botid -Confirm:$false

# Preview deletion with WhatIf
Remove-DataverseBot -BotId $bot.botid -WhatIf
```

## Bot Component Management Cmdlets

### Get-DataverseBotComponent

Retrieves bot components (topics, skills, actions) from Dataverse.

**Parameters:**
- `-BotComponentId` (Guid) - Filter by component ID
- `-Name` (String) - Filter by component name
- `-SchemaName` (String) - Filter by component schema name
- `-ParentBotId` (Guid) - Filter by parent bot ID
- `-ComponentType` (Int) - Filter by component type (10=Topic, 11=Skill, etc.)
- `-Category` (String) - Filter by category
- `-Top` (Int) - Maximum number of components to return

**Examples:**

```powershell
# List all bot components
Get-DataverseBotComponent -Top 20

# Get components for a specific bot
$bot = Get-DataverseBot -Name "Customer Service Bot"
Get-DataverseBotComponent -ParentBotId $bot.botid

# Get components by type (topics)
Get-DataverseBotComponent -ComponentType 10 -Top 10

# Get a specific component by ID
Get-DataverseBotComponent -BotComponentId "675b628c-c37a-4cde-bc1e-0030b0c6363e"
```

### Set-DataverseBotComponent

Creates a new component or updates an existing component.

**Parameters:**
- `-BotComponentId` (Guid) - Component ID for updates (omit for new component)
- `-Name` (String, Required) - Component name
- `-SchemaName` (String) - Schema name (required for new components)
- `-ParentBotId` (Guid) - Parent bot ID (required for new components)
- `-ComponentType` (Int) - Component type (required for new components: 10=Topic, 11=Skill)
- `-Data` (String) - Component data (e.g., YAML content for topics)
- `-Content` (String) - Component content
- `-Description` (String) - Component description
- `-Category` (String) - Component category
- `-Language` (Int) - Language code
- `-HelpLink` (String) - Help link URL
- `-PassThru` (Switch) - Return the component after operation

**Examples:**

```powershell
# Create a new topic
$bot = Get-DataverseBot -Name "Customer Service Bot"
$newTopic = Set-DataverseBotComponent `
    -Name "Greeting Topic" `
    -SchemaName "customer_service_bot.topic.greeting" `
    -ParentBotId $bot.botid `
    -ComponentType 10 `
    -Data "kind: AdaptiveDialog`nbeginDialog:`n  kind: SendActivity`n  activity: Hello!" `
    -Description "Greets customers" `
    -PassThru

# Update an existing component
Set-DataverseBotComponent `
    -BotComponentId $newTopic.botcomponentid `
    -Name "Greeting Topic (Updated)" `
    -Description "Updated greeting message" `
    -Data "kind: AdaptiveDialog`nbeginDialog:`n  kind: SendActivity`n  activity: Welcome!" `
    -PassThru
```

### Remove-DataverseBotComponent

Deletes a bot component from Dataverse.

**Parameters:**
- `-BotComponentId` (Guid, Required) - Component ID to delete

**Examples:**

```powershell
# Delete a component (with confirmation prompt)
Remove-DataverseBotComponent -BotComponentId "675b628c-c37a-4cde-bc1e-0030b0c6363e"

# Delete without confirmation
Remove-DataverseBotComponent -BotComponentId $component.botcomponentid -Confirm:$false

# Preview deletion with WhatIf
Remove-DataverseBotComponent -BotComponentId $component.botcomponentid -WhatIf
```

### Copy-DataverseBotComponent

Clones an existing bot component to create a new component with a different name.

**Parameters:**
- `-BotComponentId` (Guid, Required) - Source component ID to copy
- `-NewName` (String, Required) - Name for the new component
- `-NewSchemaName` (String) - Custom schema name (auto-generated if not specified)
- `-NewDescription` (String) - Description for the new component
- `-PassThru` (Switch) - Return the newly created component

**Examples:**

```powershell
# Clone a component
$sourceComponent = Get-DataverseBotComponent -Name "Greeting Topic"
$copy = Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Greeting Topic - Spanish" `
    -NewDescription "Spanish greeting" `
    -PassThru

# Clone with custom schema name
Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Custom Greeting" `
    -NewSchemaName "bot.topic.custom_greeting" `
    -PassThru

# Preview the copy operation
Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Test Copy" `
    -WhatIf
```

## Conversation Transcript Management

### Get-DataverseConversationTranscript

Retrieves conversation transcripts from Dataverse.

**Parameters:**
- `-ConversationTranscriptId` (Guid) - Filter by transcript ID
- `-BotId` (Guid) - Filter by bot ID
- `-ConversationId` (String) - Filter by conversation ID
- `-StartDate` (DateTime) - Filter conversations from this date
- `-EndDate` (DateTime) - Filter conversations up to this date
- `-Top` (Int) - Maximum number of transcripts to return

**Examples:**

```powershell
# List recent transcripts
Get-DataverseConversationTranscript -Top 10

# Get transcripts for a specific bot
$bot = Get-DataverseBot -Name "Customer Service Bot"
Get-DataverseConversationTranscript -BotId $bot.botid -Top 20

# Get transcripts in a date range
$startDate = (Get-Date).AddDays(-7)
$endDate = Get-Date
Get-DataverseConversationTranscript -StartDate $startDate -EndDate $endDate

# Get a specific transcript
Get-DataverseConversationTranscript -ConversationTranscriptId "12345678-1234-1234-1234-123456789012"
```

## Bot Backup and Restore

### Export-DataverseBot

Exports a complete bot with all its components to a backup directory.

**Parameters:**
- `-BotId` (Guid, Required) - Bot ID to export
- `-OutputPath` (String) - Output directory (auto-generates timestamped folder if not specified)
- `-PassThru` (Switch) - Return export information

**Backup Format:**
The export creates a structured directory containing:
- `manifest.json` - Export metadata (version, date, bot info, component count)
- `bot_config.json` - Bot configuration and settings
- `[component].yaml` - Component data files (YAML format)
- `[component].meta.json` - Component metadata files (name, schema, type, description)

**Examples:**

```powershell
# Export to auto-generated timestamped folder
$export = Export-DataverseBot -BotId $bot.botid -PassThru
Write-Host "Exported to: $($export.OutputPath)"
Write-Host "Components: $($export.ComponentCount)"

# Export to specific directory
Export-DataverseBot -BotId $bot.botid -OutputPath "./backups/my_bot_backup"

# Preview export with WhatIf
Export-DataverseBot -BotId $bot.botid -WhatIf
```

### Import-DataverseBot

Imports a bot from a backup directory created by Export-DataverseBot.

**Parameters:**
- `-Path` (String, Required) - Path to backup directory
- `-Name` (String) - New name for bot (uses backup name if not specified)
- `-SchemaName` (String) - New schema name (uses backup schema name if not specified)
- `-TargetBotId` (Guid) - Existing bot ID to restore components to (creates new bot if not specified)
- `-Overwrite` (Switch) - Overwrite existing components with matching schema names
- `-PassThru` (Switch) - Return import information

**Examples:**

```powershell
# Import as new bot with custom name
$import = Import-DataverseBot `
    -Path "./backups/my_bot_backup" `
    -Name "Restored Bot" `
    -SchemaName "restored_bot" `
    -PassThru
Write-Host "Created bot: $($import.BotId)"
Write-Host "Imported components: $($import.ComponentsImported)"

# Restore components to existing bot
Import-DataverseBot `
    -Path "./backups/my_bot_backup" `
    -TargetBotId $existingBot.botid `
    -Overwrite

# Preview import with WhatIf
Import-DataverseBot `
    -Path "./backups/my_bot_backup" `
    -Name "Test Import" `
    -WhatIf
```

### Complete Backup and Restore Workflow

```powershell
# 1. Export existing bot
$bot = Get-DataverseBot -Name "Production Bot"
$export = Export-DataverseBot -BotId $bot.botid -OutputPath "./backups/prod_bot" -PassThru
Write-Host "Backed up $($export.ComponentCount) components to $($export.OutputPath)"

# 2. Later, restore to new environment
$import = Import-DataverseBot `
    -Path "./backups/prod_bot" `
    -Name "Development Bot" `
    -SchemaName "dev_bot" `
    -PassThru
Write-Host "Restored $($import.ComponentsImported) components to new bot $($import.BotId)"

# 3. Verify restored bot
$restoredBot = Get-DataverseBot -BotId $import.BotId
$restoredComponents = Get-DataverseBotComponent -ParentBotId $restoredBot.botid
Write-Host "Verified: $($restoredComponents.Count) components in restored bot"
```

## Common Scenarios

### Complete Bot Setup Workflow

```powershell
# 1. Create a new bot
$newBot = Set-DataverseBot `
    -Name "Support Bot" `
    -SchemaName "support_bot" `
    -Language 1033 `
    -PassThru

# 2. Create a greeting topic
$greetingTopic = Set-DataverseBotComponent `
    -Name "Greeting" `
    -SchemaName "support_bot.topic.greeting" `
    -ParentBotId $newBot.botid `
    -ComponentType 10 `
    -Data "kind: AdaptiveDialog`nbeginDialog:`n  kind: SendActivity`n  activity: Hello! How can I help you?" `
    -PassThru

# 3. Create a help topic
$helpTopic = Set-DataverseBotComponent `
    -Name "Help" `
    -SchemaName "support_bot.topic.help" `
    -ParentBotId $newBot.botid `
    -ComponentType 10 `
    -Data "kind: AdaptiveDialog`nbeginDialog:`n  kind: SendActivity`n  activity: I can help you with..." `
    -PassThru

Write-Host "Bot created with $($newBot.botid)"
Write-Host "Created $(($greetingTopic, $helpTopic).Count) topics"
```

### Bulk Component Operations

```powershell
# Get all topics for a bot
$bot = Get-DataverseBot -Name "Support Bot"
$topics = Get-DataverseBotComponent -ParentBotId $bot.botid -ComponentType 10

# Clone each topic with a backup prefix
foreach ($topic in $topics) {
    Copy-DataverseBotComponent `
        -BotComponentId $topic.botcomponentid `
        -NewName "Backup_$($topic.name)" `
        -NewDescription "Backup of $($topic.name)"
}

Write-Host "Created $($topics.Count) backup copies"
```

### Update Multiple Components

```powershell
# Update description for all topics
$bot = Get-DataverseBot -Name "Support Bot"
$topics = Get-DataverseBotComponent -ParentBotId $bot.botid -ComponentType 10

foreach ($topic in $topics) {
    Set-DataverseBotComponent `
        -BotComponentId $topic.botcomponentid `
        -Name $topic.name `
        -Description "Updated: $(Get-Date -Format 'yyyy-MM-dd')"
}
```

### Analyze Conversation Patterns

```powershell
# Get recent conversations for analysis
$bot = Get-DataverseBot -Name "Support Bot"
$transcripts = Get-DataverseConversationTranscript -BotId $bot.botid -Top 100

# Group by date
$transcripts | 
    Group-Object { $_.createdon.Date } | 
    Select-Object Name, Count | 
    Sort-Object Name -Descending |
    Format-Table -AutoSize
```

## Component Types

Common component type values:
- `10` - Topic
- `11` - Skill
- Other values represent different component types in Copilot Studio

## Language Codes

Common language codes:
- `1033` - English (United States)
- `1031` - German (Germany)
- `1036` - French (France)
- `1040` - Italian (Italy)
- `1034` - Spanish (Spain)
- `1041` - Japanese (Japan)
- `2052` - Chinese (China)

## Notes

- Schema names must be unique and follow naming conventions (alphanumeric and underscore only)
- When copying components, unique schema names are automatically generated using timestamps
- The `conversationtranscript` table may be empty if no conversations have occurred
- All cmdlets support the standard `-Connection` parameter (uses default connection if not specified)
- All cmdlets support `-WhatIf` and `-Confirm` for safety

## See Also

- [Get-DataverseConnection](Get-DataverseConnection.md) - Connection management
- [Set-DataverseRecord](Set-DataverseRecord.md) - Generic record create/update
- [Remove-DataverseRecord](Remove-DataverseRecord.md) - Generic record delete
- [Get-DataverseRecord](Get-DataverseRecord.md) - Generic record query
