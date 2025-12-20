namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Specifies the package type for solution packing and unpacking.
    /// </summary>
    public enum SolutionPackageType
    {
        /// <summary>
        /// Unmanaged solution package.
        /// </summary>
        Unmanaged,

        /// <summary>
        /// Managed solution package.
        /// </summary>
        Managed,

        /// <summary>
        /// Both managed and unmanaged solution packages.
        /// </summary>
        Both
    }

    /// <summary>
    /// Specifies the package type for solution import (packing).
    /// </summary>
    public enum ImportSolutionPackageType
    {
        /// <summary>
        /// Unmanaged solution package.
        /// </summary>
        Unmanaged,

        /// <summary>
        /// Managed solution package.
        /// </summary>
        Managed
    }
}
