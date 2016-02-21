using System;
using System.Linq;
using System.Reflection;

namespace IronVelocity.Reflection
{
    public class OverloadResolutionData<T>
        where T : MemberInfo
    {
        public OverloadResolutionData(ApplicableForm applicableForm, FunctionMemberData<T> functionMember)
        {
            ApplicableForm = applicableForm;
            FunctionMember = functionMember.FunctionMember;
            Parameters = functionMember.Parameters;
        }

        public ApplicableForm ApplicableForm { get; }
        public T FunctionMember { get; }
        public ParameterInfo[] Parameters { get; }

        public Type GetExpandedParameterType(int index)
        {
            if (ApplicableForm == ApplicableForm.Normal || index < Parameters.Length - 1)
                return Parameters[index].ParameterType;

            return Parameters.Last().ParameterType.GetElementType();
        }

    }
}
