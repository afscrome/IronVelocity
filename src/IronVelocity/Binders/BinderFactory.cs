﻿using IronVelocity.Reflection;
using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronVelocity.Binders
{
    public class BinderFactory : IBinderFactory
    {
        private readonly IMemberResolver _memberResolver;
        private readonly IIndexResolver _indexResolver;
        private readonly IMethodResolver _methodResolver;
		private readonly IOperatorResolver _operatorResolver;

        public BinderFactory()
        {
            var overloadResolver = new OverloadResolver(new ArgumentConverter());

            _memberResolver = new MemberResolver();
            _indexResolver = new IndexResolver(overloadResolver);
            _methodResolver = new MethodResolver(overloadResolver);
			_operatorResolver = new OperatorResolver(overloadResolver);
        }

        public BinderFactory(IMemberResolver memberResolver, IIndexResolver indexResolver, IMethodResolver methodResolver, IOperatorResolver operatorResolver)
        {
            _memberResolver = memberResolver;
            _indexResolver = indexResolver;
            _methodResolver = methodResolver;
			_operatorResolver = operatorResolver;
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

        public BinaryOperationBinder GetBinaryOperationBinder(VelocityOperator type)
            => new VelocityBinaryOperationBinder(type, _operatorResolver);
    }
}
