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
            
            # Don't return anything - let FakeXrmEasy handle the request
        }
    }
   
    # Create the FakeXrmEasy mock connection directly (no longer using cmdlet's -Mock option)
    Add-Type -AssemblyName "FakeItEasy"
    
    # Build FakeXrmEasy context using assemblies from test project
    $builder = [FakeXrmEasy.Middleware.MiddlewareBuilder]::New()
    $builder = $builder.AddCrud()
    $fakeXrmEasyMsgs = [System.Reflection.Assembly]::LoadFrom("$env:TESTMODULEPATH\FakeXrmEasy.Messages.dll")
    $builder = $builder.AddFakeMessageExecutors($fakeXrmEasyMsgs)
    $builder = $builder.UseMessages()
    $builder = $builder.UseCrud()
    $builder = $builder.SetLicense([FakeXrmEasy.Abstractions.Enums.FakeXrmEasyLicense]::RPL_1_5)
    
    $xrmContext = $builder.Build()
    $xrmContext.InitializeMetadata($metadata)
    
    # Get organization service
    $orgService = $xrmContext.GetOrganizationService()
    
    # Wrap with request interceptor if provided
    if ($combinedInterceptor) {
        # Load cmdlets assembly to access MockOrganizationServiceWithScriptBlock
        Add-Type -Path "$env:TESTMODULEPATH\Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll"
        $orgService = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithScriptBlock($orgService, $combinedInterceptor)
    }
    
    # Create ServiceClient via reflection (same as old -Mock implementation)
    $httpClient = New-Object System.Net.Http.HttpClient
    $version = New-Object Version(9, 2)
    $fakeLogger = [FakeItEasy.A]::Fake([Microsoft.Extensions.Logging.ILogger])
    
    $constructor = [Microsoft.PowerPlatform.Dataverse.Client.ServiceClient].GetConstructor(
        [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance,
        $null,
        @([Microsoft.Xrm.Sdk.IOrganizationService], [System.Net.Http.HttpClient], [string], [Version], [Microsoft.Extensions.Logging.ILogger]),
        $null
    )
    
    $mockService = $constructor.Invoke(@($orgService, $httpClient, "https://fake.crm.dynamics.com", $version, $fakeLogger))
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
