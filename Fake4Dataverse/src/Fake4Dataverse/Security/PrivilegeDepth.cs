namespace Fake4Dataverse.Security
{
    /// <summary>
    /// Defines the depth at which a security privilege applies.
    /// </summary>
    public enum PrivilegeDepth
    {
        /// <summary>No access.</summary>
        None = 0,

        /// <summary>Access to user-owned records only.</summary>
        User = 1,

        /// <summary>Access to records owned by users in the same business unit.</summary>
        BusinessUnit = 2,

        /// <summary>Access to records owned by users in the same business unit and child business units.</summary>
        ParentChildBusinessUnit = 3,

        /// <summary>Access to all records in the organization.</summary>
        Organization = 4
    }
}
