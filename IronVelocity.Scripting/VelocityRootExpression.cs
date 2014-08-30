using IronVelocity.Compilation.AST;
using IronVelocity.Compilation.Directives;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronVelocity.Scripting
{
    public class VelocityRootExpression : VelocityExpression
    {
        public IReadOnlyCollection<Expression> Children {get; private set;}
        private readonly IDictionary<Type, DirectiveExpressionBuilder> _directiveHandlers;
        private readonly string _name;
        private readonly bool _staticTypeWherePossible;
        private readonly VelocityCompilerOptions _compilerOptions;
        public readonly VelocityExpressionBuilder _builder;

        public VelocityRootExpression(ASTprocess node, VelocityCompilerOptions compilerOptions, string name)
        {
            _builder = new VelocityExpressionBuilder(_directiveHandlers);
            Children = _builder.GetBlockExpressions(node);
        }

        public override Expression Reduce()
        {
            return new RenderedBlock(Children, _builder);
        }
        
    }
}
