﻿using System;
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
    public class ReusableBinderFactory : IBinderFactory
    {
        private readonly ConcurrentDictionary<string, GetMemberBinder> _getMemberBinders = new ConcurrentDictionary<string, GetMemberBinder>();
        private readonly ConcurrentDictionary<string, SetMemberBinder> _setMemberBinders = new ConcurrentDictionary<string, SetMemberBinder>();
        private readonly ConcurrentDictionary<int, GetIndexBinder> _getIndexBinders = new ConcurrentDictionary<int, GetIndexBinder>();
        private readonly ConcurrentDictionary<int, SetIndexBinder> _setIndexBinders = new ConcurrentDictionary<int, SetIndexBinder>();
        private readonly ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder> _invokeMemberBinders = new ConcurrentDictionary<Tuple<string, int>, InvokeMemberBinder>();
        private readonly ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder> _mathsBinders = new ConcurrentDictionary<MathematicalOperation, VelocityMathematicalOperationBinder>();
        private readonly ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder> _comparisonBinders = new ConcurrentDictionary<ComparisonOperation, VelocityComparisonOperationBinder>();

        private readonly IBinderFactory _rawBinderFactory;
        public ReusableBinderFactory(IBinderFactory factory)
        {
            _rawBinderFactory = factory;
        }

        public GetMemberBinder GetGetMemberBinder(string memberName)
            => _getMemberBinders.GetOrAdd(memberName, _rawBinderFactory.GetGetMemberBinder);

        public SetMemberBinder GetSetMemberBinder(string memberName)
            => _setMemberBinders.GetOrAdd(memberName, _rawBinderFactory.GetSetMemberBinder);

        public GetIndexBinder GetGetIndexBinder(int argumentCount)
            => _getIndexBinders.GetOrAdd(argumentCount, _rawBinderFactory.GetGetIndexBinder);
        public SetIndexBinder GetSetIndexBinder(int argumentCount)
            => _setIndexBinders.GetOrAdd(argumentCount, _rawBinderFactory.GetSetIndexBinder);


        public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount)
        {
            return _invokeMemberBinders.GetOrAdd(
                new Tuple<string, int>(name, argumentCount),
                (key) => { return _rawBinderFactory.GetInvokeMemberBinder(key.Item1, key.Item2); }
            );
        }

        public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
            => _comparisonBinders.GetOrAdd(operation, _rawBinderFactory.GetComparisonOperationBinder);

        public VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation operation)
            => _mathsBinders.GetOrAdd(operation, _rawBinderFactory.GetMathematicalOperationBinder);
    }

}
