using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell.EditorServices;
using Microsoft.PowerShell.EditorServices.Session;
using System.Management.Automation.Language;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service that provides PowerShell code completion using PowerShell Editor Services.
    /// </summary>
    public class PowerShellCompletionService : IDisposable
    {
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;
        private readonly string _modulePath;

        private PowerShellContext _powerShellContext;
        private LanguageService _languageService;
        private Workspace _workspace;

        public bool IsInitialized => _isInitialized;

        public event EventHandler CompletionRequestStarted;
        public event EventHandler CompletionRequestCompleted;
        public event EventHandler<CompletionErrorEventArgs> CompletionRequestFailed;

        public PowerShellCompletionService(string modulePath = null)
        {
            _modulePath = modulePath;
        }

        /// <summary>
        /// Initializes the PowerShell runspace and loads the module.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized) return;

                var hostDetails = new HostDetails(
                    "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin",
                    "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin",
                    new Version(1, 0, 0));

                _powerShellContext = new PowerShellContext(hostDetails);
                _workspace = new Workspace(_powerShellContext.PowerShellVersion);
                _languageService = new LanguageService(_powerShellContext);

                // Load the module if path is provided
                if (!string.IsNullOrEmpty(_modulePath))
                {
                    await _powerShellContext.ExecuteScriptString($"Import-Module '{_modulePath}'");
                }

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Gets code completions for the given script at the specified cursor position.
        /// </summary>
        public async Task<List<CompletionItem>> GetCompletionsAsync(string script, int cursorPosition)
        {
            CompletionRequestStarted?.Invoke(this, EventArgs.Empty);

            if (!_isInitialized)
            {
                var initCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _initLock.WaitAsync(initCts.Token);
                try
                {
                    if (!_isInitialized)
                    {
                        await InitializeAsync();
                    }
                }
                finally
                {
                    _initLock.Release();
                }
            }

            try
            {
                // Calculate line and column from cursorPosition (0-based index)
                int line = 1;
                int column = 1;
                for (int i = 0; i < cursorPosition && i < script.Length; i++)
                {
                    if (script[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                }

                // Create a ScriptFile for the current script content
                var scriptFile = new ScriptFile(
                    "untitled:script.ps1", 
                    "untitled:script.ps1", 
                    script, 
                    _powerShellContext.PowerShellVersion);

                var result = await _languageService.GetCompletionsInFile(scriptFile, line, column);

                // Find related command for parameters
                string relatedCommand = null;
                try
                {
                    var ast = Parser.ParseInput(script, out Token[] tokens, out ParseError[] errors);
                    var commandAst = ast.Find(a => a is CommandAst && a.Extent.StartOffset <= cursorPosition && a.Extent.EndOffset >= cursorPosition, true) as CommandAst;
                    if (commandAst != null)
                    {
                        relatedCommand = commandAst.GetCommandName();
                    }
                }
                catch { }

                var completions = new List<CompletionItem>();
                if (result != null && result.Completions != null)
                {
                    foreach (var match in result.Completions)
                    {
                        completions.Add(new CompletionItem
                        {
                            CompletionText = match.CompletionText,
                            ListItemText = match.ListItemText,
                            ResultType = MapCompletionType(match.CompletionType),
                            ToolTip = match.ToolTipText,
                            RelatedCommand = relatedCommand
                        });
                    }
                }
                
                CompletionRequestCompleted?.Invoke(this, EventArgs.Empty);
                return completions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompletionsAsync: {ex.Message}");
                CompletionRequestFailed?.Invoke(this, new CompletionErrorEventArgs { ErrorMessage = ex.Message });
                return new List<CompletionItem>();
            }
        }

        private CompletionResultType MapCompletionType(CompletionType type)
        {
            switch (type)
            {
                case CompletionType.Command: return CompletionResultType.Command;
                case CompletionType.Method: return CompletionResultType.Method;
                case CompletionType.ParameterName: return CompletionResultType.ParameterName;
                case CompletionType.ParameterValue: return CompletionResultType.ParameterValue;
                case CompletionType.Property: return CompletionResultType.Property;
                case CompletionType.Variable: return CompletionResultType.Variable;
                case CompletionType.Namespace: return CompletionResultType.Namespace;
                case CompletionType.Type: return CompletionResultType.Type;
                case CompletionType.Keyword: return CompletionResultType.Keyword;
                case CompletionType.Path: return CompletionResultType.ProviderItem;
                case CompletionType.Unknown:
                default: return CompletionResultType.Text;
            }
        }

        public async Task<CompletionItem> GetCompletionDetailsAsync(CompletionItem item)
        {
            if (string.IsNullOrEmpty(item.CompletionText)) return item;

            if (item.ResultType == CompletionResultType.Command)
            {
                string help = await GetHelpContentAsync(item.CompletionText, null);
                if (!string.IsNullOrEmpty(help)) item.ToolTip = help;
            }
            else if (item.ResultType == CompletionResultType.ParameterName && !string.IsNullOrEmpty(item.RelatedCommand))
            {
                string help = await GetHelpContentAsync(item.RelatedCommand, item.CompletionText);
                if (!string.IsNullOrEmpty(help)) item.ToolTip = help;
            }
            return item;
        }

        private async Task<string> GetHelpContentAsync(string command, string parameter)
        {
            try
            {
                string script;
                if (string.IsNullOrEmpty(parameter))
                {
                    script = $@"
$ErrorActionPreference = 'Stop'
$cmd = Get-Command '{command}' -ErrorAction SilentlyContinue
if (-not $cmd) {{ 
    ""Command '{command}' not found. Loaded modules: $((Get-Module).Name -join ', ')"" 
}} else {{
    $help = Get-Help '{command}' -ErrorAction SilentlyContinue
    if ($help) {{
        $help | Out-String
    }} else {{
        ""No help object returned for '{command}'. Syntax:`n$($cmd.Definition)""
    }}
}}
";
                }
                else
                {
                    string paramName = parameter.TrimStart('-');
                    script = $@"
$ErrorActionPreference = 'Stop'
$help = Get-Help '{command}' -Parameter '{paramName}' -ErrorAction SilentlyContinue
if ($help) {{
    $help | Out-String
}} else {{
    ""No help found for parameter '{paramName}'""
}}
";
                }

                var results = await _powerShellContext.ExecuteScriptString(script, false, false);
                
                if (results != null && results.Any())
                {
                    var text = string.Join(Environment.NewLine, results.Select(r => r?.ToString()));
                    return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
                }
                
                return "No results returned from help script.";
            }
            catch (Exception ex)
            {
                return $"Error fetching help: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _powerShellContext?.Dispose();
            _initLock?.Dispose();
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
        public string RelatedCommand { get; set; }
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

    /// <summary>
    /// Event arguments for completion request errors.
    /// </summary>
    public class CompletionErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}