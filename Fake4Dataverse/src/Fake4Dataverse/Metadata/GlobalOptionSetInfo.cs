using System;
using System.Collections.Generic;

namespace Fake4Dataverse.Metadata
{
    internal sealed class GlobalOptionSetInfo
    {
        public string Name { get; }
        public string? DisplayName { get; set; }
        public bool IsGlobal { get; set; } = true;
        public List<OptionInfo> Options { get; } = new List<OptionInfo>();

        public GlobalOptionSetInfo(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    internal sealed class OptionInfo
    {
        public int Value { get; set; }
        public string? Label { get; set; }

        public OptionInfo(int value, string? label = null)
        {
            Value = value;
            Label = label;
        }
    }
}
