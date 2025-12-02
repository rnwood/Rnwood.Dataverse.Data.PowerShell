using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell.EditorServices;
using Microsoft.PowerShell.EditorServices.Session;

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
                    await _powerShellContext.ExecuteScriptString($"Import-Module '{_modulePath}' -ErrorAction SilentlyContinue");
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
                            ToolTip = match.ToolTipText
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
                default: return CompletionResultType.Text;
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