#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Provides methods to invoke cmdlets.
    /// Note: PowerShell hosting in .NET requires the full PowerShell runtime,
    /// which Microsoft.PowerShell.SDK NuGet package does not provide (only reference assemblies).
    /// </summary>
    /// <remarks>
    /// Due to limitations of PowerShell SDK packaging for .NET:
    /// - In-process PowerShell hosting returns null for PowerShell.Create()
    /// - Subprocess invocation cannot share in-memory mock data
    /// 
    /// For testing cmdlets with mocks, use:
    /// - Direct unit tests for internal classes (tested via TestBase/FakeXrmEasy)
    /// - Pester tests for full cmdlet integration (existing tests/ directory)
    /// </remarks>
    public class CmdletInvoker : IDisposable
    {
        private readonly ServiceClient _connection;
        private readonly string _moduleRoot;
        private bool _disposed;

        /// <summary>
        /// Initializes a new CmdletInvoker with the specified mock connection.
        /// </summary>
        public CmdletInvoker(ServiceClient connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            
            // Get the module root path
            var cmdletAssembly = typeof(GetDataverseRecordCmdlet).Assembly;
            var cmdletsDir = Path.GetDirectoryName(cmdletAssembly.Location);
            _moduleRoot = Path.GetDirectoryName(cmdletsDir) ?? throw new InvalidOperationException("Cannot determine module root");
        }

        /// <summary>
        /// Attempts to invoke a cmdlet. Currently not supported due to PowerShell SDK limitations.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown - use Pester tests or direct unit tests instead.</exception>
        public CmdletResult<T> Invoke<T>(string cmdletName, IDictionary<string, object?> parameters)
        {
            // NOTE: PowerShell hosting doesn't work because Microsoft.PowerShell.SDK only contains
            // reference assemblies, not runtime assemblies. PowerShell.Create() returns null.
            // Subprocess invocation cannot share in-memory FakeXrmEasy mock data with parent process.
            throw new NotSupportedException(
                "PowerShell cmdlet invocation is not supported in xUnit tests due to SDK limitations. " +
                "Use direct unit tests for internal classes (FakeXrmEasy works), " +
                "or Pester tests (tests/ directory) for cmdlet integration testing.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Result from a cmdlet invocation.
    /// </summary>
    public class CmdletResult<T>
    {
        public CmdletResult()
        {
        }

        public CmdletResult(
            IReadOnlyList<T> output,
            IReadOnlyList<ErrorRecord> errors,
            IReadOnlyList<string> warnings,
            IReadOnlyList<string> verbose,
            bool hadErrors)
        {
            Output = output.ToList();
            Errors = errors.ToList();
            Warnings = warnings.ToList();
            Verbose = verbose.ToList();
            HadErrors = hadErrors;
        }

        public List<T> Output { get; set; } = new();
        public List<ErrorRecord> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Verbose { get; set; } = new();
        public bool HadErrors { get; set; }

        public T? FirstOrDefault() => Output.Count > 0 ? Output[0] : default;
        public T Single() => Output.Count == 1 
            ? Output[0] 
            : throw new InvalidOperationException($"Expected 1 output, got {Output.Count}");
    }
}
#endif
