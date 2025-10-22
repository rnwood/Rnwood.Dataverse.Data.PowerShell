using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Interface defining parameters for delete operations that can be shared between cmdlet and operation context.
    /// </summary>
    internal interface IDeleteOperationParameters
    {
        /// <summary>
        /// Gets the business logic types to bypass.
        /// </summary>
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }

        /// <summary>
        /// Gets the business logic execution step IDs to bypass.
        /// </summary>
        Guid[] BypassBusinessLogicExecutionStepIds { get; }

        /// <summary>
        /// Gets a value indicating whether to suppress errors if the record doesn't exist.
        /// </summary>
        bool IfExists { get; }

        /// <summary>
        /// Gets the number of retries for failed operations.
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Gets the initial retry delay in seconds.
        /// </summary>
        int InitialRetryDelay { get; }

        /// <summary>
        /// Gets the match-on column lists for finding existing records.
        /// </summary>
        string[][] MatchOn { get; }

        /// <summary>
        /// Gets a value indicating whether to allow processing multiple matching records.
        /// </summary>
        bool AllowMultipleMatches { get; }
    }
}
