using NVelocity.Runtime;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;


namespace IronVelocity
{
    public static class VelocityExpressionTreeBuilder
    {
        private static RuntimeInstance _runtimeService;

        static VelocityExpressionTreeBuilder()
        {
            _runtimeService = new RuntimeInstance();
            _runtimeService.Init();
        }

        public static Expression<Action<StringBuilder>> BuildExpressionTree(string input, IDictionary<string, object> context)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;
                if (ast == null)
                    throw new InvalidProgramException("Unable to parse ast");

                var converter = new VelocityASTConverter();
                var expr = converter.BuildExpressionTree(ast, context);

                return Expression.Lambda<Action<StringBuilder>>(expr, "Velocity_TODO", new[] { Constants.OutputParameter });
                //return Expression.Lambda<Action<IDictionary<string,object>, StringBuilder>>(expr, Constants.InputParameter, Constants.OutputParameter);
            }
        }
    }

}
