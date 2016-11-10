using System.Diagnostics;

namespace InfoParser.Models
{
    [DebuggerDisplay("{Namespace}\t{Name}")]
    public class Tag
    {
        public string Namespace { get; set; }

        public string Name { get; set; }
    }
}