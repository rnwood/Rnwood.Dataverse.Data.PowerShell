# Script Gallery Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     XrmToolbox Plugin UI                                  │
│                                                                           │
│  ┌──────────────────────┐  ┌─────────────────────────────────────────┐  │
│  │  Script Editor       │  │  Right Panel (TabControl)               │  │
│  │  ┌────────────────┐  │  │  ┌───────────────────────────────────┐ │  │
│  │  │ Toolbar        │  │  │  │ Help Tab                          │ │  │
│  │  │ [New] [Open]   │  │  │  └───────────────────────────────────┘ │  │
│  │  │ [Save to Gist] │  │  │  ┌───────────────────────────────────┐ │  │
│  │  └────────────────┘  │  │  │ Script Gallery Tab               │ │  │
│  │  ┌────────────────┐  │  │  │ ┌───────────────────────────────┐│ │  │
│  │  │ Tab Control    │  │  │  │ │ [Refresh] [Open]   Status     ││ │  │
│  │  │ ┌────────────┐ │  │  │  │ ├───────────────────────────────┤│ │  │
│  │  │ │Untitled-1  │ │  │  │  │ │ ╔════════════════════════════╗││ │  │
│  │  │ │            │ │  │  │  │ │ ║ Script List View           ║││ │  │
│  │  │ │ (Monaco    │ │  │  │  │ │ ║ • Script Title | Author | …║││ │  │
│  │  │ │  Editor)   │ │  │  │  │ │ ║ • Another Script | …       ║││ │  │
│  │  │ │            │ │  │  │  │ │ ╚════════════════════════════╝││ │  │
│  │  │ └────────────┘ │  │  │  │ ├───────────────────────────────┤│ │  │
│  │  └────────────────┘  │  │  │ │ Details Panel                 ││ │  │
│  └──────────────────────┘  │  │  │ Description: ...              ││ │  │
│                            │  │  │ File: script.ps1              ││ │  │
│  ┌──────────────────────┐  │  │  │ Owner: username               ││ │  │
│  │  PowerShell Console │  │  │  └───────────────────────────────┘│ │  │
│  │  (ConEmu)           │  │  └─────────────────────────────────────┘ │  │
│  └──────────────────────┘  └─────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                                     │ Events & Data Flow
                                     │
                ┌────────────────────┴────────────────────┐
                │                                         │
                ▼                                         ▼
    ┌───────────────────────┐              ┌──────────────────────┐
    │ GitHubGistService     │              │ GistSaveDialog       │
    │                       │              │                      │
    │ - SearchScriptGists() │◄─────────────│ • Description        │
    │ - GetGist()          │              │ • FileName           │
    │ - CreateGist()       │              │ • Public/Private     │
    │ - UpdateGist()       │              │ • GitHub Token       │
    │ - GetUserGists()     │              │ • Update Existing    │
    └───────────┬───────────┘              └──────────────────────┘
                │
                │ HTTP/HTTPS
                │
                ▼
    ┌────────────────────────┐
    │   GitHub REST API      │
    │                        │
    │ • /search/code         │◄──── Search for #rnwdataversepowershell
    │ • /gists/{id}          │◄──── Get gist details
    │ • POST /gists          │◄──── Create new gist
    │ • PATCH /gists/{id}    │◄──── Update existing gist
    └────────────────────────┘
                │
                ▼
    ┌────────────────────────┐
    │   GitHub Gists         │
    │                        │
    │ • Public Gists         │
    │   (visible in gallery) │
    │                        │
    │ • Private Gists        │
    │   (user only)          │
    └────────────────────────┘
```

## Component Interactions

### 1. Browse Scripts Flow

```
User Opens Gallery Tab
    │
    ▼
ScriptGalleryControl.OnLoad()
    │
    ▼
GitHubGistService.SearchScriptGistsAsync()
    │
    ├─► GitHub API: /search/code?q=#rnwdataversepowershell+language:PowerShell
    │
    ├─► Parse Results → Extract Gist IDs
    │
    ├─► For Each Gist ID:
    │       GitHub API: /gists/{id}
    │       Verify Tag Presence
    │
    ▼
List<GistInfo> Returned
    │
    ▼
Populate ListView with Scripts
    │
    ▼
User Sees Script List
```

### 2. Open Script Flow

```
User Double-Clicks Script
    │
    ▼
ScriptGalleryControl.OpenGistRequested Event
    │
    ▼
