using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Receives/polls for incoming messages from an active bot conversation via Direct Line API.
    /// Retrieves new activities (messages, typing indicators, etc.) since the last check.
    /// </summary>
    [Cmdlet(VerbsCommunications.Receive, "DataverseBotMessage")]
    [OutputType(typeof(PSObject))]
    public class ReceiveDataverseBotMessageCmdlet : PSCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the conversation session object from Start-DataverseBotConversation.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "Conversation session object from Start-DataverseBotConversation.")]
        public PSObject Session { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds to wait for new messages.
        /// If 0, returns immediately with whatever is available.
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds to wait for new messages. 0 = immediate, default is 5 seconds.")]
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to include all activity types or just messages.
        /// </summary>
        [Parameter(HelpMessage = "Include all activity types (typing, event, etc.), not just messages.")]
        public SwitchParameter IncludeAllActivities { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Extract session properties
                var conversationId = Session.Properties["ConversationId"]?.Value as string;
                var token = Session.Properties["Token"]?.Value as string;
                var watermark = Session.Properties["Watermark"]?.Value as string;
                
                if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Invalid session object. Use Start-DataverseBotConversation to create a session.");
                }

                WriteVerbose($"Polling for new messages in conversation: {conversationId}");
                WriteVerbose($"Current watermark: {watermark ?? "(none)"}");
                WriteVerbose($"Timeout: {TimeoutSeconds} seconds");

                // Poll for new activities
                var activities = PollForActivities(conversationId, token, watermark, TimeoutSeconds).GetAwaiter().GetResult();

                if (activities == null || activities.Count == 0)
                {
                    WriteVerbose("No new messages received.");
                    return;
                }

                WriteVerbose($"Received {activities.Count} new activit(ies)");

                // Update watermark in session
                var lastActivity = activities.Last();
                var watermarkProp = lastActivity.Properties["Watermark"];
                if (watermarkProp != null && !string.IsNullOrEmpty(watermarkProp.Value as string))
                {
                    Session.Properties["Watermark"].Value = watermarkProp.Value;
                    WriteVerbose($"Updated watermark to: {watermarkProp.Value}");
                }

                // Output activities
                foreach (var activity in activities)
                {
                    WriteObject(activity);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ReceiveMessageError", ErrorCategory.InvalidOperation, Session));
            }
        }

        private async Task<List<PSObject>> PollForActivities(string conversationId, string token, string watermark, int timeoutSeconds)
        {
            var activities = new List<PSObject>();
            var url = $"https://directline.botframework.com/v3/directline/conversations/{conversationId}/activities";
            
            if (!string.IsNullOrEmpty(watermark))
            {
                url += $"?watermark={watermark}";
            }

            var startTime = DateTime.UtcNow;
            var maxWaitTime = TimeSpan.FromSeconds(timeoutSeconds);
            var pollInterval = TimeSpan.FromMilliseconds(500); // Poll every 500ms

            // If timeout is 0, just do a single poll
            var singlePoll = timeoutSeconds == 0;

            do
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var response = await httpClient.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<DirectLineActivitiesResponse>(content, options);

                        if (result?.Activities != null && result.Activities.Length > 0)
                        {
                            // Filter activities based on user preferences
                            foreach (var activity in result.Activities)
                            {
                                // Skip if we only want messages and this isn't a message
                                if (!IncludeAllActivities.IsPresent && activity.Type != "message")
                                {
                                    continue;
                                }

                                // Skip our own messages (from user)
                                if (activity.From?.Role == "user")
                                {
                                    continue;
                                }

                                var psActivity = new PSObject();
                                psActivity.Properties.Add(new PSNoteProperty("Type", activity.Type));
                                psActivity.Properties.Add(new PSNoteProperty("Id", activity.Id));
                                psActivity.Properties.Add(new PSNoteProperty("Timestamp", activity.Timestamp));
                                psActivity.Properties.Add(new PSNoteProperty("From", activity.From?.Name ?? "Bot"));
                                psActivity.Properties.Add(new PSNoteProperty("FromRole", activity.From?.Role ?? "bot"));
                                psActivity.Properties.Add(new PSNoteProperty("Text", activity.Text));
                                psActivity.Properties.Add(new PSNoteProperty("Speak", activity.Speak));
                                psActivity.Properties.Add(new PSNoteProperty("InputHint", activity.InputHint));
                                psActivity.Properties.Add(new PSNoteProperty("Attachments", activity.Attachments));
                                psActivity.Properties.Add(new PSNoteProperty("Value", activity.Value));
                                psActivity.Properties.Add(new PSNoteProperty("Name", activity.Name));
                                psActivity.Properties.Add(new PSNoteProperty("Watermark", result.Watermark));

                                activities.Add(psActivity);
                            }

                            if (activities.Count > 0 || singlePoll)
                            {
                                return activities;
                            }

                            // Update watermark for next poll
                            watermark = result.Watermark;
                            url = $"https://directline.botframework.com/v3/directline/conversations/{conversationId}/activities?watermark={watermark}";
                        }
                    }
                }

                if (singlePoll)
                {
                    break;
                }

                // Wait before next poll (unless we've already exceeded timeout)
                if (DateTime.UtcNow - startTime < maxWaitTime)
                {
                    Thread.Sleep(pollInterval);
                }
            } while (DateTime.UtcNow - startTime < maxWaitTime);

            return activities;
        }

        private class DirectLineActivitiesResponse
        {
            public DirectLineActivity[] Activities { get; set; }
            public string Watermark { get; set; }
        }

        private class DirectLineActivity
        {
            public string Type { get; set; }
            public string Id { get; set; }
            public DateTime Timestamp { get; set; }
            public DirectLineChannelAccount From { get; set; }
            public string Text { get; set; }
            public string Speak { get; set; }
            public string InputHint { get; set; }
            public object[] Attachments { get; set; }
            public object Value { get; set; }
            public string Name { get; set; }
        }

        private class DirectLineChannelAccount
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}
