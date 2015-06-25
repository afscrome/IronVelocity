using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    /// <remarks>
    /// Reusing CallSiteBinder instances across multiple similar CallSites improves performance
    /// by reducing the amount of repeated compilations / JIT.
    /// 
    /// This is due to the L2 cache on the CallSite.
    /// </remarks>
    public class BinderHelper : IBinderHelper
    {
        static BinderHelper()
        {
            Instance = new BinderHelper();
        }

        public static BinderHelper Instance { get; set; }

        private readonly ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder>();
        private readonly ConcurrentDictionary<string, GetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, GetMemberBinder>();
        private readonly ConcurrentDictionary<string, SetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, SetMemberBinder>();
        private readonly ConcurrentDictionary<ExpressionType, VelocityMathematicalOperationBinder> _mathsBinders = new ConcurrentDictionary<ExpressionType, VelocityMathematicalOperationBinder>();
        private readonly ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder> _comparisonBinders = new ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder>();

        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
        {
            return _invokeMemberBinders.GetOrAdd(
                new Tuple<string, int>(name, argumentCount),
                (key) => { return CreateInvokeMemberBinder(key.Item1, key.Item2); }
            );
        }

        public GetMemberBinder GetGetMemberBinder(string memberName)
        {
            return _getMemberBinders.GetOrAdd(memberName, CreateGetMemberBinder);
        }

        public SetMemberBinder GetSetMemberBinder(string memberName)
        {
            return _setMemberBinders.GetOrAdd(memberName, CreateSetMemberBinder);
        }

        public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
        {
            return _comparisonBinders.GetOrAdd(operation, CreateComparisonOperationBinder);
        }

        public VelocityMathematicalOperationBinder GetMathematicalOperationBinder(ExpressionType type)
        {
            return _mathsBinders.GetOrAdd(type, CreateMathematicalOperationBinder);
        }


        protected virtual InvokeMemberBinder CreateInvokeMemberBinder(string name, int argumentCount)
        {
            return new VelocityInvokeMemberBinder(name, new CallInfo(argumentCount));
        }

        protected virtual GetMemberBinder CreateGetMemberBinder(string memberName)
        {
            return new VelocityGetMemberBinder(memberName);
        }

        protected virtual SetMemberBinder CreateSetMemberBinder(string memberName)
        {
            return new VelocitySetMemberBinder(memberName);
        }

        protected virtual VelocityMathematicalOperationBinder CreateMathematicalOperationBinder(ExpressionType type)
        {
            return new VelocityMathematicalOperationBinder(type);
        }

        protected virtual VelocityComparisonOperationBinder CreateComparisonOperationBinder(ComparisonOperation operation)
        {
            return new VelocityComparisonOperationBinder(operation);
        }

    }

    public interface IBinderHelper
    {
        InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount);
        GetMemberBinder GetGetMemberBinder(string memberName);
        SetMemberBinder GetSetMemberBinder(string memberName);
        VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation);
        VelocityMathematicalOperationBinder GetMathematicalOperationBinder(ExpressionType type);
    }
}
