using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sends a message to an active bot conversation via Direct Line API and retrieves the bot's response.
    /// </summary>
    [Cmdlet(VerbsCommunications.Send, "DataverseBotMessage")]
    [OutputType(typeof(PSObject))]
    public class SendDataverseBotMessageCmdlet : PSCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the conversation session object from Start-DataverseBotConversation.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "Conversation session object from Start-DataverseBotConversation.")]
        public PSObject Session { get; set; }

        /// <summary>
        /// Gets or sets the message text to send.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Message text to send to the bot.")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds to wait for bot response.
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds to wait for bot response. Default is 30 seconds.")]
        public int TimeoutSeconds { get; set; } = 30;

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
                var userId = Session.Properties["UserId"]?.Value as string;
                var userName = Session.Properties["UserName"]?.Value as string;
                var watermark = Session.Properties["Watermark"]?.Value as string;
                
                if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Invalid session object. Use Start-DataverseBotConversation to create a session.");
                }

                WriteVerbose($"Sending message to conversation: {conversationId}");
                WriteVerbose($"Message: {Message}");

                // Send message via Direct Line API
                var sendResult = SendMessageToDirectLine(conversationId, token, userId, userName, Message).GetAwaiter().GetResult();
                
                if (!sendResult)
                {
                    WriteWarning("Failed to send message to Direct Line API");
                    return;
                }

                WriteVerbose("Message sent successfully. Waiting for bot response...");

                // Poll for bot response
                var responses = PollForBotResponse(conversationId, token, watermark, TimeoutSeconds).GetAwaiter().GetResult();

                if (responses == null || responses.Count == 0)
                {
                    WriteWarning("No response received from bot within timeout period.");
                    return;
                }

                // Update watermark in session
                if (responses.Count > 0)
                {
                    var lastResponse = responses.Last();
                    var watermarkProp = lastResponse.Properties["Watermark"];
                    if (watermarkProp != null && !string.IsNullOrEmpty(watermarkProp.Value as string))
                    {
                        Session.Properties["Watermark"].Value = watermarkProp.Value;
                    }
                }

                // Output bot responses
                foreach (var response in responses)
                {
                    WriteObject(response);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SendMessageError", ErrorCategory.InvalidOperation, Session));
            }
        }

        private async Task<bool> SendMessageToDirectLine(string conversationId, string token, string userId, string userName, string messageText)
        {
            var url = $"https://directline.botframework.com/v3/directline/conversations/{conversationId}/activities";
            
            var activity = new
            {
                type = "message",
                from = new
                {
                    id = userId,
                    name = userName
                },
                text = messageText
            };

            var json = JsonSerializer.Serialize(activity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = content;
                
                var response = await httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
        }

        private async Task<List<PSObject>> PollForBotResponse(string conversationId, string token, string watermark, int timeoutSeconds)
        {
            var responses = new List<PSObject>();
            var url = $"https://directline.botframework.com/v3/directline/conversations/{conversationId}/activities";
            
            if (!string.IsNullOrEmpty(watermark))
            {
                url += $"?watermark={watermark}";
            }

            var startTime = DateTime.UtcNow;
            var maxWaitTime = TimeSpan.FromSeconds(timeoutSeconds);
            var pollInterval = TimeSpan.FromMilliseconds(500); // Poll every 500ms

            while (DateTime.UtcNow - startTime < maxWaitTime)
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
                            // Filter for bot messages (not our own echo)
                            foreach (var activity in result.Activities.Where(a => a.From?.Role == "bot" && a.Type == "message"))
                            {
                                var psActivity = new PSObject();
                                psActivity.Properties.Add(new PSNoteProperty("Type", activity.Type));
                                psActivity.Properties.Add(new PSNoteProperty("Id", activity.Id));
                                psActivity.Properties.Add(new PSNoteProperty("Timestamp", activity.Timestamp));
                                psActivity.Properties.Add(new PSNoteProperty("From", activity.From?.Name ?? "Bot"));
                                psActivity.Properties.Add(new PSNoteProperty("Text", activity.Text));
                                psActivity.Properties.Add(new PSNoteProperty("Speak", activity.Speak));
                                psActivity.Properties.Add(new PSNoteProperty("InputHint", activity.InputHint));
                                psActivity.Properties.Add(new PSNoteProperty("Attachments", activity.Attachments));
                                psActivity.Properties.Add(new PSNoteProperty("Watermark", result.Watermark));

                                responses.Add(psActivity);
                            }

                            if (responses.Count > 0)
                            {
                                return responses;
                            }

                            // Update watermark for next poll
                            watermark = result.Watermark;
                            url = $"https://directline.botframework.com/v3/directline/conversations/{conversationId}/activities?watermark={watermark}";
                        }
                    }
                }

                // Wait before next poll
                Thread.Sleep(pollInterval);
            }

            return responses;
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
        }

        private class DirectLineChannelAccount
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}
