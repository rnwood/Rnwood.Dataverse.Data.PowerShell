using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell
{
    public class SolutionInfo
    {
        public Version Version { get; set; }
        public string Name { get; set; }
        public bool IsManaged { get; set; }
        public string Description { get; set; }
        public Guid Id { get; set; }
    }
}