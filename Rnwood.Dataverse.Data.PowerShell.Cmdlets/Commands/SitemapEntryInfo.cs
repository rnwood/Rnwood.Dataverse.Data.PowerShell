using System;
using System.Collections.Generic;

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
        /// Gets or sets the resource ID for localized descriptions.
        /// </summary>
        public string DescriptionResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized tooltips.
        /// </summary>
        public string ToolTipResourceId { get; set; }

        /// <summary>
        /// Gets or sets the titles of the entry keyed by LCID.
        /// </summary>
        public Dictionary<int, string> Titles { get; set; }

        /// <summary>
        /// Gets or sets the descriptions of the entry keyed by LCID.
        /// </summary>
        public Dictionary<int, string> Descriptions { get; set; }

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
        /// Gets or sets the entity name for privilege entries.
        /// </summary>
        public string PrivilegeEntity { get; set; }

        /// <summary>
        /// Gets or sets the privilege name for privilege entries.
        /// </summary>
        public string PrivilegeName { get; set; }

        /// <summary>
        /// Gets or sets the parent SubArea ID for privilege entries.
        /// </summary>
        public string ParentSubAreaId { get; set; }

        /// <summary>
        /// Gets or sets the collection of privileges for SubArea entries.
        /// </summary>
        public List<PrivilegeInfo> Privileges { get; set; }
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
        SubArea,

        /// <summary>
        /// A privilege required for a SubArea.
        /// </summary>
        Privilege
    }

    /// <summary>
    /// Represents a privilege within a sitemap SubArea.
    /// </summary>
    public class PrivilegeInfo
    {
        /// <summary>
        /// Gets or sets the entity name for the privilege.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the privilege name (e.g., "Read", "Write", "Create", "Delete").
        /// </summary>
        public string Privilege { get; set; }
    }
}
