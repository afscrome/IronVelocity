using IronVelocity.CodeAnalysis.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Binding
{
    public class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type type):
            this(syntaxKind, kind, type, type)
        {
        }

        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            OperandType = operandType;
            ResultType = resultType ?? operandType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public Type OperandType { get; }
        public Type ResultType { get; }

        private static readonly ImmutableArray<BoundUnaryOperator> _operators = ImmutableArray.Create(
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),
            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int))
        );

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, Type operandType)
        {
            return _operators.SingleOrDefault(x => x.SyntaxKind == syntaxKind && x.OperandType == operandType);
        }

    }

}
