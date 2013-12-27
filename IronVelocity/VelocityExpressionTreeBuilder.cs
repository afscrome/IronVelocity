using NVelocity.Runtime;
using NVelocity.Runtime.Parser.Node;
using System;
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

        public static Expression<Action<StringBuilder>> BuildExpressionTree(string input)
        {
            var parser = _runtimeService.CreateNewParser();
            using (var reader = new StringReader(input))
            {
                var ast = parser.Parse(reader, null) as ASTprocess;

                var converter = new VelocityASTConverter();
                var expr = converter.BuildExpressionTree(ast);

                return Expression.Lambda<Action<StringBuilder>>(expr, Constants.OutputParameter);
            }
        }
    }

}
