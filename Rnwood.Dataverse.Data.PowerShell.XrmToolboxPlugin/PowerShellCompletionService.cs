using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service that provides PowerShell code completion using TabExpansion2 API.
    /// Compatible with PowerShell 5.1+ (does not require PowerShell 7).
    /// </summary>
    public class PowerShellCompletionService : IDisposable
    {
        private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;
        private readonly string _modulePath;
        private readonly string _accessToken;
        private readonly string _url;

        public PowerShellCompletionService(string modulePath = null, string accessToken = null, string url = null)
        {
            _modulePath = modulePath;
            _accessToken = accessToken;
            _url = url;
        }

        /// <summary>
        /// Initializes the PowerShell process and loads the module.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
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
                // Add debugging output for the request
                string debugLine = GetLineWithCursor(script, cursorPosition);
                Console.WriteLine($"Request: {debugLine}");

                // Build the PowerShell command
                string command = BuildCommand(script, cursorPosition);

                // Execute the command using CliWrap
                var result = await Cli.Wrap("powershell.exe")
                    .WithArguments($"-Command \"{command}\"")
                    .ExecuteBufferedAsync();

                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"PowerShell command failed with exit code {result.ExitCode}: {result.StandardError}");
                    return new List<CompletionItem>();
                }

                // Parse the JSON response
                string jsonResponse = ParseResponseBetweenMarkers(result.StandardOutput);
                
                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    return new List<CompletionItem>();
                }

                // Parse the JSON
                var completions = ParseCompletions(jsonResponse);
                Console.WriteLine($"Results count: {completions.Count}");
                return completions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<CompletionItem>();
            }
            finally
            {
                _requestLock.Release();
            }
        }

        private string BuildCommand(string script, int cursorPosition)
        {
            var sb = new StringBuilder();

            // Set output encoding
            sb.Append("$OutputEncoding = [System.Text.Encoding]::UTF8; ");
            sb.Append("[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; ");

            // Load the module if path is provided
            if (!string.IsNullOrEmpty(_modulePath))
            {
                sb.Append($"Import-Module '{_modulePath}' -ErrorAction SilentlyContinue; ");

                // Only add connection if we have both token and url
                if (!string.IsNullOrEmpty(_accessToken) && !string.IsNullOrEmpty(_url))
                {
                    sb.Append($"Get-DataverseConnection -AccessToken {{'{_accessToken}'}} -Url '{_url}' -SetAsDefault -ErrorAction Stop; ");
                }
            }

            // Use a Base64-encoded script to avoid escaping issues
            byte[] scriptBytes = Encoding.UTF8.GetBytes(script);
            string encodedScript = Convert.ToBase64String(scriptBytes);
            
            // Create a PowerShell command that returns JSON with a clear marker
            string marker = Guid.NewGuid().ToString();
            sb.Append($@"
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
".Trim());

            return sb.ToString();
        }

        private string ParseResponseBetweenMarkers(string output)
        {
            string startMarker = "###START_";
            string endMarker = "###END_";

            int startIndex = output.IndexOf(startMarker);
            if (startIndex == -1) return "";

            startIndex += startMarker.Length;
            int markerEnd = output.IndexOf("###", startIndex);
            if (markerEnd == -1) return "";
            string marker = output.Substring(startIndex, markerEnd - startIndex);
            startMarker = $"###START_{marker}###";
            endMarker = $"###END_{marker}###";

            startIndex = output.IndexOf(startMarker);
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

        public void Dispose()
        {
            _requestLock?.Dispose();
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
