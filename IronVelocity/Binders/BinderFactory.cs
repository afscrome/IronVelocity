using IronVelocity.Reflection;
using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class BinderFactory : IBinderFactory
    {
        private readonly IArgumentConverter _argumentConverter;
        private readonly IMemberResolver _memberResolver;
        private readonly IIndexResolver _indexResolver;
        private readonly IMethodResolver _methodResolver;

        public BinderFactory()
        {
            _argumentConverter = new ArgumentConverter();
            var overloadResolver = new OverloadResolver(_argumentConverter);

            _memberResolver = new MemberResolver();
            _indexResolver = new IndexResolver(overloadResolver);
            _methodResolver = new MethodResolver(overloadResolver);
        }

        public BinderFactory(IArgumentConverter argumentConverter, IMemberResolver memberResolver, IIndexResolver indexResolver, IMethodResolver methodResolver)
        {
            _argumentConverter = argumentConverter;
            _memberResolver = memberResolver;
            _indexResolver = indexResolver;
            _methodResolver = methodResolver;
        }


        public virtual GetMemberBinder GetGetMemberBinder(string memberName)
           => new VelocityGetMemberBinder(memberName, _memberResolver);

        public SetMemberBinder GetSetMemberBinder(string memberName)
            => new VelocitySetMemberBinder(memberName, _memberResolver);

        public GetIndexBinder GetGetIndexBinder(int argumentCount)
            => new VelocityGetIndexBinder(argumentCount, _indexResolver);

        public SetIndexBinder GetSetIndexBinder(int argumentCount)
            => new VelocitySetIndexBinder(argumentCount, _indexResolver);

        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
            => new VelocityInvokeMemberBinder(name, new CallInfo(argumentCount), _methodResolver);

        public BinaryOperationBinder GetBinaryOperationBinder(ExpressionType type)
            => new VelocityBinaryOperationBinder(type);

        public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
            => new VelocityComparisonOperationBinder(operation, _argumentConverter);
    }
}
