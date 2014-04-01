using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation.AST
{
    public class Directive : VelocityExpression
    {
        public string Name { get; private set; }
        public ASTDirective Node { get; private set; }

        public Directive(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var directive = node as ASTDirective;
            if (directive == null)
                throw new ArgumentOutOfRangeException("node");

            Name = directive.DirectiveName;
            Node = directive;
        }

        protected override Expression ReduceInternal()
        {
            if (Node.Directive == null)
                return Expression.Constant(Node.Literal);

            DirectiveExpressionBuilder builder;
            if (_directiveHandlers.TryGetValue(Node.Directive.GetType(), out builder))
                return builder.Build(Node);
            else
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Unable to handle directive type '{0}'", Node.DirectiveName));
        }


        private static readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers = new Dictionary<Type, DirectiveExpressionBuilder>()
        {
            {typeof(Foreach), new ForEachDirectiveExpressionBuilder()},
            {typeof(ForeachBeforeAllSection), new ForEachSectionExpressionBuilder(ForEachSection.BeforeAll)},
            {typeof(ForeachBeforeSection), new ForEachSectionExpressionBuilder(ForEachSection.Before)},
            {typeof(ForeachEachSection), new ForEachSectionExpressionBuilder(ForEachSection.Each)},
            {typeof(ForeachOddSection), new ForEachSectionExpressionBuilder(ForEachSection.Odd)},
            {typeof(ForeachEvenSection), new ForEachSectionExpressionBuilder(ForEachSection.Even)},
            {typeof(ForeachBetweenSection), new ForEachSectionExpressionBuilder(ForEachSection.Between)},
            {typeof(ForeachAfterSection), new ForEachSectionExpressionBuilder(ForEachSection.After)},
            {typeof(ForeachAfterAllSection), new ForEachSectionExpressionBuilder(ForEachSection.AfterAll)},
            {typeof(ForeachNoDataSection), new ForEachSectionExpressionBuilder(ForEachSection.NoData)},
        };

        public override Type Type { get { return typeof(void); } }
    }

}
