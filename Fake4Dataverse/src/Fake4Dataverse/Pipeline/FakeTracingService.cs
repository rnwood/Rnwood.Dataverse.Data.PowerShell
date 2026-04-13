using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// In-memory implementation of <see cref="ITracingService"/> that captures all trace messages
    /// written by plugins during test execution so they can be inspected in assertions.
    /// </summary>
    public sealed class FakeTracingService : ITracingService
    {
        private readonly List<string> _traces = new List<string>();

        /// <summary>Gets all trace messages written since the last <see cref="Clear"/> call.</summary>
        public IReadOnlyList<string> Traces => _traces.AsReadOnly();

        /// <summary>Records a formatted trace message.</summary>
        public void Trace(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            _traces.Add(args.Length > 0 ? string.Format(format, args) : format);
        }

        /// <summary>Removes all captured trace messages.</summary>
        public void Clear() => _traces.Clear();
    }
}
