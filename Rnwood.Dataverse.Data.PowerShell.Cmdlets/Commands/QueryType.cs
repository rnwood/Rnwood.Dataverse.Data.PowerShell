using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Enumeration of view query types in Dataverse.
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// Main application view. Value = 0.
        /// </summary>
        MainApplicationView = 0,

        /// <summary>
        /// An advanced search. Value = 1.
        /// </summary>
        AdvancedSearch = 1,

        /// <summary>
        /// A subgrid query. Value = 2.
        /// </summary>
        SubGrid = 2,

        /// <summary>
        /// A quick find query, which defines the columns searched using the Search field in a list view. Value = 4.
        /// </summary>
        QuickFindSearch = 4,

        /// <summary>
        /// A reporting query. Value = 8.
        /// </summary>
        Reporting = 8,

        /// <summary>
        /// An offline filter for Dynamics 365 for Outlook. Value = 16.
        /// </summary>
        OfflineFilters = 16,

        /// <summary>
        /// A lookup view. Value = 64.
        /// </summary>
        LookupView = 64,

        /// <summary>
        /// Specifies the service management appointment book view. Value = 128.
        /// </summary>
        SMAppointmentBookView = 128,

        /// <summary>
        /// Specifies the main application view without a subject. Value = 1024.
        /// </summary>
        MainApplicationViewWithoutSubject = 1024,

        /// <summary>
        /// A saved query used for workflow templates and email templates. Value = 2048.
        /// </summary>
        SavedQueryTypeOther = 2048,

        /// <summary>
        /// A view for a dialog (workflow process). Value = 4096.
        /// </summary>
        InteractiveWorkflowView = 4096,

        /// <summary>
        /// An offline template for Dynamics 365 for Outlook. Value = 8192.
        /// </summary>
        OfflineTemplate = 8192,

        /// <summary>
        /// A custom view. Value = 16384.
        /// </summary>
        CustomDefinedView = 16384,

        /// <summary>
        /// Specifies a view on the Product, DynamicProperty, and DynamicPropertyOptionSetItem entities that can be used to filter out the entities for which labels will be exported using the ExportFieldTranslationRequest message. Value = 65536.
        /// </summary>
        ExportFieldTranslationsView = 65536,

        /// <summary>
        /// A template for Dynamics 365 for Outlook. Value = 131072.
        /// </summary>
        OutlookTemplate = 131072,

        /// <summary>
        /// An address book filter. Value = 512.
        /// </summary>
        AddressBookFilters = 512,

        /// <summary>
        /// A filter for Dynamics 365 for Outlook. Value = 256.
        /// </summary>
        OutlookFilters = 256,

        /// <summary>
        /// A view for Copilot.
        /// </summary>
        CopilotView = 32768
    }
}