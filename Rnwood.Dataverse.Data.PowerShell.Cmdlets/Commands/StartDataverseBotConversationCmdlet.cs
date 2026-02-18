using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Starts a conversation session with a Copilot Studio bot using Direct Line API.
    /// Returns a session object that can be used with Send-DataverseBotMessage.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "DataverseBotConversation")]
    [OutputType(typeof(PSObject))]
    public class StartDataverseBotConversationCmdlet : OrganizationServiceCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the bot ID to start a conversation with.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID) to start conversation with.")]
        public Guid BotId { get; set; }

        /// <summary>
        /// Gets or sets the Direct Line secret or token.
        /// If not provided, attempts to retrieve from bot configuration.
        /// </summary>
        [Parameter(HelpMessage = "Direct Line secret or token. If not provided, attempts to retrieve from bot.")]
        public string DirectLineSecret { get; set; }

        /// <summary>
        /// Gets or sets the user ID to use for the conversation.
        /// </summary>
        [Parameter(HelpMessage = "User ID to use for the conversation. Defaults to generated GUID.")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user name to display in the conversation.
        /// </summary>
        [Parameter(HelpMessage = "User name to display in the conversation. Defaults to 'User'.")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets whether to return the session object immediately.
        /// </summary>
        [Parameter(HelpMessage = "Return the session object for pipeline use.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Get bot details
                var bot = Connection.Retrieve("bot", BotId, new ColumnSet("name", "schemaname", "botid"));
                var botName = bot.GetAttributeValue<string>("name");
                var botSchema = bot.GetAttributeValue<string>("schemaname");

                WriteVerbose($"Starting Direct Line conversation with bot: {botName} ({botSchema})");

                // If no Direct Line secret provided, try to get it from bot configuration
                // Note: This may need to be manually configured or retrieved from Azure
                if (string.IsNullOrEmpty(DirectLineSecret))
                {
                    WriteWarning("No Direct Line secret provided. You must provide a Direct Line secret via -DirectLineSecret parameter.");
                    WriteWarning("To get a Direct Line secret:");
                    WriteWarning("1. Go to Azure Portal");
                    WriteWarning("2. Find your bot resource");
                    WriteWarning("3. Go to Channels > Direct Line");
                    WriteWarning("4. Copy one of the secret keys");
                    throw new InvalidOperationException("Direct Line secret is required.");
                }

                // Start conversation using Direct Line API
                var conversationResponse = StartDirectLineConversation(DirectLineSecret).GetAwaiter().GetResult();

                if (conversationResponse == null || string.IsNullOrEmpty(conversationResponse.ConversationId))
                {
                    throw new InvalidOperationException("Failed to start Direct Line conversation. Check your Direct Line secret.");
                }

                WriteVerbose($"Direct Line conversation started: {conversationResponse.ConversationId}");

                // Create session object
                var session = new PSObject();
                session.Properties.Add(new PSNoteProperty("BotId", BotId));
                session.Properties.Add(new PSNoteProperty("BotName", botName));
                session.Properties.Add(new PSNoteProperty("BotSchema", botSchema));
                session.Properties.Add(new PSNoteProperty("ConversationId", conversationResponse.ConversationId));
                session.Properties.Add(new PSNoteProperty("Token", conversationResponse.Token));
                session.Properties.Add(new PSNoteProperty("StreamUrl", conversationResponse.StreamUrl));
                session.Properties.Add(new PSNoteProperty("UserId", string.IsNullOrEmpty(UserId) ? $"user-{Guid.NewGuid():N}" : UserId));
                session.Properties.Add(new PSNoteProperty("UserName", string.IsNullOrEmpty(UserName) ? "User" : UserName));
                session.Properties.Add(new PSNoteProperty("StartTime", DateTime.UtcNow));
                session.Properties.Add(new PSNoteProperty("Watermark", (string)null));

                WriteVerbose($"Session created successfully");
                
                if (PassThru.IsPresent)
                {
                    WriteObject(session);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "StartConversationError", ErrorCategory.InvalidOperation, BotId));
            }
        }

        private async Task<DirectLineConversationResponse> StartDirectLineConversation(string secret)
        {
            var url = "https://directline.botframework.com/v3/directline/conversations";
            
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secret);
                
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<DirectLineConversationResponse>(content, options);
            }
        }

        private class DirectLineConversationResponse
        {
            public string ConversationId { get; set; }
            public string Token { get; set; }
            public int ExpiresIn { get; set; }
            public string StreamUrl { get; set; }
        }
    }
}
