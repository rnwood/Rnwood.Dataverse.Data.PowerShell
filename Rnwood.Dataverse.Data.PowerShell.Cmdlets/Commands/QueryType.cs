using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Enumeration of view query types in Dataverse.
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// Other view type (0)
        /// </summary>
        OtherView = 0,

        /// <summary>
        /// Public view (1)
        /// </summary>
        PublicView = 1,

        /// <summary>
        /// Advanced Find view (2)
        /// </summary>
        AdvancedFind = 2,

        /// <summary>
        /// Sub-grid view (4)
        /// </summary>
        SubGrid = 4,

        /// <summary>
        /// Dashboard view (8)
        /// </summary>
        Dashboard = 8,

        /// <summary>
        /// Mobile client view (16)
        /// </summary>
        MobileClientView = 16,

        /// <summary>
        /// Lookup view (64)
        /// </summary>
        LookupView = 64,

        /// <summary>
        /// Main application view (128)
        /// </summary>
        MainApplicationView = 128,

        /// <summary>
        /// Quick Find search view (256)
        /// </summary>
        QuickFindSearch = 256,

        /// <summary>
        /// Associated view (512)
        /// </summary>
        Associated = 512,

        /// <summary>
        /// Calendar view (1024)
        /// </summary>
        CalendarView = 1024,

        /// <summary>
        /// Interactive experience view (2048)
        /// </summary>
        InteractiveExperience = 2048
    }
}