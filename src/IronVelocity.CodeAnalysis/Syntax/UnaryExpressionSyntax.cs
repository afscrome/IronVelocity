using System.Collections.Generic;
using System.Collections.Immutable;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Operand { get; }

        public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }
        public override ImmutableArray<SyntaxNode> GetChildren()
            => ImmutableArray.Create<SyntaxNode>(OperatorToken, Operand );
    }
}