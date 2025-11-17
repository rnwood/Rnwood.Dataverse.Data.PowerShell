# CI/CD Pipelines

This module can be used in CI/CD pipelines for automated deployments and data operations. Here's how to use it in Azure DevOps and GitHub Actions.

## Azure DevOps Pipelines

**Prerequisites:**
- Install the module in your pipeline
- Use service principal (client secret) authentication
- Store credentials in Azure DevOps Variable Groups or Key Vault

*Example: Azure Pipeline YAML with secure variables:*
```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'  # or 'windows-latest' for PowerShell Desktop

variables:
  - group: 'dataverse-dev'  # Variable group containing DATAVERSE_URL, CLIENT_ID, CLIENT_SECRET, TENANT_ID

steps:
  - task: PowerShell@2
    displayName: 'Install Dataverse Module'
    inputs:
      targetType: 'inline'
      script: |
        Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
        
  - task: PowerShell@2
    displayName: 'Run Dataverse Operations'
    inputs:
      targetType: 'inline'
      script: |
        $ErrorActionPreference = "Stop"
        
        # Connect using service principal from pipeline variables
        $c = Get-DataverseConnection `
          -url "$(DATAVERSE_URL)" `
          -ClientId "$(CLIENT_ID)" `
          -ClientSecret "$(CLIENT_SECRET)" `
          -TenantId "$(TENANT_ID)"
        
        # Your operations here
        $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
        Write-Host "Retrieved $($contacts.Count) contacts"
        
        # Example: Update records
        $contacts | ForEach-Object {
          $_.description = "Updated by pipeline on $(Get-Date)"
          $_
        } | Set-DataverseRecord -Connection $c
    env:
      DATAVERSE_URL: $(DATAVERSE_URL)
      CLIENT_ID: $(CLIENT_ID)
      CLIENT_SECRET: $(CLIENT_SECRET)
      TENANT_ID: $(TENANT_ID)
```

**Setting up Variable Groups:**

1. In Azure DevOps, go to **Pipelines** > **Library**
2. Create a new **Variable Group** (e.g., `dataverse-dev`, `dataverse-prod`)
3. Add variables:
   - `DATAVERSE_URL`: Your Dataverse URL (e.g., `https://myorg.crm.dynamics.com`)
   - `CLIENT_ID`: Application (client) ID from Azure App Registration
   - `CLIENT_SECRET`: Client secret from Azure App Registration (**mark as secret** 🔒)
   - `TENANT_ID`: Azure AD tenant ID
4. Link the variable group to your pipeline

