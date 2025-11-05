using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.McpServer.Tools;

public class PowerShellExecutor : IDisposable
{
    private readonly ConcurrentDictionary<string, ScriptSession> _sessions = new();
    private readonly string _modulePath;

    public PowerShellExecutor()
    {
        // Find the module path - it should be in the output directory
        var assemblyDir = Path.GetDirectoryName(typeof(PowerShellExecutor).Assembly.Location)!;
        _modulePath = Path.Combine(assemblyDir, "..", "..", "..", "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");
        _modulePath = Path.GetFullPath(_modulePath);
    }

    public string StartScript(string script)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var session = new ScriptSession(sessionId, script, _modulePath);
        
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
    private System.Management.Automation.PowerShell? _powerShell;
    private readonly List<PSDataCollection<PSObject>> _outputCollections = new();
    private readonly StringBuilder _output = new();
    private readonly StringBuilder _error = new();
    private int _lastReadPosition;
    private bool _isComplete;
    private Exception? _exception;
    private readonly object _lock = new();

    public ScriptSession(string sessionId, string script, string modulePath)
    {
        _sessionId = sessionId;
        _script = script;
        _modulePath = modulePath;
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

        // Execute the script
        _powerShell.AddScript(_script);
        
        var outputCollection = new PSDataCollection<PSObject>();
        outputCollection.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<PSObject> collection)
            {
                lock (_lock)
                {
                    foreach (var item in collection)
                    {
                        _output.AppendLine(item?.ToString() ?? "");
                    }
                }
            }
        };

        _powerShell.Streams.Error.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<ErrorRecord> collection)
            {
                lock (_lock)
                {
                    foreach (var error in collection)
                    {
                        _error.AppendLine($"ERROR: {error}");
                    }
                }
            }
        };

        _powerShell.Streams.Warning.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<WarningRecord> collection)
            {
                lock (_lock)
                {
                    foreach (var warning in collection)
                    {
                        _output.AppendLine($"WARNING: {warning}");
                    }
                }
            }
        };

        _powerShell.Streams.Verbose.DataAdded += (sender, e) =>
        {
            if (sender is PSDataCollection<VerboseRecord> collection)
            {
                lock (_lock)
                {
                    foreach (var verbose in collection)
                    {
                        _output.AppendLine($"VERBOSE: {verbose}");
                    }
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
