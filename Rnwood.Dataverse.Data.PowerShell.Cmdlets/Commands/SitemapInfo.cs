using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Represents information about a sitemap in Dataverse.
    /// </summary>
    public class SitemapInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sitemap.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the sitemap.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the XML definition of the sitemap.
        /// </summary>
        public string SitemapXml { get; set; }

        /// <summary>
        /// Gets or sets the display name of the solution this sitemap belongs to.
        /// </summary>
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets whether this is a managed sitemap.
        /// </summary>
        public bool IsManaged { get; set; }

        /// <summary>
        /// Gets or sets the app unique name if this sitemap is associated with an app.
        /// </summary>
        public string AppUniqueName { get; set; }

        /// <summary>
        /// Gets or sets when the sitemap was created.
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets when the sitemap was last modified.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }
    }
}
