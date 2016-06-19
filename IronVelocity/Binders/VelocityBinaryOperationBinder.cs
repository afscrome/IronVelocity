using IronVelocity.Compilation;
using IronVelocity.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IronVelocity.Binders
{
	public partial class VelocityBinaryOperationBinder : BinaryOperationBinder
	{
		private readonly IOverloadResolver _overloadResolver = new OverloadResolver(new ArgumentConverter());

		public VelocityBinaryOperationBinder(ExpressionType expressionType)
			: base(expressionType)
		{
			switch (expressionType)
			{
				//Relational
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				//Math
				case ExpressionType.Add:
				case ExpressionType.Subtract:
				case ExpressionType.Multiply:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(expressionType));
			}
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (arg == null)
				throw new ArgumentNullException(nameof(arg));

			if (!target.HasValue || !arg.HasValue)
				return Defer(target, arg);

			var targetType = target.LimitType;
			var argType = arg.LimitType;

			var candidates = GetCandidateOperators(targetType, argType);

			var op = _overloadResolver.Resolve(candidates, ImmutableList.Create(targetType, argType));


			var restrictions = BinderHelper.CreateCommonRestrictions(target, arg);

			if (op == null)
			{
				BindingEventSource.Log.BinaryOperationResolutionFailure(Operation, target.LimitType.FullName, arg.LimitType.FullName);
				return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
			}

			var targetExpr = VelocityExpressions.ConvertIfNeeded(target, op.Parameters[0]);
			var argExpr = VelocityExpressions.ConvertIfNeeded(arg, op.Parameters[1]);

			Func<Expression, Expression, MethodInfo, Expression> operationFunc;

			switch (Operation)
			{
				case ExpressionType.Add:
					operationFunc = Expression.AddChecked;
					break;
				case ExpressionType.Subtract:
					operationFunc = Expression.SubtractChecked;
					break;
				case ExpressionType.Multiply:
					operationFunc = Expression.MultiplyChecked;
					break;
				default:
					throw new InvalidOperationException();
			}

			Expression operation = operationFunc(targetExpr, argExpr, op.FunctionMember);

			if (op.FunctionMember == null)
				operation = AddOverflowHandling(operation, target.Expression, arg.Expression);

			operation = VelocityExpressions.ConvertIfNeeded(operation, ReturnType);

			return new DynamicMetaObject(operation, restrictions);
		}


		private Expression AddOverflowHandling(Expression operation, Expression left, Expression right)
		{
			if (!TypeHelper.IsInteger(left.Type) || !TypeHelper.IsInteger(right.Type))
				return operation;

			Func<Expression, Expression, Expression> overflowHandler;


			switch (Operation)
			{
				case ExpressionType.Add:
					overflowHandler = Expression.Add;
					break;
				case ExpressionType.Subtract:
					overflowHandler = Expression.Subtract;
					break;
				case ExpressionType.Multiply:
					overflowHandler = Expression.Multiply;
					break;
				default:
					return operation;
			}

			var overflowExpression = overflowHandler(
				BigIntegerHelper.ConvertToBigInteger(left),
				BigIntegerHelper.ConvertToBigInteger(right)
			);


			var useSignedIntegerTypes = TypeHelper.IsSignedInteger(left.Type);
			//Pass the final result into ReduceBigInteger(...) to return a more recognizable primitive
			var overflowFallback = VelocityExpressions.ConvertIfNeeded(
					Expression.Call(MethodHelpers.ReduceBigIntegerMethodInfo, overflowExpression, Expression.Constant(useSignedIntegerTypes)),
					ReturnType
			);
			return Expression.TryCatch(
				VelocityExpressions.ConvertIfNeeded(operation, ReturnType),
				Expression.Catch(typeof(OverflowException),
					overflowFallback
				)
			);
		}

		private IImmutableList<FunctionMemberData<MethodInfo>> GetCandidateOperators(Type left, Type right)
		{
			var userDefinedCandidates = GetCandidateUserDefinedOperators(left, right);
			
			return userDefinedCandidates.Any()
				? userDefinedCandidates
				: GetBuiltInOperators();

		}

		private IImmutableList<FunctionMemberData<MethodInfo>> GetCandidateUserDefinedOperators(Type left, Type right)
		{
			var operatorName = GetOperatorName();

			// The set of candidate user-defined operators provided by X and Y for the operation operator op(x, y) is determined.
			//The set consists of the union of the candidate operators provided by X and the candidate operators provided by Y
			var candidates = GetCandidateOperatorsForType(operatorName, left);

			//If X and Y are the same type, or if X and Y are derived from a common base type, then shared candidate operators only occur in the combined set once.
			if (left != right)
				candidates = candidates.Union(GetCandidateOperatorsForType(operatorName, right));

			return candidates.Select(x => new FunctionMemberData<MethodInfo>(x, x.GetParameters()))
				.ToImmutableList();
		}


		private IEnumerable<MethodInfo> GetCandidateOperatorsForType(string operatorName, Type type)
		{
			//Given a type T and an operation operator op(A), where op is an overloadable operator and A is an argument list,
			//the set of candidate user-defined operators provided by T for operator op(A) is determined as follows:

			while (type != null)
			{
				//Determine the type T0. If T is a nullable type, T0 is its underlying type, otherwise T0 is equal to T.
				type = TypeHelper.LiftIfNullable(type);

				//For all operator op declarations in T0 and all lifted forms of such operators, if at least one operator is
				// applicable (§7.5.3.1) with respect to the argument list A, then the set of candidate operators consists
				// of all such applicable operators in T0.
				var candidates = type.GetRuntimeMethods()
					.Where(x => x.IsPublic && x.IsStatic)
					.Where(x => x.DeclaringType == type)
					.Where(x => x.Name == operatorName);

				if (candidates.Any())
					return candidates;

				//Otherwise, if T0 is object, the set of candidate operators is empty.
				if (type == typeof(object))
					return ImmutableList<MethodInfo>.Empty;

				//Otherwise, the set of candidate operators provided by T0 is the set of candidate operators provided by the direct base class of T0
				//or the effective base class of T0 if T0 is a type parameter.
				type = type.BaseType.GetTypeInfo();
			}

			return Enumerable.Empty<MethodInfo>();
		}


		private IImmutableList<FunctionMemberData<MethodInfo>> GetBuiltInOperators()
		{
			switch (Operation)
			{
				case ExpressionType.Add:
					return _builtInAdditionOperators;
				case ExpressionType.Subtract:
					return _builtInSubtractionOperators;
				case ExpressionType.Multiply:
					return _builtInMultiplicationOperators;
				default:
					throw new Exception();
			}

		}

		private string GetOperatorName()
		{
			switch (Operation)
			{
				case ExpressionType.Add:
					return AdditionMethodName;
				case ExpressionType.Subtract:
					return SubtractionMethodName;
				case ExpressionType.Multiply:
					return MultiplicationMethodNameName;
				default:
					throw new Exception();
			}

		}

		private const string AdditionMethodName = "op_Addition";
		private const string SubtractionMethodName = "op_Subtraction";
		private const string MultiplicationMethodNameName = "op_Multiply";


		private static readonly ImmutableArray<FunctionMemberData<MethodInfo>> _builtInAdditionOperators = ImmutableArray.Create(
				ClrIntrinsic<int, int>(),
				ClrIntrinsic<uint, uint>(),
				ClrIntrinsic<long, long>(),
				ClrIntrinsic<ulong, ulong>(),
				ClrIntrinsic<float, float>(),
				ClrIntrinsic<double, double>(),
				BuiltInOperator<decimal, decimal>(AdditionMethodName),
				VelocityIntrinsic<string, string>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null)),
				VelocityIntrinsic<string, object>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null)),
				VelocityIntrinsic<object, string>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null))
			);

		private static readonly ImmutableArray<FunctionMemberData<MethodInfo>> _builtInSubtractionOperators = ImmutableArray.Create(
				ClrIntrinsic<int, int>(),
				ClrIntrinsic<uint, uint>(),
				ClrIntrinsic<long, long>(),
				ClrIntrinsic<ulong, ulong>(),
				ClrIntrinsic<float, float>(),
				ClrIntrinsic<double, double>(),
				BuiltInOperator<decimal, decimal>(SubtractionMethodName)
			);

		private static readonly ImmutableArray<FunctionMemberData<MethodInfo>> _builtInMultiplicationOperators = ImmutableArray.Create(
				ClrIntrinsic<int, int>(),
				ClrIntrinsic<uint, uint>(),
				ClrIntrinsic<long, long>(),
				ClrIntrinsic<ulong, ulong>(),
				ClrIntrinsic<float, float>(),
				ClrIntrinsic<double, double>(),
				BuiltInOperator<decimal, decimal>(MultiplicationMethodNameName)
			);

		private static FunctionMemberData<MethodInfo> ClrIntrinsic<TLeft, TRight>()
			=> new FunctionMemberData<MethodInfo>(null, typeof(TLeft), typeof(TRight));

		private static FunctionMemberData<MethodInfo> VelocityIntrinsic<TLeft, TRight>(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return new FunctionMemberData<MethodInfo>(method, typeof(TLeft), typeof(TRight));
		}

		private static FunctionMemberData<MethodInfo> BuiltInOperator<TLeft, TRight>(string operatorName)
		{
			var types = new[] { typeof(TLeft), typeof(TRight)};
			var method = typeof(TLeft).GetTypeInfo().GetMethod(operatorName, BindingFlags.Static, null, types, null);
			return new FunctionMemberData<MethodInfo>(method, types);
		}

	}
}
