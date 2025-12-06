# Script Gallery Feature

## Overview

The Script Gallery is a community-driven feature that allows XrmToolbox PowerShell Plugin users to share and discover PowerShell scripts for Dataverse operations. Scripts are stored and managed as GitHub Discussions in the project repository.

## Features

### 1. GitHub Authentication
- **Personal Access Token (PAT)**: Users authenticate using a GitHub Personal Access Token
- To create a PAT:
  1. Go to https://github.com/settings/tokens
  2. Click "Generate new token" (classic)
  3. Select the following scopes:
     - `repo` - Full control of private repositories
     - `read:discussion` - Read discussions
     - `write:discussion` - Write discussions
  4. Generate and copy the token
  5. Use it in the plugin's login dialog

### 2. Browse Scripts
- View all shared scripts in the gallery
- Scripts are displayed with:
  - Title
  - Author
  - Upvote count
  - Comment count
  - Creation date

### 3. View Script Details
- Select a script to view full details
- Markdown-formatted content with syntax highlighting
- Read comments from other users
- See full script content in PowerShell code blocks

### 4. Load Scripts
- Click "Load to Editor" to open a script in a new editor tab
- Scripts are automatically extracted from markdown code blocks
- Ready to run or modify immediately

### 5. Save Scripts
- Click "Save to Gallery" in the script editor toolbar
- Enter a title for your script
- Script is automatically wrapped in a PowerShell code block
- Creates a new GitHub Discussion in the repository

### 6. Interact with Community
- **Upvote** scripts you find useful (requires authentication)
- **Comment** on scripts to provide feedback or ask questions
- View all community comments on each script

## Technical Implementation

### Architecture
- **GitHubService**: Handles all GitHub API interactions using Octokit.NET
- **ScriptGalleryControl**: WinForms user control with list/detail UI
- **Integration**: Seamlessly connects with ScriptEditorControl for load/save operations

### GitHub API Usage
The gallery uses GitHub Discussions as a data store:
- Each script is a Discussion in the repository
- Script content is stored in PowerShell code blocks in the discussion body
- Comments and reactions use native GitHub features
- GraphQL API is used for discussions (REST API doesn't fully support discussions)

### Data Format
Scripts are stored in discussions with this format:
```markdown
# Script Title

```powershell
# Your PowerShell script here
Get-DataverseConnection -url "https://org.crm.dynamics.com" -interactive
Get-DataverseRecord -connection $connection -tablename account -Top 10
```

Optional description and explanation...
```

### Security
- Authentication uses GitHub Personal Access Tokens (user-provided)
- Tokens are never stored - only kept in memory during session
- All API calls go through Octokit.NET with proper authentication
- GraphQL queries are parameterized to prevent injection

## Usage Guide

### For Users Browsing Scripts
1. Open the XrmToolbox PowerShell Plugin
2. Navigate to the "Script Gallery" tab
3. Click "Refresh" to load available scripts (works without authentication)
4. Click on a script to view details
5. Click "Load to Editor" to use the script

### For Users Sharing Scripts
1. Write your PowerShell script in the editor
2. Click "Save to Gallery" in the toolbar
3. If not logged in, you'll be prompted to authenticate
4. Enter a descriptive title for your script
5. Your script is published to the community!

### For Users Interacting
1. Ensure you're logged in (click "Login to GitHub")
2. Browse or search for scripts
3. Click 👍 Upvote to show appreciation
4. Add comments to provide feedback or ask questions

## Limitations and Future Enhancements

### Current Limitations
- Authentication requires manual PAT creation (future: OAuth flow)
- No search or filtering functionality
- Discussion categories not yet configurable
- Cannot edit existing discussions (requires GitHub UI)

### Planned Enhancements
- Device flow OAuth for easier authentication
- Search and filter capabilities
- Category-based organization
- Script versioning and updates
- Favorite/bookmark scripts
- User profiles and contributions
- Script ratings and popularity metrics

## Troubleshooting

### "Failed to load discussions"
- Check your internet connection
- Verify the repository exists and is accessible
- GitHub API rate limits may apply (60 requests/hour unauthenticated, 5000/hour authenticated)

### "Authentication failed"
- Verify your token has the correct scopes
- Check that the token hasn't expired
- Ensure you copied the token correctly (no extra spaces)

### "No PowerShell script found"
- The discussion may not contain a PowerShell code block
- Code block must be tagged with `powershell` or `ps1` language identifier

## Contributing

To contribute to the Script Gallery feature:
1. Report issues on GitHub
2. Submit pull requests with improvements
3. Share your scripts in the gallery
4. Help others by commenting on their scripts

## License

This feature is part of the Rnwood.Dataverse.Data.PowerShell project and follows the same license.
