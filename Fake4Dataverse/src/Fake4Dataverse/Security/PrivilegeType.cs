namespace Fake4Dataverse.Security
{
    /// <summary>
    /// Defines the types of entity privileges in the Dataverse security model.
    /// </summary>
    public enum PrivilegeType
    {
        /// <summary>Permission to create records.</summary>
        Create,

        /// <summary>Permission to read records.</summary>
        Read,

        /// <summary>Permission to update records.</summary>
        Write,

        /// <summary>Permission to delete records.</summary>
        Delete,

        /// <summary>Permission to append a record to another.</summary>
        Append,

        /// <summary>Permission to have a record appended to this entity.</summary>
        AppendTo,

        /// <summary>Permission to share records.</summary>
        Share,

        /// <summary>Permission to assign records.</summary>
        Assign
    }
}
