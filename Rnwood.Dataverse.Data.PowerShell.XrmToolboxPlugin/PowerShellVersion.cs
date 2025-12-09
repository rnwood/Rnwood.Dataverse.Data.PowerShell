namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Represents the version/edition of PowerShell to use.
    /// </summary>
    public enum PowerShellVersion
    {
        /// <summary>
        /// Windows PowerShell (powershell.exe) - typically version 5.1
        /// </summary>
        Desktop,

        /// <summary>
        /// PowerShell Core (pwsh.exe) - version 7+, cross-platform
        /// </summary>
        Core
    }
}
