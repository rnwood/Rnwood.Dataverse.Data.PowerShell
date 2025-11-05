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

public class PowerShellExecutor : IDisposable
{
    private readonly ConcurrentDictionary<string, ScriptSession> _sessions = new();
    private readonly string _modulePath;
    private readonly string? _connectionName;
    private bool _isInitialized;
    private readonly object _initLock = new();

    public PowerShellExecutor(string? connectionName = null)
    {
        _connectionName = connectionName;
        
        // Find the module path - try multiple locations
        var assemblyDir = Path.GetDirectoryName(typeof(PowerShellExecutor).Assembly.Location)!;
        
        // Try development path first (from bin/Debug/net8.0)
        _modulePath = Path.Combine(assemblyDir, "..", "..", "..", "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");
        _modulePath = Path.GetFullPath(_modulePath);
        
        // If that doesn't exist, try relative to current directory
        if (!Directory.Exists(_modulePath) || !File.Exists(Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")))
        {
            // Try from Release build
            _modulePath = Path.Combine(assemblyDir, "..", "..", "..", "Rnwood.Dataverse.Data.PowerShell", "bin", "Release", "netstandard2.0");
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

            // Test that we can load the connection
            if (!string.IsNullOrEmpty(_connectionName))
            {
                var testScript = $"Import-Module '{Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")}'; $connection = Get-DataverseConnection -Name '{_connectionName}'; if ($null -eq $connection) {{ throw 'Failed to load connection' }}";
                
                using var runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                using var ps = System.Management.Automation.PowerShell.Create();
                ps.Runspace = runspace;
                ps.AddScript(testScript);
                
                try
                {
                    ps.Invoke();
                    if (ps.HadErrors)
                    {
                        var errorMsg = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
                        throw new InvalidOperationException($"Failed to load named connection '{_connectionName}'.\n\n{errorMsg}\n\nTo save a connection, use:\nGet-DataverseConnection -Url <your-url> -Interactive -Name '{_connectionName}' -SetAsDefault\n\nOr list saved connections with:\nGet-DataverseConnection -List");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to validate connection '{_connectionName}': {ex.Message}\n\nTo save a connection, use:\nGet-DataverseConnection -Url <your-url> -Interactive -Name '{_connectionName}' -SetAsDefault\n\nOr list saved connections with:\nGet-DataverseConnection -List", ex);
                }
            }

            _isInitialized = true;
        }
    }

    public string GetCmdletList()
    {
        EnsureInitialized();
        
        var script = $@"
Import-Module '{Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")}'
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
            throw new InvalidOperationException("Failed to get cmdlet list: " + string.Join("\n", ps.Streams.Error));
        }

        return results.FirstOrDefault()?.ToString() ?? "[]";
    }

    public string GetCmdletHelp(string cmdletName)
    {
        EnsureInitialized();
        
        var script = $@"
Import-Module '{Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1")}'
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
            throw new InvalidOperationException($"Failed to get help for cmdlet '{cmdletName}': " + string.Join("\n", ps.Streams.Error));
        }

        return results.FirstOrDefault()?.ToString() ?? "{}";
    }

    public string StartScript(string script)
    {
        EnsureInitialized();
        
        var sessionId = Guid.NewGuid().ToString("N");
        var session = new ScriptSession(sessionId, script, _modulePath, _connectionName);
        
        if (!_sessions.TryAdd(sessionId, session))
        {
            throw new InvalidOperationException($"Session {sessionId} already exists");
        }

        session.Start();
        return sessionId;
    }

    public ScriptOutputResult GetOutput(string sessionId, bool onlyNew)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));
        }

        return session.GetOutput(onlyNew);
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

public class ScriptSession : IDisposable
{
    private readonly string _sessionId;
    private readonly string _script;
    private readonly string _modulePath;
    private readonly string? _connectionName;
    private System.Management.Automation.PowerShell? _powerShell;
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _error = new();
    private int _lastReadPosition;
    private bool _isComplete;
    private Exception? _exception;
    private readonly object _lock = new();

    public ScriptSession(string sessionId, string script, string modulePath, string? connectionName)
    {
        _sessionId = sessionId;
        _script = script;
        _modulePath = modulePath;
        _connectionName = connectionName;
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
        var iss = InitialSessionState.CreateDefault();
        
        // Disable all providers except the module provider
        iss.Providers.Clear();
        
        // Create minimal runspace
        using var runspace = RunspaceFactory.CreateRunspace(iss);
        runspace.Open();

        _powerShell = System.Management.Automation.PowerShell.Create();
        _powerShell.Runspace = runspace;

        // Import the Dataverse module
        var moduleManifest = Path.Combine(_modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
        if (File.Exists(moduleManifest))
        {
            _powerShell.AddCommand("Import-Module").AddParameter("Name", moduleManifest);
            _powerShell.Invoke();
            _powerShell.Commands.Clear();
            
            if (_powerShell.HadErrors)
            {
                lock (_lock)
                {
                    foreach (var error in _powerShell.Streams.Error)
                    {
                        _error.AppendLine($"Module import error: {error}");
                    }
                }
                _powerShell.Streams.Error.Clear();
            }
        }

        // Load the default connection if specified
        if (!string.IsNullOrEmpty(_connectionName))
        {
            _powerShell.AddScript($"$connection = Get-DataverseConnection -Name '{_connectionName}'");
            _powerShell.Invoke();
            _powerShell.Commands.Clear();
            
            if (_powerShell.HadErrors)
            {
                lock (_lock)
                {
                    foreach (var error in _powerShell.Streams.Error)
                    {
                        _error.AppendLine($"Connection load error: {error}");
                    }
                }
                _powerShell.Streams.Error.Clear();
            }
        }

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
                    SessionId = _sessionId,
                    Output = newContent,
                    IsComplete = _isComplete,
                    HasError = _exception != null || _error.Length > 0
                };
            }
            else
            {
                return new ScriptOutputResult
                {
                    SessionId = _sessionId,
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
