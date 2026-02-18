using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;
using System;
using System.IO;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.CopilotStudio
{
    /// <summary>
    /// E2E tests for Copilot Studio bot management cmdlets.
    /// Tests CRUD operations, export/import, and component management.
    /// </summary>
    [Trait("Category", "BotManagement")]
    public class BotManagementTests : E2ETestBase
    {
        [Fact]
        public void GetDataverseBot_ListsBots()
        {
            var script = GetConnectionScript($@"
                $bots = Get-DataverseBot
                Write-Host ""Found $($bots.Count) bot(s)""
                if ($bots.Count -eq 0) {{
                    throw 'No bots found'
                }}
                Write-Host 'SUCCESS: Get-DataverseBot works'
            ");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }

        [Fact]
        public void GetDataverseBotComponent_ListsComponents()
        {
            var script = GetConnectionScript($@"
                $bot = Get-DataverseBot | Select-Object -First 1
                if ($null -eq $bot) {{
                    throw 'No bots found'
                }}
                
                $components = Get-DataverseBotComponent -ParentBotId $bot.botid
                Write-Host ""Found $($components.Count) component(s) for bot $($bot.name)""
                Write-Host 'SUCCESS: Get-DataverseBotComponent works'
            ");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }

        [Fact]
        public void SetAndRemoveDataverseBotComponent_CreatesAndDeletesComponent()
        {
            var script = GetConnectionScript($@"
                $bot = Get-DataverseBot | Select-Object -First 1
                if ($null -eq $bot) {{
                    throw 'No bots found'
                }}
                
                # Create a test component
                $testName = ""TEST_E2E_Component_$(Get-Date -Format 'yyyyMMddHHmmss')""
                $testSchema = ""test_e2e_component_$(Get-Date -Format 'yyyyMMddHHmmss')""
                
                Write-Host ""Creating test component: $testName""
                $newComponent = Set-DataverseBotComponent `
                    -Name $testName `
                    -SchemaName $testSchema `
                    -ParentBotId $bot.botid `
                    -ComponentType 10 `
                    -Data 'kind: AdaptiveDialog\nbeginDialog:\n  kind: SendActivity\n  activity: Test' `
                    -Description 'E2E test component' `
                    -PassThru `
                    -Confirm:$false
                
                if ($null -eq $newComponent) {{
                    throw 'Failed to create component'
                }}
                
                Write-Host ""Created component with ID: $($newComponent.botcomponentid)""
                
                # Verify it exists
                $retrieved = Get-DataverseBotComponent -BotComponentId $newComponent.botcomponentid
                if ($null -eq $retrieved) {{
                    throw 'Failed to retrieve created component'
                }}
                
                # Clean up - delete the component
                Write-Host ""Deleting test component""
                Remove-DataverseBotComponent -BotComponentId $newComponent.botcomponentid -Confirm:$false
                
                # Verify deletion
                $afterDelete = Get-DataverseBotComponent -BotComponentId $newComponent.botcomponentid
                if ($afterDelete.Count -ne 0) {{
                    throw 'Component still exists after deletion'
                }}
                
                Write-Host 'SUCCESS: Set and Remove DataverseBotComponent work'
            ");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }

        [Fact]
        public void CopyDataverseBotComponent_ClonesComponent()
        {
            var script = GetConnectionScript($@"
                $bot = Get-DataverseBot | Select-Object -First 1
                if ($null -eq $bot) {{
                    throw 'No bots found'
                }}
                
                # Get an existing component to clone
                $sourceComponent = Get-DataverseBotComponent -ParentBotId $bot.botid -Top 1 | Select-Object -First 1
                if ($null -eq $sourceComponent) {{
                    throw 'No components found to clone'
                }}
                
                Write-Host ""Cloning component: $($sourceComponent.name)""
                $copyName = ""TEST_E2E_Copy_$(Get-Date -Format 'yyyyMMddHHmmss')""
                
                $clonedComponent = Copy-DataverseBotComponent `
                    -BotComponentId $sourceComponent.botcomponentid `
                    -NewName $copyName `
                    -PassThru `
                    -Confirm:$false
                
                if ($null -eq $clonedComponent) {{
                    throw 'Failed to clone component'
                }}
                
                Write-Host ""Cloned component with ID: $($clonedComponent.botcomponentid)""
                
                # Verify it exists
                $retrieved = Get-DataverseBotComponent -BotComponentId $clonedComponent.botcomponentid
                if ($null -eq $retrieved) {{
                    throw 'Failed to retrieve cloned component'
                }}
                
                # Clean up
                Write-Host ""Cleaning up cloned component""
                Remove-DataverseBotComponent -BotComponentId $clonedComponent.botcomponentid -Confirm:$false
                
                Write-Host 'SUCCESS: Copy-DataverseBotComponent works'
            ");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }

        [Fact]
        public void ExportAndImportDataverseBot_BackupAndRestore()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"bot_test_{Guid.NewGuid():N}");
            
            try
            {
                var script = GetConnectionScript($@"
                    $bot = Get-DataverseBot | Select-Object -First 1
                    if ($null -eq $bot) {{
                        throw 'No bots found'
                    }}
                    
                    # Export bot
                    Write-Host ""Exporting bot: $($bot.name)""
                    $exportPath = '{tempDir.Replace("\\", "\\\\")}'
                    
                    $export = Export-DataverseBot `
                        -BotId $bot.botid `
                        -OutputPath $exportPath `
                        -PassThru `
                        -Confirm:$false
                    
                    if ($null -eq $export) {{
                        throw 'Export failed'
                    }}
                    
                    Write-Host ""Exported $($export.ComponentCount) components to $($export.OutputPath)""
                    
                    # Verify export structure
                    $manifestPath = Join-Path $exportPath 'manifest.json'
                    if (-not (Test-Path $manifestPath)) {{
                        throw 'Manifest file not found'
                    }}
                    
                    $configPath = Join-Path $exportPath 'bot_config.json'
                    if (-not (Test-Path $configPath)) {{
                        throw 'Bot config file not found'
                    }}
                    
                    # Verify manifest content
                    $manifest = Get-Content $manifestPath | ConvertFrom-Json
                    if ($manifest.version -ne '1.0') {{
                        throw 'Invalid manifest version'
                    }}
                    
                    if ($manifest.componentCount -ne $export.ComponentCount) {{
                        throw 'Manifest component count mismatch'
                    }}
                    
                    # Test import with WhatIf (dry run)
                    Write-Host ""Testing import with WhatIf""
                    Import-DataverseBot `
                        -Path $exportPath `
                        -Name 'Test Import' `
                        -SchemaName 'test_import' `
                        -WhatIf
                    
                    Write-Host 'SUCCESS: Export and Import DataverseBot work'
                ");

                var result = RunScript(script);

                result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
                result.StandardOutput.Should().Contain("SUCCESS");
                
                // Verify directory was created
                Directory.Exists(tempDir).Should().BeTrue("Export directory should exist");
                File.Exists(Path.Combine(tempDir, "manifest.json")).Should().BeTrue("Manifest file should exist");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        [Fact]
        public void GetDataverseConversationTranscript_ListsTranscripts()
        {
            var script = GetConnectionScript($@"
                # Get a bot to query transcripts for
                $bot = Get-DataverseBot | Select-Object -First 1
                if ($null -eq $bot) {{
                    throw 'No bots found'
                }}
                
                # List transcripts (may be empty)
                Write-Host ""Querying transcripts for bot: $($bot.name)""
                $transcripts = Get-DataverseConversationTranscript -BotId $bot.botid -Top 10
                Write-Host ""Found $($transcripts.Count) transcript(s)""
                
                Write-Host 'SUCCESS: Get-DataverseConversationTranscript works'
            ");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
