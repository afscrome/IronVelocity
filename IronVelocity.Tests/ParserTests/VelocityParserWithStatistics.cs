using IronVelocity.Parser;
using IronVelocity.Parser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Tests.ParserTests
{
    internal class VelocityParserWithStatistics : VelocityParser
    {
        public int RangeOrListCallCount { get; private set; }
        public int ExpressionCallCount { get; private set; }
        public int IntegerCallCount { get; private set; }
        public int FloatCallCount { get; private set; }
        public int StringLiteralCallCount { get; private set; }
        public int InterpolatedStringCallCount { get; private set; }
        public int BooleanLiteralOrWordCallCount { get; private set; }
        public int ReferenceCallCount { get; private set; }
        public int VariableCallCount { get; private set; }
        public int MethodCallCount { get; private set; }
        public int PropertyCallCount { get; private set; }

        public VelocityParserWithStatistics(string input)
            : base(input)
        {
        }

        public override ExpressionNode Expression()
        {
            ExpressionCallCount++;
            return base.Expression();
        }

        protected override ExpressionNode RangeOrList()
        {
            RangeOrListCallCount++;
            return base.RangeOrList();
        }

        protected override IntegerLiteralNode IntegerLiteral(string integerPart)
        {
            IntegerCallCount++;
            return base.IntegerLiteral(integerPart);
        }

        protected override FloatingPointLiteralNode FloatingPointLiteral(string integerPart)
        {
            FloatCallCount++;
            return base.FloatingPointLiteral(integerPart);
        }

        protected override StringNode StringLiteral()
        {
            StringLiteralCallCount++;
            return base.StringLiteral();
        }
        protected override StringNode InterpolatedString()
        {
            InterpolatedStringCallCount++;
            return base.InterpolatedString();
        }

        protected override ExpressionNode BooleanLiteralOrWord()
        {
            BooleanLiteralOrWordCallCount++;
            return base.BooleanLiteralOrWord();
        }

        protected override ReferenceNode Reference()
        {
            ReferenceCallCount++;
            return base.Reference();
        }

        protected override Variable Variable(Token identifier)
        {
            VariableCallCount++;
            return base.Variable(identifier);
        }

        protected override Property Property(ReferenceNodePart target, Token identifier)
        {
            PropertyCallCount++;
            return base.Property(target, identifier);
        }

        protected override Method Method(ReferenceNodePart target, Token identifier)
        {
            MethodCallCount++;
            return base.Method(target, identifier);
        }

    }
}
