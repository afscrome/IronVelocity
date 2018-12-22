using System;
using System.Reflection;

namespace IronVelocity.Reflection
{
	public interface IOperatorResolver
	{
		OverloadResolutionData<MethodInfo> Resolve(VelocityOperator operatorType, Type left, Type right);
	}
}
