using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Ends a Direct Line bot conversation session.
    /// Note: Direct Line conversations auto-expire, so this is optional cleanup.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "DataverseBotConversation")]
    public class StopDataverseBotConversationCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the conversation session object from Start-DataverseBotConversation.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "Conversation session object from Start-DataverseBotConversation.")]
        public PSObject Session { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                var conversationId = Session.Properties["ConversationId"]?.Value as string;
                var botName = Session.Properties["BotName"]?.Value as string;
                
                if (string.IsNullOrEmpty(conversationId))
                {
                    WriteWarning("Invalid session object.");
                    return;
                }

                WriteVerbose($"Ending conversation: {conversationId} with bot: {botName}");
                
                // Direct Line conversations auto-expire after a period of inactivity
                // No explicit API call needed to end them
                // This cmdlet mainly serves as a logical endpoint for the conversation
                
                WriteVerbose("Conversation session closed. Direct Line conversation will expire automatically.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "StopConversationError", ErrorCategory.InvalidOperation, Session));
            }
        }
    }
}
