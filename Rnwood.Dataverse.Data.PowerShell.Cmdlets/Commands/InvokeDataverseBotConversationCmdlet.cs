using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Starts an interactive conversation with a Copilot Studio bot using Direct Line API.
    /// User can type messages and see bot responses in real-time.
    /// Press Ctrl+C or type 'exit', 'quit', or 'bye' to end the conversation.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseBotConversation")]
    public class InvokeDataverseBotConversationCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot ID to start a conversation with.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID) to start conversation with.")]
        public Guid BotId { get; set; }

        /// <summary>
        /// Gets or sets the Direct Line secret or token.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Direct Line secret or token for the bot.")]
        public string DirectLineSecret { get; set; }

        /// <summary>
        /// Gets or sets the user name to display in the conversation.
        /// </summary>
        [Parameter(HelpMessage = "User name to display in the conversation. Defaults to 'User'.")]
        public string UserName { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Get bot details
                var bot = Connection.Retrieve("bot", BotId, new ColumnSet("name", "schemaname"));
                var botName = bot.GetAttributeValue<string>("name");

                Host.UI.WriteLine();
                Host.UI.WriteLine(ConsoleColor.Cyan, Host.UI.RawUI.BackgroundColor, $"╔══════════════════════════════════════════════════════════╗");
                Host.UI.WriteLine(ConsoleColor.Cyan, Host.UI.RawUI.BackgroundColor, $"║  Interactive Conversation with {botName.PadRight(25)}║");
                Host.UI.WriteLine(ConsoleColor.Cyan, Host.UI.RawUI.BackgroundColor, $"╚══════════════════════════════════════════════════════════╝");
                Host.UI.WriteLine();
                Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Connecting to bot via Direct Line...");

                // Create Start command
                using (var ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
                {
                    ps.AddCommand("Start-DataverseBotConversation")
                      .AddParameter("BotId", BotId)
                      .AddParameter("DirectLineSecret", DirectLineSecret)
                      .AddParameter("UserName", string.IsNullOrEmpty(UserName) ? "User" : UserName)
                      .AddParameter("PassThru", true)
                      .AddParameter("Connection", Connection);

                    var session = ps.Invoke();

                    if (ps.HadErrors || session == null || session.Count == 0)
                    {
                        Host.UI.WriteErrorLine("Failed to start conversation.");
                        foreach (var error in ps.Streams.Error)
                        {
                            Host.UI.WriteErrorLine(error.ToString());
                        }
                        return;
                    }

                    var sessionObj = session[0].BaseObject as PSObject;
                    Host.UI.WriteLine(ConsoleColor.Green, Host.UI.RawUI.BackgroundColor, "✓ Connected!");
                    Host.UI.WriteLine();
                    Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "Type your messages and press Enter to send.");
                    Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "Type 'exit', 'quit', or 'bye' to end the conversation.");
                    Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "Press Ctrl+C to abort.");
                    Host.UI.WriteLine();

                    // Interactive loop
                    while (true)
                    {
                        // Prompt for user input
                        Host.UI.Write(ConsoleColor.White, Host.UI.RawUI.BackgroundColor, "You: ");
                        var userMessage = Host.UI.ReadLine();

                        // Check for exit commands
                        if (string.IsNullOrWhiteSpace(userMessage))
                        {
                            continue;
                        }

                        var lowerMessage = userMessage.Trim().ToLowerInvariant();
                        if (lowerMessage == "exit" || lowerMessage == "quit" || lowerMessage == "bye")
                        {
                            Host.UI.WriteLine();
                            Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Ending conversation...");
                            break;
                        }

                        // Send message
                        ps.Commands.Clear();
                        ps.AddCommand("Send-DataverseBotMessage")
                          .AddParameter("Session", sessionObj)
                          .AddParameter("Message", userMessage);

                        var responses = ps.Invoke();

                        // Display bot responses
                        if (responses != null && responses.Count > 0)
                        {
                            foreach (var response in responses)
                            {
                                var responseObj = response.BaseObject as PSObject;
                                if (responseObj != null)
                                {
                                    var botText = responseObj.Properties["Text"]?.Value as string;
                                    if (!string.IsNullOrEmpty(botText))
                                    {
                                        Host.UI.WriteLine(ConsoleColor.Cyan, Host.UI.RawUI.BackgroundColor, $"Bot: {botText}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Bot: [No response]");
                        }

                        Host.UI.WriteLine();
                    }

                    // Stop conversation
                    ps.Commands.Clear();
                    ps.AddCommand("Stop-DataverseBotConversation")
                      .AddParameter("Session", sessionObj);
                    ps.Invoke();

                    Host.UI.WriteLine(ConsoleColor.Green, Host.UI.RawUI.BackgroundColor, "Conversation ended.");
                    Host.UI.WriteLine();
                }
            }
            catch (PipelineStoppedException)
            {
                // User pressed Ctrl+C
                Host.UI.WriteLine();
                Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Conversation interrupted.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InteractiveConversationError", ErrorCategory.InvalidOperation, BotId));
            }
        }
    }
}
