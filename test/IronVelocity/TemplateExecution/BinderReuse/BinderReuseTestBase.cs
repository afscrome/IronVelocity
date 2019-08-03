using IronVelocity.Binders;
using IronVelocity.Reflection;
using System.Dynamic;
using System;
using System.Threading;

namespace IronVelocity.Tests.TemplateExecution.BinderReuse
{
    public class BinderReuseTestBase : TemplateExeuctionBase
    {
        private static readonly ThreadLocal<int> _callSiteBindCount = new ThreadLocal<int>();

        //With globals, binders may not be used, so only test in AsProvided Mode
        protected BinderReuseTestBase() : base(StaticTypingMode.AsProvided) { }

        public static int CallSiteBindCount => _callSiteBindCount.Value;

        protected override IBinderFactory CreateBinderFactory()
            => new ReusableBinderFactory(new DuplicateBinderDetectionBinderFactory());

        private class DuplicateBinderDetectionBinderFactory : IBinderFactory
        {

            public DuplicateBinderDetectionBinderFactory()
            {
                _callSiteBindCount.Value = 0;
            }

            public GetMemberBinder GetGetMemberBinder(string memberName) => new DupDetectionGetMemberBinder(memberName);
            public SetMemberBinder GetSetMemberBinder(string memberName) => new DupDetectionSetMemberBinder(memberName);
            public InvokeMemberBinder GetInvokeMemberBinder(string name, int argumentCount) => new DupDetectionInvokeMemberBinder(name, new CallInfo(argumentCount));

            public GetIndexBinder GetGetIndexBinder(int argumentCount)
            {
                throw new NotImplementedException();
            }

            public SetIndexBinder GetSetIndexBinder(int argumentCount)
            {
                throw new NotImplementedException();
            }

            public BinaryOperationBinder GetBinaryOperationBinder(VelocityOperator op)
            {
                throw new NotImplementedException();
            }

            private class DupDetectionGetMemberBinder : VelocityGetMemberBinder
            {
                public DupDetectionGetMemberBinder(string name)
                    : base(name, new MemberResolver())
                {
                }


                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                {
                    _callSiteBindCount.Value++;
                    return base.FallbackGetMember(target, errorSuggestion);
                }
            }

            private class DupDetectionSetMemberBinder : VelocitySetMemberBinder
            {
                public DupDetectionSetMemberBinder(string name)
                    : base(name, new MemberResolver())
                {
                }


                public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
                {
                    _callSiteBindCount.Value++;
                    return base.FallbackSetMember(target, value, errorSuggestion);
                }
            }

            private class DupDetectionInvokeMemberBinder : VelocityInvokeMemberBinder
            {
                public DupDetectionInvokeMemberBinder(string name, CallInfo callInfo)
                    : base(name, callInfo, new MethodResolver(new OverloadResolver(new ArgumentConverter())))
                {
                }


                public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
                {
                    _callSiteBindCount.Value++;
                    return base.FallbackInvokeMember(target, args, errorSuggestion);
                }
            }

        }

    }
}
