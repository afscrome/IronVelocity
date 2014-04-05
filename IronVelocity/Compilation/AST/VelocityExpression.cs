using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public abstract class VelocityExpression : Expression
    {
        public SymbolInformation Symbols { get; private set; }

        protected VelocityExpression() { }

        protected VelocityExpression(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");


        }

        public override Expression Reduce()
        {
            return ReduceInternal();
        }

        public override bool CanReduce { get { return true; } }
        public override ExpressionType NodeType { get { return ExpressionType.Extension; } }
        public override Type Type { get { return typeof(object); } }

        protected abstract Expression ReduceInternal();
    }

    public class SymbolInformation
    {
        public SymbolInformation(INode node)
        {
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
    }
}
