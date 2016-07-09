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
				//Equality
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				//Relational
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

			if (op == null)
			{
				var restrictions = BinderHelper.CreateCommonRestrictions(target, arg);
				BindingEventSource.Log.BinaryOperationResolutionFailure(Operation, target.LimitType.FullName, arg.LimitType.FullName);
				return BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
			}

			switch (Operation)
			{
				case ExpressionType.Add:
				case ExpressionType.Subtract:
				case ExpressionType.Multiply:
					return AddSubtractMultiply(op, target, arg);
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
					return Division(op, target, arg);
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
					return Equality(op, target, arg);
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
					return Relational(op, target, arg);
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
			}

		}


		private DynamicMetaObject Equality(OverloadResolutionData<MethodInfo> op, DynamicMetaObject left, DynamicMetaObject right)
		{
			var leftExpr = VelocityExpressions.ConvertIfNeeded(left.Expression, op.Parameters[0]);
			var rightExpr = VelocityExpressions.ConvertIfNeeded(right.Expression, op.Parameters[1]);

			Expression result = Operation == ExpressionType.Equal
				? Expression.Equal(leftExpr, rightExpr, false, op.FunctionMember)
				: Expression.NotEqual(leftExpr, rightExpr, false, op.FunctionMember);

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);
			result = VelocityExpressions.ConvertIfNeeded(result, ReturnType);

			return new DynamicMetaObject(result, restrictions);
		}

		private DynamicMetaObject Relational(OverloadResolutionData<MethodInfo> op, DynamicMetaObject left, DynamicMetaObject right)
		{
			var leftExpr = VelocityExpressions.ConvertIfNeeded(left.Expression, op.Parameters[0]);
			var rightExpr = VelocityExpressions.ConvertIfNeeded(right.Expression, op.Parameters[1]);

			Expression result;
			switch (Operation)
			{
				case ExpressionType.GreaterThan:
					result = Expression.GreaterThan(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case ExpressionType.GreaterThanOrEqual:
					result = Expression.GreaterThanOrEqual(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case ExpressionType.LessThan:
					result = Expression.LessThan(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case ExpressionType.LessThanOrEqual:
					result = Expression.LessThanOrEqual(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
			}

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);
			result = VelocityExpressions.ConvertIfNeeded(result, ReturnType);

			return new DynamicMetaObject(result, restrictions);

		}

		private DynamicMetaObject AddSubtractMultiply(OverloadResolutionData<MethodInfo> op, DynamicMetaObject target, DynamicMetaObject arg)
		{
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

			var restrictions = BinderHelper.CreateCommonRestrictions(target, arg);
			return new DynamicMetaObject(operation, restrictions);
		}

		private readonly ConstantExpression _zeroSByte = Expression.Constant((sbyte)0);
		private readonly ConstantExpression _zeroShort = Expression.Constant((short)0);
		private readonly ConstantExpression _zeroInt = Expression.Constant((int)0);
		private readonly ConstantExpression _zeroLong = Expression.Constant((long)0);
		private readonly ConstantExpression _zeroByte = Expression.Constant((byte)0);
		private readonly ConstantExpression _zeroUShort = Expression.Constant((ushort)0);
		private readonly ConstantExpression _zeroUInt = Expression.Constant((uint)0);
		private readonly ConstantExpression _zeroULong = Expression.Constant((ulong)0);
		private readonly ConstantExpression _zeroDecimal = Expression.Constant((decimal)0);

		private ConstantExpression GetZeroExpression(DynamicMetaObject obj)
		{
			switch (Type.GetTypeCode(obj.RuntimeType))
			{
				case TypeCode.SByte:
					return _zeroSByte;
				case TypeCode.Byte:
					return _zeroByte;
				case TypeCode.Int16:
					return _zeroShort;
				case TypeCode.UInt16:
					return _zeroUShort;
				case TypeCode.Int32:
					return _zeroInt;
				case TypeCode.UInt32:
					return _zeroUInt;
				case TypeCode.Int64:
					return _zeroLong;
				case TypeCode.UInt64:
					return _zeroULong;
				case TypeCode.Decimal:
					return _zeroDecimal;
				default:
					return null;
			}
		}

		private readonly ConstantExpression _negativeOneSByte = Expression.Constant((sbyte)-1);
		private readonly ConstantExpression _negativeOneShort = Expression.Constant((short)-1);
		private readonly ConstantExpression _negativeOneInt = Expression.Constant((int)-1);
		private readonly ConstantExpression _negativeOneLong = Expression.Constant((long)-1);

		private ConstantExpression GetNegativeOneExpression(DynamicMetaObject obj)
		{
			switch (Type.GetTypeCode(obj.RuntimeType))
			{
				case TypeCode.SByte:
					return _negativeOneSByte;
				case TypeCode.Int16:
					return _negativeOneShort;
				case TypeCode.Int32:
					return _negativeOneInt;
				case TypeCode.Int64:
					return _negativeOneLong;
				default:
					return null;
			}
		}
		private readonly ConstantExpression _maxInt = Expression.Constant(int.MinValue);
		private readonly ConstantExpression _maxLong = Expression.Constant(long.MinValue);

		private DynamicMetaObject Division(OverloadResolutionData<MethodInfo> op, DynamicMetaObject target, DynamicMetaObject arg)
		{
			var restrictions = BinderHelper.CreateCommonRestrictions(target, arg);

			Expression result = null;

			// For signed integers, Division can fail in two ways
			// 1. Divide by Zero: RHS = 0 (Divide by Zero)
			// 2. Overflow - RHS = -1 && LHS = int.MinValue (or long.MinValue)
			// Rather than catching an exception as we do for Add & Subtract, we can detect these cases directly, avoiding
			// exception handling
			// Floating-point division can't fail
			// Decimal division can overflow, so needs an exception handler
			if (op.FunctionMember == null)
			{
				//Divide by Zero
				var argZero = GetZeroExpression(arg);
				if (argZero != null)
				{
					BindingRestrictions zeroRestriction;
					if (argZero.Value.Equals(arg.Value))
					{
						zeroRestriction = BindingRestrictions.GetExpressionRestriction(Expression.Equal(argZero, VelocityExpressions.ConvertIfNeeded(arg.Expression, argZero.Type)));
						result = Constants.VelocityUnresolvableResult;
					}
					else
					{
						zeroRestriction = BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(argZero, VelocityExpressions.ConvertIfNeeded(arg.Expression, argZero.Type)));
					}
					restrictions = restrictions.Merge(zeroRestriction);
				}

				//Overflow
				ConstantExpression lhsMax = null;
				if (target.RuntimeType == typeof(int))
					lhsMax = _maxInt;

				if (target.RuntimeType == typeof(long))
					lhsMax = _maxLong;

				if (lhsMax != null)
				{
					var negativeOne = GetNegativeOneExpression(arg);

					if (negativeOne != null)
					{
						BindingRestrictions overflowRestrictions;
						if (target.Value.Equals(lhsMax.Value) && arg.Value.Equals(negativeOne.Value))
						{
							result = Constants.VelocityUnresolvableResult;
							overflowRestrictions = BindingRestrictions.GetExpressionRestriction(
									Expression.AndAlso(
										Expression.Equal(target.Expression, lhsMax),
										Expression.Equal(arg.Expression, negativeOne)
									)
								);
						}
						else
						{
							overflowRestrictions = BindingRestrictions.GetExpressionRestriction(
									Expression.OrElse(
										Expression.NotEqual(lhsMax, VelocityExpressions.ConvertIfNeeded(target.Expression, lhsMax.Type)),
										Expression.NotEqual(negativeOne, VelocityExpressions.ConvertIfNeeded(arg.Expression, lhsMax.Type))
									)
								);
						}
						restrictions = restrictions.Merge(overflowRestrictions);
					}
				}

			}

			if (result == null)
			{
				var targetExpr = VelocityExpressions.ConvertIfNeeded(target, op.Parameters[0]);
				var argExpr = VelocityExpressions.ConvertIfNeeded(arg, op.Parameters[1]);
				if (Operation == ExpressionType.Divide)
					result = Expression.Divide(targetExpr, argExpr, op.FunctionMember);
				else if (Operation == ExpressionType.Modulo)
					result = Expression.Modulo(targetExpr, argExpr, op.FunctionMember);
				else
					throw new InvalidOperationException();
			}

			if (op.Parameters[0] == typeof(decimal) && op.Parameters[1] == typeof(decimal))
			{
				result = Expression.TryCatch(
					VelocityExpressions.ConvertIfNeeded(result, ReturnType),
					Expression.Catch(typeof(ArithmeticException),
						Constants.VelocityUnresolvableResult
					)
				);
			}

			var operation = VelocityExpressions.ConvertIfNeeded(result, ReturnType);

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
				Expression.Catch(typeof(ArithmeticException),
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
				case ExpressionType.Divide:
					return _builtInDivisionOperators;
				case ExpressionType.Modulo:
					return _builtInModulusOperators;
				case ExpressionType.Equal:
					return _builtInEqualityOperators;
				case ExpressionType.NotEqual:
					return _builtInInequalityOperators;
				case ExpressionType.GreaterThan:
					return _builtInGreaterThanOperators;
				case ExpressionType.GreaterThanOrEqual:
					return _builtInGreaterThanOrEqualOperators;
				case ExpressionType.LessThan:
					return _builtInLessThanOperators;
				case ExpressionType.LessThanOrEqual:
					return _builtInLessThanOrEqualOperators;
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
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
					return MultiplicationMethodName;
				case ExpressionType.Divide:
					return DivisionMethodName;
				case ExpressionType.Modulo:
					return ModuloMethodName;
				case ExpressionType.Equal:
					return EqualityMethodName;
				case ExpressionType.NotEqual:
					return InequalityMethodName;
				case ExpressionType.GreaterThan:
					return GreaterThanMethodName;
				case ExpressionType.GreaterThanOrEqual:
					return GreaterThanOrEqualMethodName;
				case ExpressionType.LessThan:
					return LessThanMethodName;
				case ExpressionType.LessThanOrEqual:
					return LessThanOrEqualMethodName;
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
			}

		}

		private const string AdditionMethodName = "op_Addition";
		private const string SubtractionMethodName = "op_Subtraction";
		private const string MultiplicationMethodName = "op_Multiply";
		private const string DivisionMethodName = "op_Division";
		private const string ModuloMethodName = "op_Modulus";
		private const string EqualityMethodName = "op_Equality";
		private const string InequalityMethodName = "op_Inequality";
		private const string GreaterThanMethodName = "op_GreaterThan";
		private const string GreaterThanOrEqualMethodName = "op_GreaterThanOrEqual";
		private const string LessThanMethodName = "op_LessThan";
		private const string LessThanOrEqualMethodName = "op_LessThanOrEqual";



		static VelocityBinaryOperationBinder()
		{
			var commonBuiltInOperators = ImmutableArray.Create(
				ClrIntrinsic<int, int>(),
				ClrIntrinsic<uint, uint>(),
				ClrIntrinsic<long, long>(),
				ClrIntrinsic<ulong, ulong>(),
				ClrIntrinsic<float, float>(),
				ClrIntrinsic<double, double>()
				);

			var commonEquality = commonBuiltInOperators.Add(ClrIntrinsic<bool, bool>());


			_builtInAdditionOperators = commonBuiltInOperators.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(AdditionMethodName),
				VelocityIntrinsic<string, string>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null)),
				VelocityIntrinsic<string, object>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null)),
				VelocityIntrinsic<object, string>(typeof(string).GetMethod("Concat", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null))
			});

			_builtInSubtractionOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(SubtractionMethodName));
			_builtInMultiplicationOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(MultiplicationMethodName));
			_builtInDivisionOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(DivisionMethodName));
			_builtInModulusOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(ModuloMethodName));

			_builtInGreaterThanOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(GreaterThanMethodName));
			_builtInGreaterThanOrEqualOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(GreaterThanOrEqualMethodName));
			_builtInLessThanOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(LessThanMethodName));
			_builtInLessThanOrEqualOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(LessThanOrEqualMethodName));

			_builtInEqualityOperators = commonEquality.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(EqualityMethodName),
				BuiltInOperator<string, string>(EqualityMethodName),
			});
			_builtInInequalityOperators = commonEquality.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(InequalityMethodName),
				BuiltInOperator<string, string>(InequalityMethodName),
			});
		}

		private static readonly ImmutableArray<FunctionMemberData<MethodInfo>>
			_builtInAdditionOperators, _builtInSubtractionOperators,
			_builtInMultiplicationOperators ,_builtInDivisionOperators, _builtInModulusOperators,
			_builtInEqualityOperators, _builtInInequalityOperators,
			_builtInGreaterThanOperators, _builtInGreaterThanOrEqualOperators,
			_builtInLessThanOperators, _builtInLessThanOrEqualOperators;

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
			var types = new[] { typeof(TLeft), typeof(TRight) };
			var method = typeof(TLeft).GetTypeInfo().GetMethod(operatorName, BindingFlags.Static, null, types, null);
			return new FunctionMemberData<MethodInfo>(method, types);
		}

	}
}
