using IronVelocity.Reflection;
using System;
using System.Collections.Concurrent;
using System.Dynamic;

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
        private readonly IIndexResolver _indexResolver;
        private readonly IMethodResolver _methodResolver;

        private readonly ConcurrentDictionary<string, GetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, GetMemberBinder>();
        private readonly ConcurrentDictionary<string, SetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, SetMemberBinder>();
        private readonly ConcurrentDictionary<int, GetIndexBinder> _getIndexBinders = new ConcurrentDictionary<int, GetIndexBinder>();
        private readonly ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder>();
        private readonly ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder> _mathsBinders = new ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder>();
        private readonly ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder> _comparisonBinders = new ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder>();

        public BinderFactory()
        {
            _argumentConverter = new ArgumentConverter();
            var overloadResolver = new OverloadResolver(_argumentConverter);

            _memberResolver = new MemberResolver();
            _indexResolver = new IndexResolver(overloadResolver);
            _methodResolver = new MethodResolver(overloadResolver, _argumentConverter);
        }

        public BinderFactory(IArgumentConverter argumentConverter, IMemberResolver memberResolver, IIndexResolver indexResolver, IMethodResolver methodResolver)
        {
            _argumentConverter = argumentConverter;
            _memberResolver = memberResolver;
            _indexResolver = indexResolver;
            _methodResolver = methodResolver;
        }

        public GetMemberBinder GetGetMemberBinder(string memberName)
            => _getMemberBinders.GetOrAdd(memberName, CreateGetMemberBinder);

        public SetMemberBinder GetSetMemberBinder(string memberName)
            => _setMemberBinders.GetOrAdd(memberName, CreateSetMemberBinder);

        public GetIndexBinder GetGetIndexBinder(int argumentCount)
            => _getIndexBinders.GetOrAdd(argumentCount, CreateGetIndexBinder);


        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
        {
            return _invokeMemberBinders.GetOrAdd(
                new Tuple<string, int>(name, argumentCount),
                (key) => { return CreateInvokeMemberBinder(key.Item1, key.Item2); }
            );
        }

        public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
            => _comparisonBinders.GetOrAdd(operation, CreateComparisonOperationBinder);

        public VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation operation)
            => _mathsBinders.GetOrAdd(operation, CreateMathematicalOperationBinder);


        protected virtual GetMemberBinder CreateGetMemberBinder(string memberName)
            => new VelocityGetMemberBinder(memberName, _memberResolver);

        protected virtual SetMemberBinder CreateSetMemberBinder(string memberName)
            => new VelocitySetMemberBinder(memberName, _memberResolver);

        protected virtual GetIndexBinder CreateGetIndexBinder(int argumentCount)
            => new VelocityGetIndexBinder(argumentCount, _indexResolver);

        protected virtual InvokeMemberBinder CreateInvokeMemberBinder(string name, int argumentCount)
            => new VelocityInvokeMemberBinder(name, new CallInfo(argumentCount), _methodResolver);

        protected virtual VelocityMathematicalOperationBinder CreateMathematicalOperationBinder(MathematicalOperation operation)
            => new VelocityMathematicalOperationBinder(operation, _argumentConverter);

        protected virtual VelocityComparisonOperationBinder CreateComparisonOperationBinder(ComparisonOperation operation)
            => new VelocityComparisonOperationBinder(operation, _argumentConverter);

    }

    public interface IBinderFactory
    {
        GetMemberBinder GetGetMemberBinder(string memberName);
        SetMemberBinder GetSetMemberBinder(string memberName);
        GetIndexBinder GetGetIndexBinder(int argumentCount);
        InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount);
        VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation);
        VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation type);
    }
}