**For production environments:** Consider using [Azure Key Vault integration](https://learn.microsoft.com/azure/devops/pipelines/release/azure-key-vault) to store secrets securely.

**Learn more:**
- [Azure DevOps: Define variables](https://learn.microsoft.com/azure/devops/pipelines/process/variables)
- [Azure DevOps: Variable groups](https://learn.microsoft.com/azure/devops/pipelines/library/variable-groups)
- [Azure DevOps: Use Azure Key Vault secrets](https://learn.microsoft.com/azure/devops/pipelines/release/azure-key-vault)

### Using PAC CLI Authentication in Azure DevOps

If you have the [Power Platform CLI](https://learn.microsoft.com/power-platform/developer/cli/introduction) installed and configured in your pipeline, you can leverage PAC CLI authentication profiles instead of managing credentials separately.

*Example: Azure Pipeline using PAC CLI authentication:*
```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'windows-latest'  # PAC CLI requires Windows or Linux with .NET

steps:
  - task: PowerShell@2
    displayName: 'Install PAC CLI'
    inputs:
      targetType: 'inline'
      script: |
        # Install PAC CLI if not already available
        dotnet tool install --global Microsoft.PowerApps.CLI.Tool
        
  - task: PowerShell@2
    displayName: 'Authenticate with PAC CLI'
    inputs:
      targetType: 'inline'
      script: |
        # Authenticate using service principal
        pac auth create `
          --name "Pipeline" `
          --url "$(DATAVERSE_URL)" `
          --applicationId "$(CLIENT_ID)" `
          --clientSecret "$(CLIENT_SECRET)" `
          --tenant "$(TENANT_ID)"
    env:
      DATAVERSE_URL: $(DATAVERSE_URL)
      CLIENT_ID: $(CLIENT_ID)
      CLIENT_SECRET: $(CLIENT_SECRET)
      TENANT_ID: $(TENANT_ID)
      
  - task: PowerShell@2
    displayName: 'Install Dataverse Module'
    inputs:
      targetType: 'inline'
      script: |
        Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
        
  - task: PowerShell@2
    displayName: 'Run Dataverse Operations'
    inputs:
      targetType: 'inline'
      script: |
        $ErrorActionPreference = "Stop"
        
        # Connect using PAC CLI profile
        $c = Get-DataverseConnection -FromPac
        
        # Your operations here
        $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
        Write-Host "Retrieved $($contacts.Count) contacts"
```

**Learn more:**
- [Power Platform CLI: Installation](https://learn.microsoft.com/power-platform/developer/cli/introduction#install-power-platform-cli)
- [Power Platform CLI: Authentication](https://learn.microsoft.com/power-platform/developer/cli/reference/auth)

### Using Managed Identity in Azure DevOps

For Azure-hosted agents and self-hosted agents on Azure VMs, you can use Azure Managed Identity for passwordless authentication. This eliminates the need to manage secrets entirely.

*Example: Azure Pipeline using Managed Identity:*
```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'  # Can also use Azure-hosted or self-hosted agents on Azure VMs

steps:
  - task: PowerShell@2
    displayName: 'Install Dataverse Module'
    inputs:
      targetType: 'inline'
      script: |
        Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
        
  - task: PowerShell@2
    displayName: 'Run Dataverse Operations with Managed Identity'
    inputs:
      targetType: 'inline'
      script: |
        $ErrorActionPreference = "Stop"
        
        # Connect using Managed Identity (system-assigned or user-assigned)
        $c = Get-DataverseConnection `
          -url "$(DATAVERSE_URL)" `
          -ManagedIdentity
        
        # Or use user-assigned managed identity with specific client ID
        # $c = Get-DataverseConnection `
        #   -url "$(DATAVERSE_URL)" `
        #   -ManagedIdentity `
        #   -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"
        
        # Your operations here
        $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
        Write-Host "Retrieved $($contacts.Count) contacts"
    env:
      DATAVERSE_URL: $(DATAVERSE_URL)
```

**Prerequisites for Managed Identity:**
1. Enable managed identity on your Azure DevOps agent (Azure VM, Azure Container Instance, etc.)
2. Grant the managed identity appropriate permissions in your Dataverse environment:
   - Go to [Power Platform Admin Center](https://admin.powerplatform.microsoft.com/)
   - Select your environment > **Settings** > **Users + permissions** > **Application users**
   - Create an application user for the managed identity
   - Assign appropriate security role

**Learn more:**
- [Azure Managed Identities Overview](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Configure managed identity for Azure Pipelines](https://learn.microsoft.com/azure/devops/pipelines/library/connect-to-azure#use-a-managed-identity)
- [Authentication Methods](../getting-started/authentication.md) - All supported authentication methods

## GitHub Actions

**Prerequisites:**
- Install the module in your workflow
- Use service principal (client secret) authentication
- Store credentials in GitHub Secrets or Environment Secrets

*Example: GitHub Actions workflow with secure secrets:*
```yaml
# .github/workflows/dataverse-deploy.yml
name: Dataverse Operations

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest  # or windows-latest for PowerShell Desktop
    
    # Use environment for environment-specific secrets
    environment: development
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        
      - name: Install Dataverse Module
        shell: pwsh
        run: |
          Install-Module -Name Rnwood.Dataverse.Data.PowerShell -Force -Scope CurrentUser
          
      - name: Run Dataverse Operations
        shell: pwsh
        env:
          DATAVERSE_URL: ${{ secrets.DATAVERSE_URL }}
          CLIENT_ID: ${{ secrets.CLIENT_ID }}
          CLIENT_SECRET: ${{ secrets.CLIENT_SECRET }}
          TENANT_ID: ${{ secrets.TENANT_ID }}
        run: |
          $ErrorActionPreference = "Stop"
          
          # Connect using service principal from secrets
          $c = Get-DataverseConnection `
            -url $env:DATAVERSE_URL `
            -ClientId $env:CLIENT_ID `
            -ClientSecret $env:CLIENT_SECRET `
            -TenantId $env:TENANT_ID
          
          # Your operations here
          $contacts = Get-DataverseRecord -Connection $c -TableName contact -Top 10
          Write-Host "Retrieved $($contacts.Count) contacts"
          
          # Example: Create/update records
          $contacts | ForEach-Object {
            $_.description = "Updated by GitHub Actions on $(Get-Date)"
            $_
          } | Set-DataverseRecord -Connection $c -WhatIf  # Remove -WhatIf when ready
```

**Setting up GitHub Secrets:**

1. Go to your repository **Settings** > **Secrets and variables** > **Actions**
2. Add **Repository secrets** or **Environment secrets**:
   - `DATAVERSE_URL`: Your Dataverse URL (e.g., `https://myorg.crm.dynamics.com`)
   - `CLIENT_ID`: Application (client) ID from Azure App Registration
   - `CLIENT_SECRET`: Client secret from Azure App Registration
   - `TENANT_ID`: Azure AD tenant ID

**Using Environments for multiple stages:**

```yaml
# Deploy to different environments
jobs:
  deploy-dev:
    runs-on: ubuntu-latest
    environment: development
    steps:
      # ... uses secrets.DATAVERSE_URL from development environment
      
  deploy-prod:
    runs-on: ubuntu-latest
    environment: production
    needs: deploy-dev  # Runs after dev deployment
    steps:
      # ... uses secrets.DATAVERSE_URL from production environment
```

**Learn more:**
- [GitHub Actions: Using secrets](https://docs.github.com/actions/security-guides/using-secrets-in-github-actions)
- [GitHub Actions: Using environments](https://docs.github.com/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [GitHub Actions: Workflow syntax](https://docs.github.com/actions/using-workflows/workflow-syntax-for-github-actions)

## Azure App Registration Setup

Both Azure DevOps and GitHub Actions require an Azure App Registration for service principal authentication:

1. **Create App Registration:**
   - Go to [Azure Portal](https://portal.azure.com) > **Azure Active Directory** > **App registrations**
   - Click **New registration**, give it a name (e.g., `Dataverse-CI-CD`)
   - Click **Register**

2. **Create Client Secret:**
   - In your app registration, go to **Certificates & secrets**
   - Click **New client secret**, add a description, set expiration
   - **Copy the secret value immediately** (you can't see it again)

3. **Grant Permissions in Dataverse:**
   - Go to [Power Platform Admin Center](https://admin.powerplatform.microsoft.com/)
   - Select your environment > **Settings** > **Users + permissions** > **Application users**
   - Click **New app user**, select your app registration
   - Assign appropriate security role (e.g., System Administrator for full access)

4. **Note down:**
   - Application (client) ID
   - Directory (tenant) ID  
   - Client secret value

**Learn more:**
- [Microsoft Docs: Register an application](https://learn.microsoft.com/power-apps/developer/data-platform/walkthrough-register-app-azure-active-directory)
- [Microsoft Docs: Application user authentication](https://learn.microsoft.com/power-apps/developer/data-platform/use-single-tenant-server-server-authentication)

## See Also

- [Data Export](data-export.md) - Export data to various formats
- [Data Import](data-import.md) - Import data from files
- [Source Control](source-control.md) - Manage data in source control and copy between environments
