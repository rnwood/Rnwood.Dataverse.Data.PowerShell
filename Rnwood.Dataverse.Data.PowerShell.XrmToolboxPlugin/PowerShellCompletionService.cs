using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service that provides PowerShell code completion using TabExpansion2 API.
    /// Compatible with PowerShell 5.1+ (does not require PowerShell 7).
    /// </summary>
    public class PowerShellCompletionService : IDisposable
    {
        private Process _powerShellProcess;
        private StreamWriter _inputWriter;
        private StreamReader _outputReader;
        private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;
        private readonly string _modulePath;

        public PowerShellCompletionService(string modulePath = null)
        {
            _modulePath = modulePath;
        }

        /// <summary>
        /// Initializes the PowerShell process and loads the module.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                // Start PowerShell process
                // Use "powershell.exe" for PowerShell 5.1 compatibility on Windows
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -NoLogo -NonInteractive",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                _powerShellProcess = Process.Start(startInfo);
                _inputWriter = _powerShellProcess.StandardInput;
                _outputReader = _powerShellProcess.StandardOutput;

                // Set output encoding and configure environment
                await SendCommandAsync("$OutputEncoding = [System.Text.Encoding]::UTF8");
                await SendCommandAsync("[Console]::OutputEncoding = [System.Text.Encoding]::UTF8");
                
                // Load the module if path is provided
                if (!string.IsNullOrEmpty(_modulePath))
                {
                    // Import the module
                    string importCommand = $"Import-Module '{_modulePath}' -ErrorAction SilentlyContinue";
                    await SendCommandAsync(importCommand);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize PowerShell completion service: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets code completions for the given script at the specified cursor position.
        /// </summary>
        public async Task<List<CompletionItem>> GetCompletionsAsync(string script, int cursorPosition)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            await _requestLock.WaitAsync();
            try
            {
                // Use a Base64-encoded script to avoid escaping issues
                byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
                string encodedScript = Convert.ToBase64String(scriptBytes);
                
                // Create a PowerShell command that returns JSON with a clear marker
                string marker = Guid.NewGuid().ToString();
                string command = $@"
$encodedScript = '{encodedScript}'
$scriptBytes = [System.Convert]::FromBase64String($encodedScript)
$decodedScript = [System.Text.Encoding]::UTF8.GetString($scriptBytes)
$result = TabExpansion2 $decodedScript {cursorPosition}
Write-Output '###START_{marker}###'
if ($result -and $result.CompletionMatches) {{
    $completions = @($result.CompletionMatches | ForEach-Object {{
        @{{
            CompletionText = $_.CompletionText
            ListItemText = $_.ListItemText
            ResultType = [int]$_.ResultType
            ToolTip = $_.ToolTip
        }}
    }})
    ($completions | ConvertTo-Json -Compress -Depth 2)
}} else {{
    '[]'
}}
Write-Output '###END_{marker}###'
";

                // Send command
                await _inputWriter.WriteLineAsync(command);
                await _inputWriter.FlushAsync();
                
                // Read the JSON response between markers
                string jsonResponse = await ReadResponseBetweenMarkersAsync($"###START_{marker}###", $"###END_{marker}###");
                
                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    return new List<CompletionItem>();
                }

                // Parse the JSON
                var completions = ParseCompletions(jsonResponse);
                return completions;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting completions: {ex.Message}");
                return new List<CompletionItem>();
            }
            finally
            {
                _requestLock.Release();
            }
        }

        private async Task SendCommandAsync(string command)
        {
            // Write the command with a unique marker
            string marker = Guid.NewGuid().ToString();
            await _inputWriter.WriteLineAsync(command);
            await _inputWriter.WriteLineAsync($"Write-Output '###MARKER_{marker}###'");
            await _inputWriter.FlushAsync();

            // Read until we see the marker
            while (true)
            {
                string line = await _outputReader.ReadLineAsync();
                if (line != null && line.Contains($"###MARKER_{marker}###"))
                {
                    break;
                }
            }
        }

        private async Task<string> ReadResponseBetweenMarkersAsync(string startMarker, string endMarker)
        {
            var sb = new StringBuilder();
            bool capturing = false;

            // Read until we find both markers
            while (true)
            {
                string line = await _outputReader.ReadLineAsync();
                if (line == null)
                    break;

                if (line.Contains(startMarker))
                {
                    capturing = true;
                    continue;
                }

                if (line.Contains(endMarker))
                {
                    break;
                }

                if (capturing)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString().Trim();
        }

        private List<CompletionItem> ParseCompletions(string json)
        {
            var completions = new List<CompletionItem>();
            
            try
            {
                // Handle both array and single object
                if (json.StartsWith("["))
                {
                    var items = JsonSerializer.Deserialize<List<JsonElement>>(json);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            completions.Add(ParseCompletionItem(item));
                        }
                    }
                }
                else if (json.StartsWith("{"))
                {
                    var item = JsonSerializer.Deserialize<JsonElement>(json);
                    completions.Add(ParseCompletionItem(item));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing completions JSON: {ex.Message}");
            }

            return completions;
        }

        private CompletionItem ParseCompletionItem(JsonElement element)
        {
            string completionText = element.TryGetProperty("CompletionText", out var ct) ? ct.GetString() : "";
            string listItemText = element.TryGetProperty("ListItemText", out var lit) ? lit.GetString() : "";
            int resultType = element.TryGetProperty("ResultType", out var rt) ? rt.GetInt32() : 0;
            string toolTip = element.TryGetProperty("ToolTip", out var tt) ? tt.GetString() : "";

            return new CompletionItem
            {
                CompletionText = completionText,
                ListItemText = listItemText,
                ResultType = (CompletionResultType)resultType,
                ToolTip = toolTip
            };
        }

        public void Dispose()
        {
            _requestLock?.Dispose();
            
            if (_inputWriter != null)
            {
                try
                {
                    _inputWriter.WriteLine("exit");
                    _inputWriter.Flush();
                    _inputWriter.Dispose();
                }
                catch { }
            }

            if (_powerShellProcess != null && !_powerShellProcess.HasExited)
            {
                try
                {
                    _powerShellProcess.Kill();
                    _powerShellProcess.Dispose();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Represents a code completion item.
    /// </summary>
    public class CompletionItem
    {
        public string CompletionText { get; set; }
        public string ListItemText { get; set; }
        public CompletionResultType ResultType { get; set; }
        public string ToolTip { get; set; }
    }

    /// <summary>
    /// PowerShell completion result types (from System.Management.Automation.CompletionResultType).
    /// </summary>
    public enum CompletionResultType
    {
        Text = 0,
        History = 1,
        Command = 2,
        ProviderItem = 3,
        ProviderContainer = 4,
        Property = 5,
        Method = 6,
        ParameterName = 7,
        ParameterValue = 8,
        Variable = 9,
        Namespace = 10,
        Type = 11,
        Keyword = 12,
        DynamicKeyword = 13
    }
}
