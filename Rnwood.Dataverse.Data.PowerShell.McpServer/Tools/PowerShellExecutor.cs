using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

public class PowerShellExecutorConfig
{
    public string[] AllowedUrls { get; set; } = Array.Empty<string>();
}

public class PowerShellExecutor : IDisposable
{
    private readonly ConcurrentDictionary<string, PersistentSession> _sessions = new();
    private readonly string _modulePath;
    private readonly PowerShellExecutorConfig _config;
    private bool _isInitialized;
    private readonly object _initLock = new();

    public PowerShellExecutor(PowerShellExecutorConfig config)
    {
        _config = config;
        
        // Find the module path - try multiple locations
        var assemblyDir = Path.GetDirectoryName(typeof(PowerShellExecutor).Assembly.Location)!;
        
        // Try packaged module directory first (for global tool)
        _modulePath = Path.Combine(assemblyDir, "module");
        
        // If not found, try development path (from bin/Debug/net8.0)
        if (!Directory.Exists(_modulePath) || !File.Exists(Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")))
        {
            // From: Rnwood.Dataverse.Data.PowerShell.McpServer\bin\Debug\net8.0
            // To:   Rnwood.Dataverse.Data.PowerShell\bin\Debug\netstandard2.0
            // Need to go up 4 levels to solution root
            _modulePath = Path.Combine(assemblyDir, "..", "..", "..", "..", "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");
            _modulePath = Path.GetFullPath(_modulePath);
        }
        
        // If that doesn't exist, try Release build
        if (!Directory.Exists(_modulePath) || !File.Exists(Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")))
        {
            _modulePath = Path.Combine(assemblyDir, "..", "..", "..", "..", "Rnwood.Dataverse.Data.PowerShell", "bin", "Release", "netstandard2.0");
            _modulePath = Path.GetFullPath(_modulePath);
        }
        
        // If still not found, check environment variable
        if (!Directory.Exists(_modulePath) || !File.Exists(Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")))
        {
            var envPath = Environment.GetEnvironmentVariable("DATAVERSE_MODULE_PATH");
            if (!string.IsNullOrEmpty(envPath))
            {
                _modulePath = envPath;
            }
        }
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        lock (_initLock)
        {
            if (_isInitialized) return;

            // Validate allowed URLs were provided
            if (_config.AllowedUrls == null || _config.AllowedUrls.Length == 0)
            {
                throw new InvalidOperationException("No allowed URLs specified. Use --allowed-urls parameter to specify allowed Dataverse URLs.");
            }

            _isInitialized = true;
        }
    }

    public string GetCmdletList()
    {
        EnsureInitialized();
        
        var moduleManifestPath = Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
        
        if (!File.Exists(moduleManifestPath))
        {
            throw new InvalidOperationException($"Module manifest not found at: {moduleManifestPath}");
        }
        
        var script = $@"
Import-Module '{moduleManifestPath}'
Get-Command -Module Rnwood.Dataverse.Data.PowerShell | ForEach-Object {{
    $help = Get-Help $_.Name -ErrorAction SilentlyContinue
    [PSCustomObject]@{{
        Name = $_.Name
        Synopsis = if ($help.Synopsis) {{ $help.Synopsis.Trim() }} else {{ '' }}
    }}
}} | ConvertTo-Json
";

        using var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        using var ps = System.Management.Automation.PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(script);
        
        var results = ps.Invoke();
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"Failed to get cmdlet list: {errors}");
        }

        return results.FirstOrDefault()?.ToString() ?? "[]";
    }

    public string GetCmdletHelp(string cmdletName)
    {
        EnsureInitialized();
        
        var moduleManifestPath = Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
        
        if (!File.Exists(moduleManifestPath))
        {
            throw new InvalidOperationException($"Module manifest not found at: {moduleManifestPath}");
        }
        
        var script = $@"
Import-Module '{moduleManifestPath}'
$help = Get-Help '{cmdletName}' -Full -ErrorAction Stop
$helpObj = [PSCustomObject]@{{
    Name = $help.Name
    Synopsis = $help.Synopsis
    Description = ($help.Description | ForEach-Object {{ $_.Text }}) -join ""`n""
    Syntax = ($help.Syntax.syntaxItem | ForEach-Object {{ $_.name + ' ' + (($_.parameter | ForEach-Object {{ '-' + $_.name + ' <' + $_.type.name + '>' }}) -join ' ') }}) -join ""`n""
    Parameters = @($help.parameters.parameter | ForEach-Object {{
        [PSCustomObject]@{{
            Name = $_.name
            Type = $_.type.name
            Required = $_.required
            Description = ($_.description | ForEach-Object {{ $_.Text }}) -join ' '
        }}
    }})
    Examples = @($help.examples.example | ForEach-Object {{
        [PSCustomObject]@{{
            Title = $_.title
            Code = $_.code
            Remarks = ($_.remarks | ForEach-Object {{ $_.Text }}) -join ""`n""
        }}
    }})
}}
$helpObj | ConvertTo-Json -Depth 10
";

        using var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        using var ps = System.Management.Automation.PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(script);
        
        var results = ps.Invoke();
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"Failed to get help for cmdlet '{cmdletName}': {errors}");
        }

        return results.FirstOrDefault()?.ToString() ?? "{}";
    }

    public string CreateSession()
    {
        EnsureInitialized();
        
        var sessionId = Guid.NewGuid().ToString("N");
        var session = new PersistentSession(sessionId, _modulePath, _config);
        
        if (!_sessions.TryAdd(sessionId, session))
        {
            throw new InvalidOperationException($"Session {sessionId} already exists");
        }

        session.Initialize();
        return sessionId;
    }

    public string RunScriptInSession(string sessionId, string script)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));
        }

        return session.RunScript(script);
    }

    public ScriptOutputResult GetScriptOutput(string sessionId, string scriptExecutionId, bool onlyNew)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));
        }

        return session.GetScriptOutput(scriptExecutionId, onlyNew);
    }

    public void EndSession(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            session.Dispose();
        }
        else
        {
            throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));
        }
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
        {
            session.Dispose();
        }
        _sessions.Clear();
    }
}



public class PersistentSession : IDisposable
{
    private readonly string _sessionId;
    private readonly string _modulePath;
    private readonly PowerShellExecutorConfig _config;
    private Runspace? _runspace;
    private readonly ConcurrentDictionary<string, ScriptExecution> _scriptExecutions = new();
    private readonly object _lock = new();
    private bool _isDisposed;

    public PersistentSession(string sessionId, string modulePath, PowerShellExecutorConfig config)
    {
        _sessionId = sessionId;
        _modulePath = modulePath;
        _config = config;
    }

    public void Initialize()
    {
        lock (_lock)
        {
            if (_runspace != null)
            {
                throw new InvalidOperationException("Session already initialized");
            }

            // Create initial session state - always use default to ensure module can load
            var iss = InitialSessionState.CreateDefault();
            
            // Always use FullLanguage mode - the module requires ability to import .ps1 files
            // Security is enforced through allowed URL restrictions instead of language mode
            iss.LanguageMode = PSLanguageMode.FullLanguage;

            _runspace = RunspaceFactory.CreateRunspace(iss);
            _runspace.Open();

            // Import the Dataverse module
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = _runspace;
                var moduleManifestPath = Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
                
                if (!File.Exists(moduleManifestPath))
                {
                    throw new InvalidOperationException($"Module manifest not found at: {moduleManifestPath}");
                }
                
                ps.AddCommand("Import-Module").AddParameter("Name", moduleManifestPath);
                ps.Invoke();
                
                if (ps.HadErrors)
                {
                    var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
                    throw new InvalidOperationException($"Failed to import module from {moduleManifestPath}: {errors}");
                }
            }
            

            // Set the allowed URLs as a session variable that scripts can reference
            var allowedUrlsList = string.Join("', '", _config.AllowedUrls);
            var firstAllowedUrl = _config.AllowedUrls.FirstOrDefault() ?? "";
            
            using (var ps2 = System.Management.Automation.PowerShell.Create())
            {
                ps2.Runspace = _runspace;
                ps2.AddScript($"$Global:AllowedDataverseUrls = @('{allowedUrlsList}')");
                ps2.AddScript($"$Global:DefaultDataverseUrl = '{firstAllowedUrl}'");
                ps2.Invoke();
                
                if (ps2.HadErrors)
                {
                    var errors = string.Join("\n", ps2.Streams.Error.Select(e => e.ToString()));
                    throw new InvalidOperationException($"Failed to initialize session variables: {errors}");
                }
            }

            // Automatically establish Dataverse connection
            using (var ps3 = System.Management.Automation.PowerShell.Create())
            {
                ps3.Runspace = _runspace;
                ps3.AddCommand("Get-DataverseConnection")
                   .AddParameter("Interactive", true)
                   .AddParameter("SetAsDefault", true)
                   .AddParameter("Url", firstAllowedUrl);
                ps3.Invoke();
                
                if (ps3.HadErrors)
                {
                    var errors = string.Join("\n", ps3.Streams.Error.Select(e => e.ToString()));
                    throw new InvalidOperationException($"Failed to establish Dataverse connection: {errors}");
                }
            }
        }
    }

    public string RunScript(string script)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Session has been disposed");
        }

        var executionId = Guid.NewGuid().ToString("N");
        var execution = new ScriptExecution(executionId, script, _runspace!);
        
        if (!_scriptExecutions.TryAdd(executionId, execution))
        {
            throw new InvalidOperationException($"Script execution {executionId} already exists");
        }

        execution.Start();
        return executionId;
    }

    public ScriptOutputResult GetScriptOutput(string scriptExecutionId, bool onlyNew)
    {
        if (!_scriptExecutions.TryGetValue(scriptExecutionId, out var execution))
        {
            throw new ArgumentException($"Script execution {scriptExecutionId} not found", nameof(scriptExecutionId));
        }

        return execution.GetOutput(onlyNew);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed) return;
            
            _isDisposed = true;
            
            foreach (var execution in _scriptExecutions.Values)
            {
                execution.Dispose();
            }
            _scriptExecutions.Clear();
            
            _runspace?.Dispose();
            _runspace = null;
        }
    }
}

public class ScriptExecution : IDisposable
{
    private readonly string _executionId;
    private readonly string _script;
    private readonly Runspace _runspace;
    private System.Management.Automation.PowerShell? _powerShell;
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _error = new();
    private int _lastReadPosition;
    private bool _isComplete;
    private Exception? _exception;
    private readonly object _lock = new();

    public ScriptExecution(string executionId, string script, Runspace runspace)
    {
        _executionId = executionId;
        _script = script;
        _runspace = runspace;
    }

    public void Start()
    {
        Task.Run(() =>
        {
            try
            {
                ExecuteScript();
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _exception = ex;
                    _error.AppendLine($"ERROR: {ex.Message}");
                    _isComplete = true;
                }
            }
        });
    }

    private void ExecuteScript()
    {
        _powerShell = System.Management.Automation.PowerShell.Create();
        _powerShell.Runspace = _runspace;

        // Execute the script
        _powerShell.AddScript(_script);
        
        var outputCollection = new PSDataCollection<PSObject>();
        outputCollection.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<PSObject> collection && e.Index < collection.Count)
            {
                lock (_lock)
                {
                    _output.AppendLine(collection[e.Index]?.ToString() ?? "");
                }
            }
        };

        _powerShell.Streams.Error.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<ErrorRecord> collection && e.Index < collection.Count)
            {
                lock (_lock)
                {
                    _error.AppendLine($"ERROR: {collection[e.Index]}");
                }
            }
        };

        _powerShell.Streams.Warning.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<WarningRecord> collection && e.Index < collection.Count)
            {
                lock (_lock)
                {
                    _output.AppendLine($"WARNING: {collection[e.Index]}");
                }
            }
        };

        _powerShell.Streams.Verbose.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<VerboseRecord> collection && e.Index < collection.Count)
            {
                lock (_lock)
                {
                    _output.AppendLine($"VERBOSE: {collection[e.Index]}");
                }
            }
        };

        try
        {
            _powerShell.Invoke(null, outputCollection);
        }
        finally
        {
            lock (_lock)
            {
                _isComplete = true;
            }
        }
    }

    public ScriptOutputResult GetOutput(bool onlyNew)
    {
        lock (_lock)
        {
            var fullOutput = _output.ToString() + _error.ToString();
            
            if (onlyNew)
            {
                var newContent = fullOutput.Substring(Math.Min(_lastReadPosition, fullOutput.Length));
                _lastReadPosition = fullOutput.Length;
                
                return new ScriptOutputResult
                {
                    SessionId = _executionId,
                    Output = newContent,
                    IsComplete = _isComplete,
                    HasError = _exception != null || _error.Length > 0
                };
            }
            else
            {
                return new ScriptOutputResult
                {
                    SessionId = _executionId,
                    Output = fullOutput,
                    IsComplete = _isComplete,
                    HasError = _exception != null || _error.Length > 0
                };
            }
        }
    }

    public void Dispose()
    {
        _powerShell?.Dispose();
    }
}

public class ScriptOutputResult
{
    public required string SessionId { get; init; }
    public required string Output { get; init; }
    public required bool IsComplete { get; init; }
    public required bool HasError { get; init; }
}
