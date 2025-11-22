$ErrorActionPreference = "Stop"

# Helper to safely check if a global variable exists (avoids StrictMode errors)
function Get-GlobalVarExists([string]$Name) {
    return (Get-Variable -Scope Global -Name $Name -ErrorAction SilentlyContinue) -ne $null
}

# Only run setup once - check if already initialized (safe under StrictMode)
if (-not (Get-GlobalVarExists 'TestsInitialized') -or -not $global:TestsInitialized) {
    # Module path setup before anything else
    if ($env:TESTMODULEPATH) {
        $source = $env:TESTMODULEPATH
    }
    else {
        $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
    }

    $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
    New-Item -ItemType Directory $tempmodulefolder | Out-Null
    Copy-Item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
    $env:PSModulePath = $tempmodulefolder
    $env:ChildProcessPSModulePath = $tempmodulefolder

    # Import module early so it's available for all tests
    Import-Module Rnwood.Dataverse.Data.PowerShell -Force

    # Mark as initialized
    $global:TestsInitialized = $true
}

# Helper functions in global scope so all tests can access them
# Use global variable for metadata cache - stores metadata by entity name
if (-not (Get-GlobalVarExists 'TestMetadataCache')) {
    $global:TestMetadataCache = @{}
}

function global:getMockConnection([ScriptBlock]$RequestInterceptor = $null, [string[]]$Entities = @("contact")) {
    # Load metadata for requested entities if not already loaded
    $metadata = @()
    foreach ($entityName in $Entities) {
        if (-not $global:TestMetadataCache.ContainsKey($entityName)) {
            LoadTestMetadata -EntityName $entityName
        }
        $metadata += $global:TestMetadataCache[$entityName]
    }
   
    # Create combined interceptor that handles unsupported requests and delegates to custom interceptor
    $combinedInterceptor = if ($null -ne $RequestInterceptor) {
        {
            param($request)
            
            # First try the custom interceptor - it takes priority
            # This allows tests to override default behavior
            try {
                $customResult = & $RequestInterceptor $request
                if ($null -ne $customResult) {
                    return $customResult
                }
            } catch {
                # If custom interceptor throws, re-throw it (don't suppress test exceptions)
                throw
            }
            
            # Handle unsupported requests that FakeXrmEasy doesn't support
            # Only do this if custom interceptor didn't handle it
            
            # Handle RetrieveUnpublishedRequest - let it throw exception so cmdlets fall back
            # FormXmlHelper and other cmdlets catch exceptions from RetrieveUnpublished
            # and fall back to regular Retrieve. Don't intercept this - let FakeXrmEasy throw.
            # (No handler needed - FakeXrmEasy will throw OpenSourceUnsupportedException)
            
            # Handle RetrieveUnpublishedMultipleRequest - return empty collection
            # Cmdlets query unpublished first to get uncommitted changes, then query published
            # Returning empty here means they won't find unpublished data and will use published queries
            if ($request.GetType().Name -eq 'RetrieveUnpublishedMultipleRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
            
            # Handle ValidateAppRequest - return success response
            if ($request.GetType().Name -eq 'ValidateAppRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.ValidateAppResponse
                # Create proper AppValidationResponse object
                $validationResponseType = [Microsoft.Crm.Sdk.Messages.ValidateAppResponse].Assembly.GetType('Microsoft.Crm.Sdk.Messages.AppValidationResponse')
                if ($null -ne $validationResponseType) {
                    $validationResponse = [Activator]::CreateInstance($validationResponseType)
                    # ValidationIssueList should be empty array
                    $validationResponse.ValidationIssueList = @()
                    $response.Results.Add("AppValidationResponse", $validationResponse)
                } else {
                    # Fallback: create a minimal object
                    $validationResponse = New-Object PSObject -Property @{
                        ValidationIssueList = @()
                    }
                    $response.Results.Add("AppValidationResponse", $validationResponse)
                }
                return $response
            }
            
            # Handle PublishXmlRequest - return empty response
            if ($request.GetType().Name -eq 'PublishXmlRequest') {
                return New-Object Microsoft.Crm.Sdk.Messages.PublishXmlResponse
            }
            
            # Handle UpdateEntityRequest - return empty response
            if ($request.GetType().Name -eq 'UpdateEntityRequest') {
                return New-Object Microsoft.Xrm.Sdk.Messages.UpdateEntityResponse
            }
            
            # Handle RetrieveEntityRequest - return entity metadata from cache
            if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                $entityName = $request.LogicalName
                if ($global:TestMetadataCache.ContainsKey($entityName)) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $response.Results.Add("EntityMetadata", $global:TestMetadataCache[$entityName])
                    return $response
                }
                # If entity not found, let FakeXrmEasy throw appropriate exception
            }
            
            # Handle AddAppComponentsRequest and RemoveAppComponentsRequest
            if ($request.GetType().Name -eq 'AddAppComponentsRequest' -or 
                $request.GetType().Name -eq 'RemoveAppComponentsRequest') {
                # Return empty response - the request was successful
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
            
            # Handle CreateEntityKeyRequest - return success response with new key ID
            if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                return $response
            }
            
            # Handle DeleteEntityKeyRequest - return empty success response
            if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                return New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
            }
            
            # Handle WhoAmI request - return mock organization ID and user ID
            if ($request.RequestName -eq 'WhoAmI') {
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.Results.Add("UserId", [Guid]::NewGuid())
                $response.Results.Add("BusinessUnitId", [Guid]::NewGuid())
                $response.Results.Add("OrganizationId", [Guid]::Parse("00000000-0000-0000-0000-000000000001"))
                return $response
            }
            
            # Handle RetrieveRequest for organization entity - return mock organization with language code
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $retrieveReq = $request -as [Microsoft.Xrm.Sdk.Messages.RetrieveRequest]
                if ($retrieveReq.Target.LogicalName -eq 'organization') {
                    $org = New-Object Microsoft.Xrm.Sdk.Entity("organization", $retrieveReq.Target.Id)
                    $org.Attributes.Add("languagecode", 1033) # English (US)
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                    $response.Results.Add("Entity", $org)
                    return $response
                }
            }
            
            # Don't return anything - let FakeXrmEasy handle the request
        }.GetNewClosure()
    } else {
        {
            param($request)
            
            # Handle unsupported requests that FakeXrmEasy doesn't support
            
            # Handle RetrieveUnpublishedRequest - let it throw exception so cmdlets fall back
            # FormXmlHelper and other cmdlets catch exceptions from RetrieveUnpublished
            # and fall back to regular Retrieve. Don't intercept this - let FakeXrmEasy throw.
            # (No handler needed - FakeXrmEasy will throw OpenSourceUnsupportedException)
            
            # Handle RetrieveUnpublishedMultipleRequest - return empty collection
            # Cmdlets query unpublished first to get uncommitted changes, then query published
            # Returning empty here means they won't find unpublished data and will use published queries
            if ($request.GetType().Name -eq 'RetrieveUnpublishedMultipleRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse
                $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                $response.Results.Add("EntityCollection", $entityCollection)
                return $response
            }
            
            # Handle ValidateAppRequest - return success response
            if ($request.GetType().Name -eq 'ValidateAppRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.ValidateAppResponse
                # Create proper AppValidationResponse object
                $validationResponseType = [Microsoft.Crm.Sdk.Messages.ValidateAppResponse].Assembly.GetType('Microsoft.Crm.Sdk.Messages.AppValidationResponse')
                if ($null -ne $validationResponseType) {
                    $validationResponse = [Activator]::CreateInstance($validationResponseType)
                    # ValidationIssueList should be empty array
                    $validationResponse.ValidationIssueList = @()
                    $response.Results.Add("AppValidationResponse", $validationResponse)
                } else {
                    # Fallback: create a minimal object
                    $validationResponse = New-Object PSObject -Property @{
                        ValidationIssueList = @()
                    }
                    $response.Results.Add("AppValidationResponse", $validationResponse)
                }
                return $response
            }
            
            # Handle PublishXmlRequest - return empty response
            if ($request.GetType().Name -eq 'PublishXmlRequest') {
                return New-Object Microsoft.Crm.Sdk.Messages.PublishXmlResponse
            }
            
            # Handle UpdateEntityRequest - return empty response
            if ($request.GetType().Name -eq 'UpdateEntityRequest') {
                return New-Object Microsoft.Xrm.Sdk.Messages.UpdateEntityResponse
            }
            
            # Handle RetrieveEntityRequest - return entity metadata from cache
            if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                $entityName = $request.LogicalName
                if ($global:TestMetadataCache.ContainsKey($entityName)) {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $response.Results.Add("EntityMetadata", $global:TestMetadataCache[$entityName])
                    return $response
                }
                # If entity not found, let FakeXrmEasy throw appropriate exception
            }
            
            # Handle AddAppComponentsRequest and RemoveAppComponentsRequest
            if ($request.GetType().Name -eq 'AddAppComponentsRequest' -or 
                $request.GetType().Name -eq 'RemoveAppComponentsRequest') {
                # Return empty response - the request was successful
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
            
            # Handle CreateEntityKeyRequest - return success response with new key ID
            if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                return $response
            }
            
            # Handle DeleteEntityKeyRequest - return empty success response
            if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                return New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
            }
            
            # Handle WhoAmI request - return mock organization ID and user ID
            if ($request.RequestName -eq 'WhoAmI') {
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.Results.Add("UserId", [Guid]::NewGuid())
                $response.Results.Add("BusinessUnitId", [Guid]::NewGuid())
                $response.Results.Add("OrganizationId", [Guid]::Parse("00000000-0000-0000-0000-000000000001"))
                return $response
            }
            
            # Handle RetrieveRequest for organization entity - return mock organization with language code
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $retrieveReq = $request -as [Microsoft.Xrm.Sdk.Messages.RetrieveRequest]
                if ($retrieveReq.Target.LogicalName -eq 'organization') {
                    $org = New-Object Microsoft.Xrm.Sdk.Entity("organization", $retrieveReq.Target.Id)
                    $org.Attributes.Add("languagecode", 1033) # English (US)
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                    $response.Results.Add("Entity", $org)
                    return $response
                }
            }
            
            # Don't return anything - let FakeXrmEasy handle the request
        }
    }
   
    # Create the connection (no caching for test isolation)
    $mockService = Get-DataverseConnection -url https://fake.crm.dynamics.com/ -mock $metadata -RequestInterceptor $combinedInterceptor
    return $mockService
}

