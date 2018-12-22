using System;
using System.Linq;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class FunctionMemberData<T>
		where T: MemberInfo
    {
        public T FunctionMember { get; }
        public Type[] Parameters { get; }
		public bool HasParamsArray { get; }

		public FunctionMemberData(T function, params Type[] argumentTypes)
		{
			FunctionMember = function;
			Parameters = argumentTypes;
		}

		[Obsolete]
		public FunctionMemberData(T function, ParameterInfo[] parameters)
			: this(function, parameters.Select(x => x.ParameterType).ToArray())
		{
			HasParamsArray = parameters.LastOrDefault()?.GetCustomAttributes<ParamArrayAttribute>().Any() ?? false;
		}

	}
}
