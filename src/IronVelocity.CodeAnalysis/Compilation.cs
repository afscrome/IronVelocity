using IronVelocity.CodeAnalysis.Binding;
using IronVelocity.CodeAnalysis.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.CodeAnalysis
{
    public class Compilation
    {
        public Compilation(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }

        public EvaluationResult Evaluate()
        {
            var binder = new Binder();
            var boundExpression = binder.BindExpression(SyntaxTree.Root.Expression);

            var diagnostics = SyntaxTree.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();

            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics);
            }

            var evaluator = new Evaluator(boundExpression);
            var value = evaluator.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}
