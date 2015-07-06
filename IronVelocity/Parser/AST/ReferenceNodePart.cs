
using System;
namespace IronVelocity.Parser.AST
{
    public abstract class ReferenceNodePart : SyntaxNode
    {
        [Obsolete]
        protected ReferenceNodePart()
        {
        }

        protected ReferenceNodePart(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException("name");

            Name = name;
        }

        public string Name { get; set; }
    }
}
