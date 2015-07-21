
using System;
namespace IronVelocity.Parser.AST
{
    public abstract class ReferenceNodePart : SyntaxNode
    {
        protected ReferenceNodePart(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            Name = name;
        }

        public string Name { get; set; }
    }
}
