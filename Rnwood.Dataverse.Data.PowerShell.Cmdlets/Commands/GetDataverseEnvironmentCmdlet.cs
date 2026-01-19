using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using OrganizationDetail = Microsoft.Xrm.Sdk.Discovery.OrganizationDetail;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Lists all Dataverse environments accessible to the authenticated user.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEnvironment")]
    [OutputType(typeof(OrganizationDetail))]
    public class GetDataverseEnvironmentCmdlet : PSCmdlet
    {
        private const string PARAMSET_ACCESSTOKEN = "With access token";
        private const string PARAMSET_CONNECTION = "With connection";

        /// <summary>
        /// Gets or sets the script block to provide access tokens.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_ACCESSTOKEN, HelpMessage = "Script block that returns an access token string. Called with the resource URL as a parameter.")]
        public ScriptBlock AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the connection to use for discovering environments.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = PARAMSET_CONNECTION, HelpMessage = "Connection to use for discovering environments. If not specified, the default connection is used.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Gets or sets the friendly name filter. Supports wildcards.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter environments by friendly name. Supports wildcards (* and ?).")]
        [SupportsWildcards]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the geo/region filter.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter environments by geographic region (e.g., NA, EMEA, APAC).")]
        public string Geo { get; set; }

        /// <summary>
        /// Gets or sets the organization type filter.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter environments by organization type.")]
        public Microsoft.Xrm.Sdk.Organization.OrganizationType? OrganizationType { get; set; }

        /// <summary>
        /// Gets or sets the timeout for the discovery operation in seconds.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Timeout for the discovery operation in seconds. Defaults to 5 minutes.")]
        public uint Timeout { get; set; } = 5 * 60;

        // Cancellation token source that is cancelled when the user hits Ctrl+C (StopProcessing)
        private CancellationTokenSource _userCancellationCts;

        /// <summary>
        /// Initializes the cmdlet processing.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            // initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Called when the user cancels the cmdlet.
        /// </summary>
        protected override void StopProcessing()
        {
            // Called when user presses Ctrl+C. Signal cancellation to any ongoing operations.
            try
            {
                _userCancellationCts?.Cancel();
            }
            catch { }
            base.StopProcessing();
        }

        /// <summary>
        /// Completes cmdlet processing.
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();
            _userCancellationCts?.Dispose();
            _userCancellationCts = null;
        }

        private CancellationTokenSource CreateLinkedCts(TimeSpan timeout)
        {
            var timeoutCts = new CancellationTokenSource(timeout);
            return CancellationTokenSource.CreateLinkedTokenSource(_userCancellationCts?.Token ?? CancellationToken.None, timeoutCts.Token);
        }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                Func<string, Task<string>> tokenProvider;

                if (ParameterSetName == PARAMSET_ACCESSTOKEN)
                {
                    // Use the provided AccessToken script block
                    tokenProvider = GetTokenWithScriptBlock;
                }
                else
                {
                    // Get connection (use default if not specified)
                    var connection = Connection ?? DefaultConnectionManager.DefaultConnection;
                    if (connection == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException("No connection specified and no default connection is set. Use Get-DataverseConnection with -SetAsDefault or provide a -Connection parameter."),
                            "NoConnection",
                            ErrorCategory.InvalidOperation,
                            null));
                        return;
                    }

                    // Extract token provider from connection if it's a ServiceClientWithTokenProvider
                    if (connection is ServiceClientWithTokenProvider serviceClientWithToken)
                    {
                        tokenProvider = serviceClientWithToken.TokenProviderFunction;
                    }
                    else
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException("The provided connection does not support environment discovery. Please use Get-DataverseEnvironment with -AccessToken parameter instead, or create a connection using one of the authentication methods that supports token providers (Interactive, DeviceCode, ClientSecret, ClientCertificate, etc.)."),
                            "UnsupportedConnection",
                            ErrorCategory.InvalidOperation,
                            connection));
                        return;
                    }
                }

                // Discover environments
                using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
                {
                    var orgDetails = ServiceClient.DiscoverOnlineOrganizationsAsync(
                        tokenProvider,
                        new Uri("https://globaldisco.crm.dynamics.com"),
                        null,
                        null,
                        cts.Token).GetAwaiter().GetResult();

                    if (orgDetails == null || orgDetails.Count == 0)
                    {
                        WriteVerbose("No Dataverse environments found for this user.");
                        return;
                    }

                    // Apply filters
                    IEnumerable<OrganizationDetail> filteredOrgs = orgDetails;

                    // Filter by FriendlyName with wildcard support
                    if (!string.IsNullOrEmpty(FriendlyName))
                    {
                        WildcardPattern pattern = new WildcardPattern(FriendlyName, WildcardOptions.IgnoreCase);
                        filteredOrgs = filteredOrgs.Where(org => pattern.IsMatch(org.FriendlyName));
                    }

                    // Filter by Geo
                    if (!string.IsNullOrEmpty(Geo))
                    {
                        filteredOrgs = filteredOrgs.Where(org => 
                            string.Equals(org.Geo, Geo, StringComparison.OrdinalIgnoreCase));
                    }

                    // Filter by OrganizationType
                    if (OrganizationType.HasValue)
                    {
                        filteredOrgs = filteredOrgs.Where(org => org.OrganizationType == OrganizationType.Value);
                    }

                    // Write each environment to the pipeline
                    foreach (var org in filteredOrgs)
                    {
                        WriteObject(org);
                    }
                }
            }
            catch (Exception e)
            {
                // If cancellation was requested, throw a terminating PipelineStoppedException so PowerShell treats this as an interrupted operation
                if (e is OperationCanceledException || e is TaskCanceledException || (_userCancellationCts != null && _userCancellationCts.IsCancellationRequested))
                {
                    ThrowTerminatingError(new ErrorRecord(new PipelineStoppedException(), "OperationStopped", ErrorCategory.OperationStopped, null));
                }

                // If it's already a PipelineStoppedException (from ThrowTerminatingError), rethrow it
                if (e is PipelineStoppedException)
                {
                    throw;
                }

                WriteError(new ErrorRecord(e, "dataverse-failed-discovery", ErrorCategory.ConnectionError, null) { ErrorDetails = new ErrorDetails($"Failed to discover Dataverse environments: {e.Message}") });
            }
        }

        private async Task<string> GetTokenWithScriptBlock(string url)
        {
            using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
            {
                var results = await Task.Run(() => AccessToken.Invoke(url), cts.Token);
                if (results.Count == 0)
                {
                    throw new InvalidOperationException("AccessToken script block did not return a value.");
                }
                return results[0].BaseObject.ToString();
            }
        }
    }
}
