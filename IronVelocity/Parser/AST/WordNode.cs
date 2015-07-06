using System;

namespace IronVelocity.Parser.AST
{
    public class WordNode : ExpressionNode
    {
        public string Name { get; private set; }

        public WordNode(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException("name");

            Name = name;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitWordNode(this);
        }
    }
}
