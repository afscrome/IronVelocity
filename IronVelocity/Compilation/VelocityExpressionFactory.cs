using IronVelocity.Binders;
using IronVelocity.Compilation.AST;
using IronVelocity.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Compilation
{
    public class VelocityExpressionFactory
    {
        private readonly IBinderFactory _binderFactory;
        public VelocityExpressionFactory(IBinderFactory binderFactory)
        {
            _binderFactory = binderFactory;
        }

        public virtual Expression Variable(string name, SourceInfo sourceInfo)
            => new VariableExpression(name);

        public virtual Expression Property(Expression target, string name, SourceInfo sourceInfo)
            => new PropertyAccessExpression(target, name, sourceInfo, _binderFactory.GetGetMemberBinder(name));

        public virtual Expression Method(Expression target, string name, IImmutableList<Expression> args, SourceInfo sourceInfo)
            => new MethodInvocationExpression(target, args, sourceInfo, _binderFactory.GetInvokeMemberBinder(name, args?.Count ?? 0));

        public virtual Expression Index(Expression target, IImmutableList<Expression> args, SourceInfo sourceInfo)
            => new IndexInvocationExpression(target, args, sourceInfo, _binderFactory.GetGetIndexBinder(args.Count));

        public virtual Expression Comparison(Expression left, Expression right, ComparisonOperation operation, SourceInfo sourceInfo)
            => new ComparisonExpression(left, right, sourceInfo, _binderFactory.GetComparisonOperationBinder(operation));

        public virtual Expression Maths(Expression left, Expression right, SourceInfo sourceInfo, MathematicalOperation operation)
            => new MathematicalExpression(left, right, sourceInfo, operation, _binderFactory.GetMathematicalOperationBinder(operation));

        public virtual Expression Assign(Expression target, Expression value, SourceInfo sourceInfo)
            => new SetDirective(target, value, sourceInfo, _binderFactory);

    }

    public class StaticTypedVelocityExpressionFactory : VelocityExpressionFactory
    {
        private IImmutableDictionary<string, object> _globals;
        private readonly IMemberResolver _memberResolver = new MemberResolver();
        private readonly IIndexResolver _indexResolver = new IndexResolver(new OverloadResolver(new ArgumentConverter()));
        private readonly IMethodResolver _methodResolver = new MethodResolver(new OverloadResolver(new ArgumentConverter()), new ArgumentConverter());

        private readonly IDictionary<string, Expression> _variableCache = new Dictionary<string, Expression>();

        public StaticTypedVelocityExpressionFactory(IBinderFactory binderFactory, IImmutableDictionary<string, object> globals)
            : base(binderFactory)
        {
            var nullGlobals = globals?.Where(x => x.Value == null);
            if (nullGlobals?.Any() ?? false)
            {
                throw new ArgumentOutOfRangeException(nameof(globals), $"The following global variables must not be null: {String.Join(", ", nullGlobals.Select(x => x.Key))}");
            }

            _globals = globals;
        }

        public override Expression Variable(string name, SourceInfo sourceInfo)
        {
            Expression result;
            if (!_variableCache.TryGetValue(name, out result))
            {
                object global = null;
                if (_globals?.TryGetValue(name, out global) ?? false)
                {
                    _variableCache[name] = result = new GlobalVariableExpression(name, global);
                }
                else
                {
                    result = base.Variable(name, sourceInfo);
                }
                _variableCache[name] = result;
            }

            return result;
        }

        public override Expression Property(Expression target, string name, SourceInfo sourceInfo)
        {
            if (IsConstantType(target))
            {
                //TODO: if null, log (or error in strict mode)
                return _memberResolver.MemberExpression(name, target.Type, target, Reflection.MemberAccessMode.Read)
                    ?? Constants.NullExpression;
            }

            return base.Property(target, name, sourceInfo);
        }

        public override Expression Index(Expression target, IImmutableList<Expression> args, SourceInfo sourceInfo)
        {
            if (IsConstantType(target) && args.All(IsConstantType))
            {
                var indexExpression = _indexResolver.ReadableIndexer(new DynamicMetaObject(target, BindingRestrictions.Empty), args.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray());

                return indexExpression
                    ?? Constants.NullExpression;
            }

            return base.Index(target, args, sourceInfo);
        }

        public override Expression Method(Expression target, string name, IImmutableList<Expression> args, SourceInfo sourceInfo)
        {
            if (IsConstantType(target) && args.All(IsConstantType))
            {
                var methodInfo = _methodResolver.ResolveMethod(target.Type.GetTypeInfo(), name, args.Select(x => x.Type).ToImmutableArray());
                //TODO: Include debug info
                if (methodInfo == null)
                    return Constants.NullExpression;

                return _methodResolver.ConvertMethodParameters(methodInfo, target, args.Select(x => new DynamicMetaObject(x, BindingRestrictions.Empty)).ToArray())
                    ?? Constants.NullExpression;
            }

            return base.Method(target, name, args, sourceInfo);
        }

        public override Expression Comparison(Expression left, Expression right, ComparisonOperation operation, SourceInfo sourceInfo)
        {
            if (IsConstantType(left) && IsConstantType(right))
            {
                //TODO: If types are static, we can optimise here with a static expression, rather than dynamic
            }
            return base.Comparison(left, right, operation, sourceInfo);
        }

        public override Expression Maths(Expression left, Expression right, SourceInfo sourceInfo, MathematicalOperation operation)
        {
            if (IsConstantType(left) && IsConstantType(right))
            {
                //TODO: If types are static, we can optimise here with a static expression, rather than dynamic
            }
            return base.Maths(left, right, sourceInfo, operation);
        }

        public override Expression Assign(Expression target, Expression value, SourceInfo sourceInfo)
        {
            if (target is GlobalVariableExpression)
                throw new InvalidOperationException("Cannot assign to a global variable");

            //TODO: Are there any static typing optimisations that can be done?
            if (IsConstantType(target) && IsConstantType(value))
            {

            }


            return base.Assign(target, value, sourceInfo);
        }

        private static bool IsConstantType(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression is DynamicExpression || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(expression.Type))
                return false;

            if (expression is ConstantExpression || expression is DefaultExpression
                || expression is InterpolatedStringExpression || expression is DictionaryStringExpression
                || expression is DictionaryExpression || expression is ObjectArrayExpression
                || expression is GlobalVariableExpression || expression is IntegerRangeExpression)
                return true;

            if (expression.Type == typeof(void))
                return true;

            if (!TypeHelper.IsNullableType(expression.Type))
                return true;

            //If the return type is sealed, we can't get any subclasses back
            //But it could be null
            //if (expression.Type.IsSealed)
            //    return true;

            var reference = expression as ReferenceExpression;
            if (reference != null && IsConstantType(reference.Value))
                return true;

            return false;
        }

    }
}
