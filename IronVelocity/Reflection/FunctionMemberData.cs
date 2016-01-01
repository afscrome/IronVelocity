using System.Reflection;

namespace IronVelocity.Reflection
{
    public class FunctionMemberData<T>
    {
        public T FunctionMember { get; }
        public ParameterInfo[] Parameters { get; }

        public FunctionMemberData(T function, ParameterInfo[] parameters)
        {
            FunctionMember = function;
            Parameters = parameters;
        }
    }
}
