using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.Reflection
{
	public enum VelocityOperator
	{
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
	}

	public interface IOperatorResolver
	{
		OverloadResolutionData<MethodInfo> Resolve(VelocityOperator operatorType, Type left, Type right);
	}

	public class OperatorResolver : IOperatorResolver
	{
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

		private readonly IOverloadResolver _overloadResolver = new OverloadResolver(new ArgumentConverter());
		private static readonly ImmutableArray<FunctionMemberData<MethodInfo>>
			_builtInAdditionOperators, _builtInSubtractionOperators,
			_builtInMultiplicationOperators, _builtInDivisionOperators, _builtInModulusOperators,
			_builtInEqualityOperators, _builtInInequalityOperators,
			_builtInGreaterThanOperators, _builtInGreaterThanOrEqualOperators,
			_builtInLessThanOperators, _builtInLessThanOrEqualOperators;


		static OperatorResolver()
		{
			var commonBuiltInOperators = ImmutableArray.Create(
				ClrIntrinsic<int, int>(),
				ClrIntrinsic<uint, uint>(),
				ClrIntrinsic<long, long>(),
				ClrIntrinsic<ulong, ulong>(),
				ClrIntrinsic<float, float>(),
				ClrIntrinsic<double, double>()
				);

			//Equality
			var commonEquality = commonBuiltInOperators.Add(ClrIntrinsic<bool, bool>());
			_builtInEqualityOperators = commonEquality.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(EqualityMethodName),
				BuiltInOperator<string, string>(EqualityMethodName),
			});
			_builtInInequalityOperators = commonEquality.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(InequalityMethodName),
				BuiltInOperator<string, string>(InequalityMethodName),
			});

			//Relational
			_builtInGreaterThanOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(GreaterThanMethodName));
			_builtInGreaterThanOrEqualOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(GreaterThanOrEqualMethodName));
			_builtInLessThanOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(LessThanMethodName));
			_builtInLessThanOrEqualOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(LessThanOrEqualMethodName));


			//Maths
			_builtInSubtractionOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(SubtractionMethodName));
			_builtInMultiplicationOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(MultiplicationMethodName));
			_builtInDivisionOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(DivisionMethodName));
			_builtInModulusOperators = commonBuiltInOperators.Add(BuiltInOperator<decimal, decimal>(ModuloMethodName));
			_builtInAdditionOperators = commonBuiltInOperators.AddRange(new[] {
				BuiltInOperator<decimal, decimal>(AdditionMethodName),
				VelocityIntrinsic<string, string>(typeof(string).GetMethod(nameof(String.Concat), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null)),
				VelocityIntrinsic<string, object>(typeof(string).GetMethod(nameof(String.Concat), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null)),
				VelocityIntrinsic<object, string>(typeof(string).GetMethod(nameof(String.Concat), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(object) }, null))
			});
		}

		public OverloadResolutionData<MethodInfo> Resolve(VelocityOperator operatorType, Type left, Type right)
		{
			var candidates = GetCandidateUserDefinedOperators(operatorType, left, right);
			if (!candidates.Any())
				candidates = GetBuiltInOperators(operatorType);

			return _overloadResolver.Resolve(candidates, ImmutableList.Create(left, right));
		}

		private IImmutableList<FunctionMemberData<MethodInfo>> GetCandidateUserDefinedOperators(VelocityOperator operatorType, Type left, Type right)
		{
			var operatorName = GetOperatorName(operatorType);

			// The set of candidate user-defined operators provided by X and Y for the operation operator op(x, y) is determined.
			//The set consists of the union of the candidate operators provided by X and the candidate operators provided by Y
			var candidates = GetCandidateUserDefinedOperatorsForType(operatorName, left);

			//If X and Y are the same type, or if X and Y are derived from a common base type, then shared candidate operators only occur in the combined set once.
			if (left != right)
				candidates = candidates.Union(GetCandidateUserDefinedOperatorsForType(operatorName, right));

			return candidates.Select(x => new FunctionMemberData<MethodInfo>(x, x.GetParameters()))
				.ToImmutableList();
		}


		private IEnumerable<MethodInfo> GetCandidateUserDefinedOperatorsForType(string operatorName, Type type)
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
					break;

				//Otherwise, the set of candidate operators provided by T0 is the set of candidate operators provided by the direct base class of T0
				//or the effective base class of T0 if T0 is a type parameter.
				type = type.BaseType?.GetTypeInfo();
			}

			return ImmutableList<MethodInfo>.Empty;
		}


		private IImmutableList<FunctionMemberData<MethodInfo>> GetBuiltInOperators(VelocityOperator operation)
		{
			switch (operation)
			{
				case VelocityOperator.Add:
					return _builtInAdditionOperators;
				case VelocityOperator.Subtract:
					return _builtInSubtractionOperators;
				case VelocityOperator.Multiply:
					return _builtInMultiplicationOperators;
				case VelocityOperator.Divide:
					return _builtInDivisionOperators;
				case VelocityOperator.Modulo:
					return _builtInModulusOperators;
				case VelocityOperator.Equal:
					return _builtInEqualityOperators;
				case VelocityOperator.NotEqual:
					return _builtInInequalityOperators;
				case VelocityOperator.GreaterThan:
					return _builtInGreaterThanOperators;
				case VelocityOperator.GreaterThanOrEqual:
					return _builtInGreaterThanOrEqualOperators;
				case VelocityOperator.LessThan:
					return _builtInLessThanOperators;
				case VelocityOperator.LessThanOrEqual:
					return _builtInLessThanOrEqualOperators;
				default:
					throw new ArgumentOutOfRangeException(nameof(operation));
			}

		}

		private string GetOperatorName(VelocityOperator operation)
		{
			switch (operation)
			{
				case VelocityOperator.Add:
					return AdditionMethodName;
				case VelocityOperator.Subtract:
					return SubtractionMethodName;
				case VelocityOperator.Multiply:
					return MultiplicationMethodName;
				case VelocityOperator.Divide:
					return DivisionMethodName;
				case VelocityOperator.Modulo:
					return ModuloMethodName;
				case VelocityOperator.Equal:
					return EqualityMethodName;
				case VelocityOperator.NotEqual:
					return InequalityMethodName;
				case VelocityOperator.GreaterThan:
					return GreaterThanMethodName;
				case VelocityOperator.GreaterThanOrEqual:
					return GreaterThanOrEqualMethodName;
				case VelocityOperator.LessThan:
					return LessThanMethodName;
				case VelocityOperator.LessThanOrEqual:
					return LessThanOrEqualMethodName;
				default:
					throw new ArgumentOutOfRangeException(nameof(operation));
			}

		}



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
