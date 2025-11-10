using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public partial class InvokeDataverseRequestCmdlet
    {
        /// <summary>
        /// Holds state for a single request operation in a batch or retry queue.
        /// </summary>
        internal class RequestOperationContext
        {
            private readonly Action<string> _writeVerbose;
            private readonly Action<ErrorRecord> _writeError;
            private readonly Action<object> _writeObject;
            private readonly Func<string, bool> _shouldProcess;
            private readonly DataverseEntityConverter _entityConverter;

            public RequestOperationContext(PSObject inputObject, OrganizationRequest request, int retries, int initialRetryDelay, Action<string> writeVerbose, Action<ErrorRecord> writeError, Action<object> writeObject, Func<string, bool> shouldProcess, DataverseEntityConverter entityConverter = null)
            {
                InputObject = inputObject;
                Request = request;
                Retries = retries;
                InitialRetryDelay = initialRetryDelay;
                RetriesRemaining = retries;
                NextRetryTime = DateTime.MinValue;
                _writeVerbose = writeVerbose;
                _writeError = writeError;
                _writeObject = writeObject;
                _shouldProcess = shouldProcess;
                _entityConverter = entityConverter;
            }

            public PSObject InputObject { get; }
            public OrganizationRequest Request { get; set; }
            public int Retries { get; }
            public int InitialRetryDelay { get; }
            public int RetriesRemaining { get; set; }
            public DateTime NextRetryTime { get; set; }

            public void Complete(OrganizationResponse response)
            {
                _writeVerbose($"Request completed: {Request?.GetType().Name}");
                try 
                { 
                    // Convert response to PSObject if we have a converter
                    if (_entityConverter != null)
                    {
                        var convertedResponse = _entityConverter.ConvertResponseToPSObject(response);
                        _writeObject(convertedResponse);
                    }
                    else
                    {
                        _writeObject(response);
                    }
                } 
                catch { }
            }

            public bool HandleFault(OrganizationServiceFault fault)
            {
                // No special handling for generic requests by default
                return false;
            }

            public void ScheduleRetry(Exception e)
            {
                int attemptNumber = Retries - RetriesRemaining + 1;
                int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
                NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
                RetriesRemaining--;

                _writeVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {Retries + 1}): {this}\n{e}");
            }

            public void ReportError(Exception e)
            {
                _writeError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, (object)InputObject ?? (object)Request));
            }

            public override string ToString()
            {
                return Request == null ? "Request" : Request.GetType().Name;
            }
        }
    }
}
