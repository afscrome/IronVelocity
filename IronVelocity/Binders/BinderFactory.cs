using IronVelocity.Compilation.AST;
using IronVelocity.Reflection;
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
    public class BinderFactory : IBinderFactory
    {
        private readonly IArgumentConverter _argumentConverter;
        private readonly IMemberResolver _memberResolver;
        private readonly IMethodResolver _methodResolver;

        private readonly ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder>();
        private readonly ConcurrentDictionary<string, GetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, GetMemberBinder>();
        private readonly ConcurrentDictionary<string, SetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, SetMemberBinder>();
        private readonly ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder> _mathsBinders = new ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder>();
        private readonly ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder> _comparisonBinders = new ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder>();

        public BinderFactory()
        {
            _argumentConverter = new ArgumentConverter();
            _methodResolver = new MethodResolver(new OverloadResolver(_argumentConverter), _argumentConverter);
            _memberResolver = new MemberResolver();
        }

        public BinderFactory(IArgumentConverter argumentConverter, IMemberResolver memberResolver, IMethodResolver methodResolver)
        {
            _argumentConverter = argumentConverter;
            _methodResolver = methodResolver;
            _memberResolver = memberResolver;
        }

        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
        {
            return _invokeMemberBinders.GetOrAdd(
                new Tuple<string, int>(name, argumentCount),
                (key) => { return CreateInvokeMemberBinder(key.Item1, key.Item2); }
            );
        }

        public GetMemberBinder GetGetMemberBinder(string memberName)
            => _getMemberBinders.GetOrAdd(memberName, CreateGetMemberBinder);

        public SetMemberBinder GetSetMemberBinder(string memberName)
            => _setMemberBinders.GetOrAdd(memberName, CreateSetMemberBinder);

        public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
            => _comparisonBinders.GetOrAdd(operation, CreateComparisonOperationBinder);

        public VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation operation)
            => _mathsBinders.GetOrAdd(operation, CreateMathematicalOperationBinder);


        protected virtual InvokeMemberBinder CreateInvokeMemberBinder(string name, int argumentCount)
            => new VelocityInvokeMemberBinder(name, new CallInfo(argumentCount), _methodResolver);

        protected virtual GetMemberBinder CreateGetMemberBinder(string memberName)
            => new VelocityGetMemberBinder(memberName, _memberResolver);

        protected virtual SetMemberBinder CreateSetMemberBinder(string memberName)
            => new VelocitySetMemberBinder(memberName, _memberResolver);

        protected virtual VelocityMathematicalOperationBinder CreateMathematicalOperationBinder(MathematicalOperation operation)
            => new VelocityMathematicalOperationBinder(operation, _argumentConverter);

        protected virtual VelocityComparisonOperationBinder CreateComparisonOperationBinder(ComparisonOperation operation)
            => new VelocityComparisonOperationBinder(operation, _argumentConverter);

    }

    public interface IBinderFactory
    {
        InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount);
        GetMemberBinder GetGetMemberBinder(string memberName);
        SetMemberBinder GetSetMemberBinder(string memberName);
        VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation);
        VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation type);
    }
}
