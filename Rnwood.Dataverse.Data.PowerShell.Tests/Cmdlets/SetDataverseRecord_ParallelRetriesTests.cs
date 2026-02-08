using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Threading;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Parallel Retries tests for Set-DataverseRecord cmdlet.
    /// Uses interceptors to simulate transient failures for retry testing.
    /// </summary>
    public class SetDataverseRecord_ParallelRetriesTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        [Fact(Skip = "Retry testing with interceptors requires changes to interceptor architecture to inject failures after initial setup")]
        public void SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithBatching()
        {
            // This test validates retry logic in parallel mode with batching enabled
            // Each parallel worker maintains its own batch processor with retry capability
            // The current interceptor architecture doesn't support injecting failures
            // after metadata requests complete, making it difficult to test retry logic
            // in isolation. This is better tested in E2E tests where real transient
            // failures can occur naturally.
            //
            // To properly test this, we would need:
            // - An interceptor that counts CreateRequest/UpdateRequest calls specifically
            // - Fails on first N ExecuteMultiple requests but allows metadata requests
            // - Which requires distinguishing request types at the interceptor level
            //
            // For now, the retry logic is validated via:
            // - E2E tests with real Dataverse (can experience real transient failures)
            // - Code review of SetBatchProcessor retry implementation
        }

        [Fact(Skip = "Retry testing with interceptors requires changes to interceptor architecture to inject failures after initial setup")]
        public void SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithoutBatching()
        {
            // This test validates retry logic in parallel mode with BatchSize=1 (non-batched)
            // Tests individual request retries across parallel workers
            // Same limitations as above - interceptor needs to distinguish request types
        }

        [Fact(Skip = "Retry testing with interceptors requires changes to interceptor architecture to inject failures after initial setup")]
        public void SetDataverseRecord_ParallelRetries_ExhaustsRetriesAndReportsError()
        {
            // This test validates error reporting when retries are exhausted in parallel mode
            // Ensures errors from all parallel workers are properly aggregated and reported
            // Same limitations as above - interceptor needs to distinguish request types
        }

        // Note: The main parallel functionality tests (without retry simulation) 
        // are in SetDataverseRecord_ParallelExecutionTests.cs and work with FakeXrmEasy
    }
}
