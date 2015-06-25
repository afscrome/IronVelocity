using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Parser.AST
{
    public class Method : ReferenceNodePart
    {
        public ReferenceNodePart Target { get; set; }
        public ArgumentsNode Arguments { get; set; }
    }
}
