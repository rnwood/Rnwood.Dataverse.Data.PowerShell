using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Represents a sitemap entry (Area, Group, or SubArea).
    /// </summary>
    public class SitemapEntryInfo
    {
        /// <summary>
        /// Gets or sets the type of sitemap entry.
        /// </summary>
        public SitemapEntryType EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized titles.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the title/label of the entry.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the entry.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon path (for Areas and SubAreas).
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the entity logical name (for SubAreas).
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the URL (for SubAreas).
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets whether the entry is a default entry.
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID (for Groups and SubAreas).
        /// </summary>
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID (for SubAreas).
        /// </summary>
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets whether to show in the app navigation.
        /// </summary>
        public bool? ShowInAppNavigation { get; set; }

        /// <summary>
        /// Gets or sets the privilege required to view this entry.
        /// </summary>
        public string Privilege { get; set; }
    }

    /// <summary>
    /// Type of sitemap entry.
    /// </summary>
    public enum SitemapEntryType
    {
        /// <summary>
        /// A top-level navigation area.
        /// </summary>
        Area,

        /// <summary>
        /// A group within an area.
        /// </summary>
        Group,

        /// <summary>
        /// A navigation item within a group.
        /// </summary>
        SubArea
    }
}
