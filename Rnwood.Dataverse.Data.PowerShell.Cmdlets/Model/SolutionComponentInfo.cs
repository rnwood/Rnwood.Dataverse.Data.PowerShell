using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Internal helper class for component comparison information.
    /// </summary>
    internal class SolutionComponentInfo
    {
        public string LogicalName { get; set; }
        public Guid? ObjectId { get; set; }
        public int ComponentType { get; set; }
        public int? RootComponentBehavior { get; set; }
        public bool IsSubcomponent { get; set; }
        public int? ParentComponentType { get; set; }
        public string ParentTableName { get; set; }
    }
}
