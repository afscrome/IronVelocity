using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.Parser
{
    public partial class VelocityParser
    {
        public IImmutableList<string> BlockDirectives { get; set; }

        private bool IsBlockDirective()
        {
            var directiveName = _input.Lt(2).Text;
            if (directiveName == "{")
                directiveName = _input.Lt(3).Text;

            return BlockDirectives.Contains(directiveName);
        }

    }
}
