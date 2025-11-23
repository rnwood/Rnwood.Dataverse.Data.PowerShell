using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using System.IO;
using System.Diagnostics;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service that provides PowerShell code completion using TabExpansion2 API.
    /// Compatible with PowerShell 5.1+ (does not require PowerShell 7).
    /// Uses a persistent PowerShell process for fast completions.
    /// </summary>
    public class PowerShellCompletionService : IDisposable
    {
        private bool _isInitialized = false;
        private readonly string _modulePath;
        private readonly Func<string> _accessTokenProvider;
        private readonly string _url;
        private Process _powerShellProcess;
        private StreamWriter _stdin;
        private StreamReader _stdout;
        private StreamReader _stderr;
        private bool _disposed = false;

        public PowerShellCompletionService(string modulePath = null, Func<string> accessTokenProvider = null, string url = null)
        {
            _modulePath = modulePath;
            _accessTokenProvider = accessTokenProvider;
            _url = url;
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
                if (_isInitialized)
                    return;

                Console.WriteLine("Initializing persistent PowerShell process...");

                _powerShellProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoExit -NoLogo -NoProfile -Command -",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                _powerShellProcess.Start();
                _stdin = _powerShellProcess.StandardInput;
                _stdout = _powerShellProcess.StandardOutput;
                _stderr = _powerShellProcess.StandardError;

                await SendInitializationCommandsAsync();

                _isInitialized = true;
                Console.WriteLine("PowerShell process initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing PowerShell process: {ex.Message}");
                CleanupProcess();
                throw;
            }
        }

        private async Task SendInitializationCommandsAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine("$OutputEncoding = [System.Text.Encoding]::UTF8");
            sb.AppendLine("[Console]::OutputEncoding = [System.Text.Encoding]::UTF8");
            sb.AppendLine("$ErrorActionPreference = 'SilentlyContinue'");

            if (!string.IsNullOrEmpty(_modulePath))
            {
                sb.AppendLine($"Import-Module '{_modulePath}' -ErrorAction SilentlyContinue");

                if (_accessTokenProvider != null && !string.IsNullOrEmpty(_url))
                {
                    string accessToken = _accessTokenProvider();
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        sb.AppendLine($"Get-DataverseConnection -AccessToken {{'{accessToken}'}} -Url '{_url}' -SetAsDefault -ErrorAction SilentlyContinue");
                    }
                }
            }

            string initMarker = Guid.NewGuid().ToString();
            sb.AppendLine($"Write-Output '###INIT_COMPLETE_{initMarker}###'");

            await _stdin.WriteAsync(sb.ToString());
            await _stdin.FlushAsync();

            string line;
            while ((line = await _stdout.ReadLineAsync()) != null)
            {
                if (line.Contains($"###INIT_COMPLETE_{initMarker}###"))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Refreshes the Dataverse connection with a new access token.
        /// </summary>
        public async Task RefreshConnectionAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
                return;
            }

            try
            {
                if (_accessTokenProvider != null && !string.IsNullOrEmpty(_url))
                {
                    string accessToken = _accessTokenProvider();
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var sb = new StringBuilder();
                        string marker = Guid.NewGuid().ToString();
                        
                        sb.AppendLine($"Get-DataverseConnection -AccessToken {{'{accessToken}'}} -Url '{_url}' -SetAsDefault -ErrorAction SilentlyContinue");
                        sb.AppendLine($"Write-Output '###REFRESH_COMPLETE_{marker}###'");

                        await _stdin.WriteAsync(sb.ToString());
                        await _stdin.FlushAsync();

                        string line;
                        while ((line = await _stdout.ReadLineAsync()) != null)
                        {
                            if (line.Contains($"###REFRESH_COMPLETE_{marker}###"))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing connection: {ex.Message}");
                _isInitialized = false;
                CleanupProcess();
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

            try
            {
                if (_powerShellProcess == null || _powerShellProcess.HasExited)
                {
                    Console.WriteLine("PowerShell process has exited, reinitializing...");
                    _isInitialized = false;
                    CleanupProcess();
                    await InitializeAsync();
                }

                string debugLine = GetLineWithCursor(script, cursorPosition);
                Console.WriteLine($"Request: {debugLine}");

                string command = BuildCompletionCommand(script, cursorPosition);

                await _stdin.WriteAsync(command);
                await _stdin.FlushAsync();

                string marker = ExtractMarkerFromCommand(command);
                string output = await ReadUntilMarkerAsync(marker);

                string jsonResponse = ParseResponseBetweenMarkers(output, marker);
                
                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    return new List<CompletionItem>();
                }

                var completions = ParseCompletions(jsonResponse);
                Console.WriteLine($"Results count: {completions.Count}");
                return completions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _isInitialized = false;
                CleanupProcess();
                return new List<CompletionItem>();
            }
        }

        private string BuildCompletionCommand(string script, int cursorPosition)
        {
            byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
            string encodedScript = Convert.ToBase64String(scriptBytes);
            
            string marker = Guid.NewGuid().ToString();
            
            var sb = new StringBuilder();
            sb.AppendLine($"$encodedScript = '{encodedScript}'");
            sb.AppendLine("$scriptBytes = [System.Convert]::FromBase64String($encodedScript)");
            sb.AppendLine("$decodedScript = [System.Text.Encoding]::UTF8.GetString($scriptBytes)");
            sb.AppendLine($"$result = TabExpansion2 $decodedScript {cursorPosition}");
            sb.AppendLine($"Write-Output '###START_{marker}###'");
            sb.AppendLine("if ($result -and $result.CompletionMatches) {");
            sb.AppendLine("    $completionResults = @()");
            sb.AppendLine("    foreach ($match in $result.CompletionMatches) {");
            sb.AppendLine("        $completionResult = [PSCustomObject]@{");
            sb.AppendLine("            CompletionText = $match.CompletionText");
            sb.AppendLine("            ListItemText = $match.ListItemText");
            sb.AppendLine("            ResultType = [int]$match.ResultType");
            sb.AppendLine("            ToolTip = $match.ToolTip");
            sb.AppendLine("        }");
            sb.AppendLine("        $completionResults += $completionResult");
            sb.AppendLine("    }");
            sb.AppendLine("    $completionResults | ConvertTo-Json -Compress -Depth 2");
            sb.AppendLine("} else {");
            sb.AppendLine("    '[]'");
            sb.AppendLine("}");
            sb.AppendLine($"Write-Output '###END_{marker}###'");

            return sb.ToString();
        }

        private string ExtractMarkerFromCommand(string command)
        {
            int startIndex = command.IndexOf("###START_");
            if (startIndex == -1) return Guid.NewGuid().ToString();
            
            startIndex += "###START_".Length;
            int endIndex = command.IndexOf("###", startIndex);
            if (endIndex == -1) return Guid.NewGuid().ToString();
            
            return command.Substring(startIndex, endIndex - startIndex);
        }

        private async Task<string> ReadUntilMarkerAsync(string marker)
        {
            var sb = new StringBuilder();
            string endMarker = $"###END_{marker}###";
            bool foundStart = false;

            string line;
            while ((line = await _stdout.ReadLineAsync()) != null)
            {
                sb.AppendLine(line);
                
                if (!foundStart && line.Contains($"###START_{marker}###"))
                {
                    foundStart = true;
                }
                
                if (foundStart && line.Contains(endMarker))
                {
                    break;
                }
            }

            return sb.ToString();
        }

        private string ParseResponseBetweenMarkers(string output, string marker)
        {
            string startMarker = $"###START_{marker}###";
            string endMarker = $"###END_{marker}###";

            int startIndex = output.IndexOf(startMarker);
            if (startIndex == -1) return "";
            startIndex += startMarker.Length;

            int endIndex = output.IndexOf(endMarker, startIndex);
            if (endIndex == -1) return "";

            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private List<CompletionItem> ParseCompletions(string json)
        {
            var completions = new List<CompletionItem>();
            
            try
            {
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
                Console.WriteLine($"Error parsing completions JSON: {ex.Message}");
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

        private string GetLineWithCursor(string script, int cursorPosition)
        {
            int lineStart = script.LastIndexOf('\n', Math.Max(0, cursorPosition - 1)) + 1;
            int lineEnd = script.IndexOf('\n', cursorPosition);
            if (lineEnd == -1) lineEnd = script.Length;
            string line = script.Substring(lineStart, lineEnd - lineStart);
            int cursorInLine = cursorPosition - lineStart;
            if (cursorInLine < 0) cursorInLine = 0;
            if (cursorInLine > line.Length) cursorInLine = line.Length;
            string withCursor = line.Insert(cursorInLine, "|");
            return $"\"{withCursor}\"";
        }

        private void CleanupProcess()
        {
            try
            {
                _stdin?.Dispose();
                _stdout?.Dispose();
                _stderr?.Dispose();

                if (_powerShellProcess != null && !_powerShellProcess.HasExited)
                {
                    _powerShellProcess.Kill();
                }

                _powerShellProcess?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up PowerShell process: {ex.Message}");
            }
            finally
            {
                _stdin = null;
                _stdout = null;
                _stderr = null;
                _powerShellProcess = null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CleanupProcess();
        }
    }

    /// <summary>
    /// Represents a code completion item.
    /// </summary>
    [Serializable]
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
    [Serializable]
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
