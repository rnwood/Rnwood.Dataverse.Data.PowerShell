# Copilot Studio Management Cmdlets

This module provides PowerShell cmdlets for managing Microsoft Copilot Studio bots and components via Dataverse. These cmdlets enable you to automate bot management, component manipulation, and conversation analysis directly from PowerShell.

## Overview

The module includes cmdlets for:
- **Bot Management**: List and retrieve Copilot Studio bots
- **Bot Component Management**: Manage topics, skills, and other bot components
- **Conversation Transcripts**: Access and analyze conversation history
- **Component Operations**: Clone and compare bot components

## Prerequisites

- PowerShell 5.1 or PowerShell Core 7+
- Access to a Dataverse environment with Copilot Studio bots
- Appropriate permissions to read/write bot data

## Installation

The cmdlets are part of the `Rnwood.Dataverse.Data.PowerShell` module:

```powershell
Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser
```

## Getting Started

### Connect to Dataverse

First, establish a connection to your Dataverse environment:

```powershell
$conn = Get-DataverseConnection `
    -Url "https://yourorg.crm.dynamics.com" `
    -ClientId "your-client-id" `
    -ClientSecret "your-client-secret" `
    -SetAsDefault
```

## Available Cmdlets

### Get-DataverseBot

Lists and retrieves Copilot Studio bots from Dataverse.

**Parameters:**
- `-BotId` - Filter by bot ID (GUID)
- `-Name` - Filter by bot name (exact match)
- `-SchemaName` - Filter by bot schema name (exact match)
- `-Top` - Maximum number of bots to return

**Examples:**

```powershell
# List all bots
Get-DataverseBot

# Get a specific bot by ID
Get-DataverseBot -BotId "ff636ba1-4764-4824-80a4-c469868f2e96"

# Get a bot by name
Get-DataverseBot -Name "My Copilot"

# Get bots with a limit
Get-DataverseBot -Top 10
```

### Get-DataverseBotComponent

Lists and retrieves bot components (topics, skills, actions, etc.) from Dataverse.

**Parameters:**
- `-BotComponentId` - Filter by component ID (GUID)
- `-Name` - Filter by component name
- `-SchemaName` - Filter by component schema name
- `-ParentBotId` - Filter by parent bot ID
- `-ComponentType` - Filter by component type (10=Topic, 11=Skill, etc.)
- `-Category` - Filter by category
- `-Top` - Maximum number of components to return

**Examples:**

```powershell
# List all bot components
Get-DataverseBotComponent -Top 20

# Get components for a specific bot
$bot = Get-DataverseBot -Name "My Copilot"
Get-DataverseBotComponent -ParentBotId $bot.botid

# Get components by type (topics)
Get-DataverseBotComponent -ComponentType 10 -Top 10

# Get a specific component by ID
Get-DataverseBotComponent -BotComponentId "675b628c-c37a-4cde-bc1e-0030b0c6363e"
```

### Get-DataverseConversationTranscript

Lists and retrieves conversation transcripts from Dataverse.

**Parameters:**
- `-ConversationTranscriptId` - Filter by transcript ID (GUID)
- `-BotId` - Filter by bot ID
- `-ConversationId` - Filter by conversation ID
- `-StartDate` - Filter conversations from this date
- `-EndDate` - Filter conversations up to this date
- `-Top` - Maximum number of transcripts to return

**Examples:**

```powershell
# List recent transcripts
Get-DataverseConversationTranscript -Top 10

# Get transcripts for a specific bot
$bot = Get-DataverseBot -Name "My Copilot"
Get-DataverseConversationTranscript -BotId $bot.botid -Top 20

# Get transcripts in a date range
$startDate = (Get-Date).AddDays(-7)
$endDate = Get-Date
Get-DataverseConversationTranscript -StartDate $startDate -EndDate $endDate

# Get a specific transcript
Get-DataverseConversationTranscript -ConversationTranscriptId "12345678-1234-1234-1234-123456789012"
```

### Copy-DataverseBotComponent

Clones a bot component to create a new component with a different name.

**Parameters:**
- `-BotComponentId` - Source component ID to copy (required)
- `-NewName` - Name for the new component (required)
- `-NewSchemaName` - Custom schema name (optional, auto-generated if not specified)
- `-NewDescription` - Description for the new component (optional)
- `-PassThru` - Return the newly created component
- `-WhatIf` - Show what would happen without actually copying
- `-Confirm` - Prompt for confirmation before copying

**Examples:**

```powershell
# Clone a component
$sourceComponent = Get-DataverseBotComponent -Name "Greeting Topic"
Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Custom Greeting Topic" `
    -NewDescription "Customized greeting for VIP customers"

# Clone and return the new component
$newComponent = Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Test Greeting" `
    -PassThru

# Preview the copy operation
Copy-DataverseBotComponent `
    -BotComponentId $sourceComponent.botcomponentid `
    -NewName "Test Copy" `
    -WhatIf
```

### Compare-DataverseBotComponent

Compares two bot components and shows their differences.

**Parameters:**
- `-ComponentId1` - First component ID (required)
- `-ComponentId2` - Second component ID (required)
- `-Attributes` - Specific attributes to compare (optional)

**Examples:**

