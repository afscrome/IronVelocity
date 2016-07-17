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
		private readonly IOperatorResolver _operatorResolver = new OperatorResolver();

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

			DynamicMetaObject result;

			switch (Operation)
			{
				case ExpressionType.Add:
					result = AddSubtractMultiply(VelocityOperator.Add, target, arg);
					break;
				case ExpressionType.Subtract:
					result = AddSubtractMultiply(VelocityOperator.Subtract, target, arg);
					break;
				case ExpressionType.Multiply:
					result = AddSubtractMultiply(VelocityOperator.Multiply, target, arg);
					break;
				case ExpressionType.Divide:
					result = Division(VelocityOperator.Divide, target, arg);
					break;
				case ExpressionType.Modulo:
					result = Division(VelocityOperator.Modulo, target, arg);
					break;
				case ExpressionType.Equal:
					result = Equality(VelocityOperator.Equal, target, arg);
					break;
				case ExpressionType.NotEqual:
					result = Equality(VelocityOperator.NotEqual, target, arg);
					break;
				case ExpressionType.GreaterThan:
					result = Relational(VelocityOperator.GreaterThan, target, arg);
					break;
				case ExpressionType.GreaterThanOrEqual:
					result = Relational(VelocityOperator.GreaterThanOrEqual, target, arg);
					break;
				case ExpressionType.LessThan:
					result = Relational(VelocityOperator.LessThan, target, arg);
					break;
				case ExpressionType.LessThanOrEqual:
					result = Relational(VelocityOperator.LessThanOrEqual, target, arg);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
			}

			if (result == null)
			{
				var restrictions = BinderHelper.CreateCommonRestrictions(target, arg);
				BindingEventSource.Log.BinaryOperationResolutionFailure(Operation, target.LimitType.FullName, arg.LimitType.FullName);
				result = BinderHelper.UnresolveableResult(restrictions, errorSuggestion);
			}
			return result;
		}


		private static readonly MethodInfo ObjectEquals = typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object), typeof(object) }, null);

		private DynamicMetaObject Equality(VelocityOperator operatorType, DynamicMetaObject left, DynamicMetaObject right)
		{
			var op = _operatorResolver.Resolve(operatorType, left.RuntimeType, right.RuntimeType);
			Expression result;
			if (op != null)
			{
				var leftExpr = VelocityExpressions.ConvertIfNeeded(left.Expression, op.Parameters[0]);
				var rightExpr = VelocityExpressions.ConvertIfNeeded(right.Expression, op.Parameters[1]);

				result = operatorType == VelocityOperator.Equal
					? Expression.Equal(leftExpr, rightExpr, false, op.FunctionMember)
					: Expression.NotEqual(leftExpr, rightExpr, false, op.FunctionMember);
			}
			else
			{
				var leftExpr = VelocityExpressions.ConvertIfNeeded(left.Expression, typeof(object));
				var rightExpr = VelocityExpressions.ConvertIfNeeded(right.Expression, typeof(object));

				result = Expression.Equal(leftExpr, rightExpr, false, ObjectEquals);
				if (operatorType == VelocityOperator.NotEqual)
					result = Expression.Not(result);
			}

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);
			result = VelocityExpressions.ConvertIfNeeded(result, ReturnType);

			return new DynamicMetaObject(result, restrictions);
		}



		private DynamicMetaObject Relational(VelocityOperator operatorType, DynamicMetaObject left, DynamicMetaObject right)
		{
			var op = _operatorResolver.Resolve(operatorType, left.RuntimeType, right.RuntimeType);
			if (op == null)
			{
				if (operatorType == VelocityOperator.GreaterThanOrEqual || operatorType == VelocityOperator.LessThanOrEqual)
					return Equality(VelocityOperator.Equal, left, right);
				else
					return null;
			}


			var leftExpr = VelocityExpressions.ConvertIfNeeded(left.Expression, op.Parameters[0]);
			var rightExpr = VelocityExpressions.ConvertIfNeeded(right.Expression, op.Parameters[1]);

			Expression result;
			switch (operatorType)
			{
				case VelocityOperator.GreaterThan:
					result = Expression.GreaterThan(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case VelocityOperator.GreaterThanOrEqual:
					result = Expression.GreaterThanOrEqual(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case VelocityOperator.LessThan:
					result = Expression.LessThan(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				case VelocityOperator.LessThanOrEqual:
					result = Expression.LessThanOrEqual(leftExpr, rightExpr, false, op.FunctionMember);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Operation));
			}

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);
			result = VelocityExpressions.ConvertIfNeeded(result, ReturnType);

			return new DynamicMetaObject(result, restrictions);

		}

		private DynamicMetaObject AddSubtractMultiply(VelocityOperator operatorType, DynamicMetaObject left, DynamicMetaObject right)
		{
			var op = _operatorResolver.Resolve(operatorType, left.RuntimeType, right.RuntimeType);
			if (op == null)
				return null;

			var targetExpr = VelocityExpressions.ConvertIfNeeded(left, op.Parameters[0]);
			var argExpr = VelocityExpressions.ConvertIfNeeded(right, op.Parameters[1]);

			Func<Expression, Expression, MethodInfo, Expression> operationFunc;

			switch (operatorType)
			{
				case VelocityOperator.Add:
					operationFunc = Expression.AddChecked;
					break;
				case VelocityOperator.Subtract:
					operationFunc = Expression.SubtractChecked;
					break;
				case VelocityOperator.Multiply:
					operationFunc = Expression.MultiplyChecked;
					break;
				default:
					throw new InvalidOperationException();
			}

			Expression operation = operationFunc(targetExpr, argExpr, op.FunctionMember);

			if (op.FunctionMember == null)
				operation = AddOverflowHandling(operation, left.Expression, right.Expression);

			operation = VelocityExpressions.ConvertIfNeeded(operation, ReturnType);

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);
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

		private DynamicMetaObject Division(VelocityOperator operatorType, DynamicMetaObject left, DynamicMetaObject right)
		{
			var op = _operatorResolver.Resolve(operatorType, left.RuntimeType, right.RuntimeType);
			if (op == null)
				return null;

			var restrictions = BinderHelper.CreateCommonRestrictions(left, right);

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
				var argZero = GetZeroExpression(right);
				if (argZero != null)
				{
					BindingRestrictions zeroRestriction;
					if (argZero.Value.Equals(right.Value))
					{
						zeroRestriction = BindingRestrictions.GetExpressionRestriction(Expression.Equal(argZero, VelocityExpressions.ConvertIfNeeded(right.Expression, argZero.Type)));
						result = Constants.VelocityUnresolvableResult;
					}
					else
					{
						zeroRestriction = BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(argZero, VelocityExpressions.ConvertIfNeeded(right.Expression, argZero.Type)));
					}
					restrictions = restrictions.Merge(zeroRestriction);
				}

				//Overflow
				ConstantExpression lhsMax = null;
				if (left.RuntimeType == typeof(int))
					lhsMax = _maxInt;

				if (left.RuntimeType == typeof(long))
					lhsMax = _maxLong;

				if (lhsMax != null)
				{
					var negativeOne = GetNegativeOneExpression(right);

					if (negativeOne != null)
					{
						BindingRestrictions overflowRestrictions;
						if (left.Value.Equals(lhsMax.Value) && right.Value.Equals(negativeOne.Value))
						{
							result = Constants.VelocityUnresolvableResult;
							overflowRestrictions = BindingRestrictions.GetExpressionRestriction(
									Expression.AndAlso(
										Expression.Equal(left.Expression, lhsMax),
										Expression.Equal(right.Expression, negativeOne)
									)
								);
						}
						else
						{
							overflowRestrictions = BindingRestrictions.GetExpressionRestriction(
									Expression.OrElse(
										Expression.NotEqual(lhsMax, VelocityExpressions.ConvertIfNeeded(left.Expression, lhsMax.Type)),
										Expression.NotEqual(negativeOne, VelocityExpressions.ConvertIfNeeded(right.Expression, lhsMax.Type))
									)
								);
						}
						restrictions = restrictions.Merge(overflowRestrictions);
					}
				}

			}

			if (result == null)
			{
				var targetExpr = VelocityExpressions.ConvertIfNeeded(left, op.Parameters[0]);
				var argExpr = VelocityExpressions.ConvertIfNeeded(right, op.Parameters[1]);
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



	}
}
