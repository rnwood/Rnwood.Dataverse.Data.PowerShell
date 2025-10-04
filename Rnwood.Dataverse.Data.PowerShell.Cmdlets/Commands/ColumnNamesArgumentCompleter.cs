using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Thin wrapper for ColumnNameArgumentCompleter to make a separate type name for plural-prefixed parameters
    /// (e.g. Columns, IgnoreProperties). The implementation simply delegates to ColumnNameArgumentCompleter.
    /// </summary>
    public class ColumnNamesArgumentCompleter : IArgumentCompleter
    {
        private readonly ColumnNameArgumentCompleter inner = new ColumnNameArgumentCompleter();

        /// <summary>
        /// Delegate completion to <see cref="ColumnNameArgumentCompleter"/> to provide column name suggestions for plural parameter forms.
        /// </summary>
        /// <param name="commandName">The name of the cmdlet being completed.</param>
        /// <param name="parameterName">The name of the parameter being completed.</param>
        /// <param name="wordToComplete">The current partial word to complete.</param>
        /// <param name="commandAst">The AST for the command being completed.</param>
        /// <param name="fakeBoundParameters">A dictionary of bound parameters provided by PowerShell.</param>
        /// <returns>A sequence of <see cref="CompletionResult"/> values from the inner completer.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            return inner.CompleteArgument(commandName, parameterName, wordToComplete, commandAst, fakeBoundParameters) ?? Enumerable.Empty<CompletionResult>();
        }
    }
}