PowerShellConsolePlugin.ScriptGalleryControl_OpenGistRequested()
    │
    ▼
ScriptEditorControl.OpenFromGistAsync(gist)
    │
    ├─► Create New Tab
    ├─► Store Gist Reference (tab.Tag = gist)
    ├─► Initialize Monaco Editor
    ├─► Load Gist Content
    │
    ▼
New Editor Tab with Script
```

### 3. Save to Gist Flow

```
User Clicks "Save to Gist" Button
    │
    ▼
ScriptEditorControl.SaveToGistRequested Event
    │
    ▼
PowerShellConsolePlugin.ScriptEditorControl_SaveToGistRequested()
    │
    ├─► Get Script Content from Editor
    ├─► Check for Existing Gist (if opened from gallery)
    │
    ▼
Show GistSaveDialog
    │
    ├─► User Fills: Description, FileName, Token
    ├─► User Selects: Public/Private, Update/New
    │
    ▼
User Clicks Save
    │
    ▼
Validate Input
    │
    ▼
GitHubGistService.CreateGistAsync() or UpdateGistAsync()
    │
    ├─► Set Authorization Header (token)
    ├─► POST /gists or PATCH /gists/{id}
    │
    ▼
GitHub Creates/Updates Gist
    │
    ▼
Return GistInfo with URL
    │
    ▼
Show Success Message with URL
```

## Data Models

### GistInfo
```csharp
{
    Id: "abc123...",
    Description: "My script #rnwdataversepowershell",
    HtmlUrl: "https://gist.github.com/user/abc123...",
    IsPublic: true,
    CreatedAt: DateTime,
    UpdatedAt: DateTime,
    Files: {
        "script.ps1": {
            Filename: "script.ps1",
            Content: "# PowerShell script...",
            Size: 1234
        }
    },
    Owner: {
        Login: "username",
        AvatarUrl: "..."
    }
}
```

## Security Model

```
┌──────────────────────────────────────────┐
│ User Authentication                      │
│                                          │
│ • GitHub Personal Access Token (PAT)    │
│ • Scope: gist                            │
│ • Not stored by plugin                   │
│ • Entered each time user saves           │
└──────────────────────────────────────────┘
                    │
                    ▼
┌──────────────────────────────────────────┐
│ Authorization Header                     │
│                                          │
│ Authorization: token {PAT}               │
│                                          │
│ • Sent with POST /gists                  │
│ • Sent with PATCH /gists/{id}            │
│ • Not sent with GET requests (public)    │
└──────────────────────────────────────────┘
                    │
                    ▼
┌──────────────────────────────────────────┐
│ GitHub Permissions                       │
│                                          │
│ • User can only update own gists         │
│ • Public gists visible to all            │
│ • Private gists visible to owner only    │
│ • Rate limiting applies                  │
└──────────────────────────────────────────┘
```

## Error Handling

```
┌─────────────────────────────────────────┐
│ Network Errors                          │
│ • Return empty list                     │
│ • Show error message in status          │
│ • User can retry with Refresh button    │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ API Rate Limiting                       │
│ • Graceful degradation                  │
│ • Return empty list                     │
│ • Inform user to try again later        │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Authentication Errors                   │
│ • Show validation error                 │
│ • Prompt for valid token                │
│ • Link to token creation page           │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Invalid Gist Data                       │
│ • Skip individual gist                  │
│ • Continue processing others            │
│ • No error shown to user                │
└─────────────────────────────────────────┘
```

## Files Added/Modified

### New Files (8)
1. `GitHubGistService.cs` - API client for GitHub Gists
2. `GistSaveDialog.cs` - Save dialog implementation
3. `GistSaveDialog.Designer.cs` - Save dialog UI
4. `ScriptGalleryGuide.md` - Comprehensive user guide
5. `IMPLEMENTATION_SUMMARY.md` - Technical documentation
6. `ARCHITECTURE_DIAGRAM.md` - This file

### Modified Files (5)
1. `ScriptGalleryControl.cs` - Gallery implementation
2. `ScriptGalleryControl.Designer.cs` - Gallery UI
3. `ScriptEditorControl.cs` - Added Save to Gist
4. `ScriptEditorControl.Designer.cs` - Added toolbar button
5. `PowerShellConsolePlugin.cs` - Wired up events
6. `README.md` - Updated with gallery info

### Statistics
- Total Lines Added: ~1,800
- New Classes: 8
- New Methods: 15+
- New UI Controls: 10+
- Documentation Pages: 3