function global:LoadTestMetadata {
    param(
        [Parameter(Mandatory=$true)]
        [string]$EntityName
    )
    
    if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
        Import-Module Rnwood.Dataverse.Data.PowerShell
    }
    
    Add-Type -AssemblyName "System.Runtime.Serialization"

    # Define the DataContractSerializer
    $serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])

    $xmlFile = "$PSScriptRoot/$EntityName.xml"
    if (Test-Path $xmlFile) {
        Write-Verbose "Loading metadata for entity: $EntityName..."
        $stream = [IO.File]::OpenRead($xmlFile)
        $metadata = $serializer.ReadObject($stream)
        $stream.Close()
        $global:TestMetadataCache[$EntityName] = $metadata
        Write-Verbose "Metadata loaded for entity: $EntityName"
    }
    else {
        throw "Metadata file not found: $xmlFile - Entity '$EntityName' metadata must be available for testing. Available entities: $(Get-ChildItem $PSScriptRoot/*.xml | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) } | Join-String -Separator ', ')"
    }
}

function global:newPwsh([scriptblock] $scriptblock) {
    if ([System.Environment]::OSVersion.Platform -eq "Unix") {
        pwsh -noninteractive -noprofile -command $scriptblock
    }
    else {
        cmd /c pwsh -noninteractive -noprofile -command $scriptblock
    }
}
