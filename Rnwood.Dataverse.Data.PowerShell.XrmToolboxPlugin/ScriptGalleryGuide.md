# Script Gallery User Guide

The Script Gallery is a community-driven feature that allows you to browse, share, and reuse PowerShell scripts for Dataverse automation using GitHub Gists.

## Overview

The Script Gallery uses GitHub Gists as a backend for storing and sharing scripts. Scripts are tagged with the hashtag `#rnwdataversepowershell` in their description, which allows the gallery to discover and display them.

## Browsing Scripts

### Viewing Available Scripts

1. Navigate to the **Script Gallery** tab in the right panel of the XrmToolbox plugin
2. The gallery automatically loads public scripts from GitHub Gists
3. You'll see a list of scripts with the following information:
   - **Script Title**: Extracted from the gist description
   - **Author**: GitHub username of the script creator
   - **Updated**: Last modification date

### Viewing Script Details

1. Click on any script in the list to view its details in the panel below
2. The details panel shows:
   - Full description
   - File name and size
   - Author information
   - Creation and update timestamps
   - GitHub URL

### Opening Scripts

1. Select a script from the list
2. Click the **Open** button or double-click the script
3. The script will open in a new editor tab
4. You can now view, edit, run, or save the script locally

### Refreshing the Gallery

Click the **Refresh** button to reload the list of scripts from GitHub Gists.

## Sharing Your Scripts

### Saving a New Script to GitHub Gist

1. Write or edit your script in the editor
2. Click **Save to Gist** in the editor toolbar
3. Fill in the Save to Gist dialog:

   **Description**:
   - Provide a clear, descriptive title for your script
   - The hashtag `#rnwdataversepowershell` is automatically added if not present
   - Example: "Bulk update account records #rnwdataversepowershell"

   **File Name**:
   - Must end with `.ps1` extension
   - Use a descriptive name like `bulk-update-accounts.ps1`

   **Visibility**:
   - **Public**: Script will be visible in the Script Gallery for everyone
   - **Private**: Script is saved to your GitHub account but not visible in the gallery

   **GitHub Personal Access Token**:
   - Required for authentication
   - See "Creating a GitHub Token" section below

4. Click **Save** to create the gist

### Updating an Existing Script

When you open a script from the gallery:

1. Make your edits in the editor
2. Click **Save to Gist** in the toolbar
3. The dialog will show "Opened from gist: [gist-id]"
4. Check **Update existing gist** to modify the original gist
   - **Important**: Only do this if you are the owner of the gist
5. Uncheck to create a new gist (fork the script)

### Best Practices for Sharing Scripts

1. **Clear Description**: Write a concise description that explains what the script does
2. **Meaningful Filename**: Use descriptive filenames like `export-solutions.ps1` instead of `script.ps1`
3. **Add Comments**: Include comments in your script to explain complex logic
4. **Include Examples**: Add usage examples in comments at the top of the script
5. **Test Before Sharing**: Test your script thoroughly before making it public
6. **Security**: Never include passwords, connection strings, or sensitive data in public scripts

## Creating a GitHub Personal Access Token

To save scripts to GitHub Gists, you need a Personal Access Token:

### Step-by-Step Instructions

1. Log in to your GitHub account
2. Go to [https://github.com/settings/tokens](https://github.com/settings/tokens)
3. Click **Generate new token** → **Generate new token (classic)**
4. Configure your token:
   - **Note**: Give it a descriptive name like "XrmToolbox Script Gallery"
   - **Expiration**: Choose an expiration period (recommended: 90 days)
   - **Scopes**: Select **gist** (this is the only scope needed)
5. Click **Generate token** at the bottom
6. Copy the token immediately (you won't be able to see it again)
7. Store it securely (consider using a password manager)

### Token Security

- **Do not share your token** with anyone
- **Do not commit tokens** to public repositories
- If you suspect your token has been compromised, revoke it immediately at [https://github.com/settings/tokens](https://github.com/settings/tokens)
- The plugin does not store your token; you'll need to enter it each time you save a gist

## Troubleshooting

### "No scripts found" Message

- **Cause**: No public gists with the `#rnwdataversepowershell` tag exist yet
- **Solution**: Be the first to share a script! Create and share your own scripts to populate the gallery

### "Error loading scripts" Message

- **Cause**: Network connectivity issues or GitHub API rate limiting
- **Solutions**:
  - Check your internet connection
  - Wait a few minutes and click Refresh (GitHub has API rate limits)
  - Try again later if GitHub is experiencing issues

### "Failed to save gist" Error

- **Causes**:
  - Invalid or expired GitHub token
  - Network connectivity issues
  - Invalid gist data
- **Solutions**:
  - Verify your GitHub token is correct and not expired
  - Check the token has the "gist" scope
  - Ensure description and filename are filled in correctly
  - Check your internet connection

### "Failed to open gist" Error

- **Causes**:
  - Network connectivity issues
  - The gist has been deleted by the owner
  - The gist doesn't contain PowerShell content
- **Solutions**:
  - Click Refresh to reload the gallery
  - Try opening a different script
  - Check your internet connection

## Technical Details

### How the Gallery Works

1. **Discovery**: The gallery uses GitHub's Code Search API to find gists containing `#rnwdataversepowershell`
2. **Filtering**: Results are filtered to only include public gists with PowerShell (.ps1) files
3. **Display**: Scripts are sorted by last update date (newest first)
4. **Limitation**: The gallery displays up to 50 most recent scripts

### GitHub API Rate Limits

- **Unauthenticated requests**: 60 requests per hour per IP address
- **Authenticated requests**: 5,000 requests per hour (when using a token)
- The gallery makes 1 request to search, plus 1 request per displayed script

### Privacy and Security

- **Public gists**: Visible to everyone with the URL, and discoverable via search
- **Private gists**: Only visible to you when logged into GitHub
- **Data storage**: Scripts are stored on GitHub's servers, not locally
- **No tracking**: The plugin does not track which scripts you view or use

## Community Guidelines

When sharing scripts, please:

1. **Respect licenses**: Only share code you have the right to share
2. **Give credit**: If your script is based on someone else's work, mention it
3. **Be helpful**: Include clear descriptions and usage instructions
4. **Test thoroughly**: Don't share untested or broken scripts
5. **Be respectful**: Follow GitHub's Terms of Service and Community Guidelines

## Support and Feedback

If you encounter issues or have suggestions for improving the Script Gallery:

1. Check the troubleshooting section above
2. Report issues on the GitHub repository: [https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell)
3. Include detailed information about the error and steps to reproduce

---

**Happy scripting!** We're excited to see what the community creates and shares through the Script Gallery.