```powershell
# Compare two components
$component1 = Get-DataverseBotComponent -Name "Greeting (en-US)"
$component2 = Get-DataverseBotComponent -Name "Greeting (es-ES)"
$differences = Compare-DataverseBotComponent `
    -ComponentId1 $component1.botcomponentid `
    -ComponentId2 $component2.botcomponentid

# Show only different attributes
$differences | Where-Object { $_.IsDifferent } | Format-Table -AutoSize

# Compare specific attributes
Compare-DataverseBotComponent `
    -ComponentId1 $component1.botcomponentid `
    -ComponentId2 $component2.botcomponentid `
    -Attributes "name", "data", "language"
```

## Working with Bots and Components

### Creating a New Bot

Use the standard `Set-DataverseRecord` cmdlet:

```powershell
$newBot = Set-DataverseRecord -TableName bot -InputObject @{
    name = "My New Bot"
    schemaname = "my_new_bot"
    language = 1033  # English (US)
    configuration = '{"$kind": "BotConfiguration"}'
} -CreateOnly -PassThru
```

### Updating a Bot Component

```powershell
# Get the component
$component = Get-DataverseBotComponent -Name "Greeting Topic"

# Update it
Set-DataverseRecord -TableName botcomponent -Id $component.botcomponentid -InputObject @{
    description = "Updated greeting message"
    data = "kind: AdaptiveDialog`nbeginDialog:`n  kind: SendActivity`n  activity: Hello!"
}
```

### Deleting a Bot Component

```powershell
# Get the component
$component = Get-DataverseBotComponent -Name "Old Topic"

# Delete it
Remove-DataverseRecord -TableName botcomponent -Id $component.botcomponentid
```

## Advanced Scenarios

### Bulk Component Operations

```powershell
# Get all components of a specific type
$topics = Get-DataverseBotComponent -ComponentType 10

# Clone each topic with a prefix
foreach ($topic in $topics) {
    Copy-DataverseBotComponent `
        -BotComponentId $topic.botcomponentid `
        -NewName "Backup_$($topic.name)" `
        -NewDescription "Backup of $($topic.name)"
}
```

### Compare Multiple Components

```powershell
# Get all language variants of a topic
$components = Get-DataverseBotComponent | Where-Object { $_.name -like "Greeting*" }

# Compare each pair
for ($i = 0; $i -lt $components.Count - 1; $i++) {
    for ($j = $i + 1; $j -lt $components.Count; $j++) {
        Write-Host "Comparing $($components[$i].name) with $($components[$j].name)"
        $diffs = Compare-DataverseBotComponent `
            -ComponentId1 $components[$i].botcomponentid `
            -ComponentId2 $components[$j].botcomponentid
        $diffCount = ($diffs | Where-Object { $_.IsDifferent }).Count
        Write-Host "  Found $diffCount differences"
    }
}
```

### Analyze Conversation Patterns

```powershell
# Get recent conversations
$transcripts = Get-DataverseConversationTranscript -Top 100

# Group by bot
$transcripts | Group-Object { $_.bot.name } | 
    Select-Object Name, Count | 
    Format-Table -AutoSize
```

## Integration with PiStudio-CLI

These cmdlets provide similar functionality to the [PiStudio-CLI](https://github.com/anthonyrhopkins/PiStudio-CLI) project but are implemented as native PowerShell cmdlets that integrate seamlessly with the Dataverse SDK. Key advantages include:

- **Type Safety**: Strong typing and IntelliSense support
- **Pipeline Support**: Works with PowerShell pipelines
- **Cross-Platform**: Runs on Windows, Linux, and macOS
- **Integration**: Works with other Dataverse cmdlets in this module
- **Error Handling**: Proper PowerShell error handling and verbose output

## Comparison with PiStudio-CLI Features

| PiStudio-CLI Feature | PowerShell Equivalent |
|---------------------|----------------------|
| `pistudio copilot list` | `Get-DataverseBot` |
| `pistudio agents` | `Get-DataverseBotComponent` |
| `pistudio agents clone` | `Copy-DataverseBotComponent` |
| `pistudio agents diff` | `Compare-DataverseBotComponent` |
| `pistudio convs` | `Get-DataverseConversationTranscript` |
| Bot CRUD operations | `Set-DataverseRecord` / `Remove-DataverseRecord` with `-TableName bot` |
| Component CRUD | `Set-DataverseRecord` / `Remove-DataverseRecord` with `-TableName botcomponent` |

## Notes

- The `conversationtranscript` table may be empty if no conversations have occurred or if transcripts are not being stored.
- Component types: 10=Topic, 11=Skill, and other values represent different component types in Copilot Studio.
- Schema names must be unique and follow naming conventions (alphanumeric and underscore only).
- When copying components, unique schema names are automatically generated using timestamps.

## See Also

- [Get-DataverseConnection](../docs/Get-DataverseConnection.md) - Connection management
- [Set-DataverseRecord](../docs/Set-DataverseRecord.md) - Create/update records
- [Remove-DataverseRecord](../docs/Remove-DataverseRecord.md) - Delete records
- [Get-DataverseRecord](../docs/Get-DataverseRecord.md) - Query records

## License

This module is part of Rnwood.Dataverse.Data.PowerShell and is licensed under the same terms.
