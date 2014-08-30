using NVelocity.Runtime.Parser.Node;
using System;
using System.Linq.Expressions;

namespace IronVelocity.Compilation.AST
{
    public abstract partial class VelocityExpression : Expression
    {
        public SymbolInformation Symbols { get; protected set; }
        public override bool CanReduce { get { return true; } }
        public override ExpressionType NodeType { get { return ExpressionType.Extension; } }
        public override Type Type { get { return typeof(object); } }

        protected VelocityExpression() { }

        [Obsolete("remove", true)]
        protected VelocityExpression(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            Symbols = new SymbolInformation(node);
        }

        public abstract override Expression Reduce();
    }

    public class SymbolInformation
    {
        public SymbolInformation(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            StartLine = node.FirstToken.BeginLine +1;
            StartColumn = node.FirstToken.BeginColumn;
            EndLine = node.LastToken.EndLine + 1;
            EndColumn = node.LastToken.EndColumn;
        }

        public int StartLine { get; private set; }
        public int StartColumn { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }

        public override bool Equals(object obj)
        {
            var symbol = obj as SymbolInformation;
            return symbol == null
                ? false
                : symbol == this;
        }

        public static bool operator ==(SymbolInformation left, SymbolInformation right)
        {
            if (Object.Equals(left, null) || Object.ReferenceEquals(right, null))
                return Object.ReferenceEquals(left,right);
            else
                return left.StartLine == right.StartLine
                && left.StartColumn == right.StartColumn
                && left.EndLine == right.EndLine
                && left.EndColumn == right.EndColumn;

        }

        public static bool operator !=(SymbolInformation left, SymbolInformation right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash *= 23 + StartLine.GetHashCode();
                hash *= 23 + StartColumn.GetHashCode();
                hash *= 23 + EndLine.GetHashCode();
                hash *= 23 + EndColumn.GetHashCode();
                return hash;
            }
        }

    }
}
