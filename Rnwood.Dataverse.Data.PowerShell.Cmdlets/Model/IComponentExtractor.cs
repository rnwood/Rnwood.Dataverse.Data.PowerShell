using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Interface for extracting solution components from different sources.
    /// </summary>
    public interface IComponentExtractor
    {
        /// <summary>
        /// Gets a value indicating whether the solution is managed. Returns null for file-based sources.
        /// </summary>
        bool? IsManagedSolution { get; }

        /// <summary>
        /// Gets the components from the source.
        /// </summary>
        /// <param name="includeSubcomponents">Whether to include subcomponents.</param>
        /// <returns>A list of solution components.</returns>
        List<SolutionComponent> GetComponents(bool includeSubcomponents);

        /// <summary>
        /// Gets the subcomponents for a specific parent component.
        /// </summary>
        /// <param name="parentComponent">The parent component.</param>
        /// <returns>A list of subcomponents.</returns>
        List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent);
    }
}
