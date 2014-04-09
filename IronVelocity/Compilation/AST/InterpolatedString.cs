using NVelocity.Exception;
using NVelocity.Runtime.Parser.Node;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Compilation.AST
{
    public class InterpolatedString : VelocityExpression
    {
        public string Value { get; private set; }

        public InterpolatedString(string value)
        {
            Value = value;
        }

        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object[]) }, null);

        public override Expression Reduce()
        {
            //TODO; Refactor to share with VelocityExpressionTreeBuilder, or reuse the same parser
            var parser = new NVelocity.Runtime.RuntimeInstance().CreateNewParser();
            using (var reader = new System.IO.StringReader(Value))
            {
                SimpleNode ast;
                try
                {
                    ast = parser.Parse(reader, null);
                }
                catch (ParseErrorException)
                {
                    ast = null;
                }

                //If we fail to parse, the ast returned will be null, so just return our normal string
                if (ast == null)
                    return Expression.Constant(Value);

                var expressions =  new VelocityExpressionBuilder(null).GetBlockExpressions(ast)
                    .Where(x => x.Type != typeof(void))
                    .ToArray();

                if (expressions.Length == 1)
                    return expressions[0];
                else
                    return Expression.Call(_stringConcatMethodInfo, Expression.NewArrayInit(typeof(object), expressions));
            }
        }

    }
}
