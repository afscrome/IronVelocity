using IronVelocity.Binders;
using IronVelocity.Reflection;
using NUnit.Framework;
using System.Dynamic;
using System;

namespace IronVelocity.Tests.TemplateExecution.BinderReuse
{
    public class BinderReuseTestBase : TemplateExeuctionBase
    {
        //With globals, binders may not be used, so only test in AsProvided Mode
        protected BinderReuseTestBase() : base(StaticTypingMode.AsProvided) { }

        public int CallSiteBindCount => DuplcateBinderDetectionBinderFactory.CallSiteBindCount;

        protected override IBinderFactory CreateBinderFactory()
            => new ReusableBinderFactory(new DuplcateBinderDetectionBinderFactory());

        private class DuplcateBinderDetectionBinderFactory : IBinderFactory
        {
            public static int CallSiteBindCount { get; set; }
            private readonly ReusableBinderFactory _factory = new ReusableBinderFactory(new BinderFactory());

            public DuplcateBinderDetectionBinderFactory()
            {
                CallSiteBindCount = 0;
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

            public VelocityComparisonOperationBinder GetComparisonOperationBinder(ComparisonOperation operation)
            {
                throw new NotImplementedException();
            }

            public VelocityMathematicalOperationBinder GetMathematicalOperationBinder(MathematicalOperation type)
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
                    CallSiteBindCount++;
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
                    CallSiteBindCount++;
                    return base.FallbackSetMember(target, value, errorSuggestion);
                }
            }

            private class DupDetectionInvokeMemberBinder : VelocityInvokeMemberBinder
            {
                public DupDetectionInvokeMemberBinder(string name, CallInfo callInfo)
                    : base(name, callInfo, new MethodResolver(new OverloadResolver(new ArgumentConverter()), new ArgumentConverter()))
                {
                }


                public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
                {
                    CallSiteBindCount++;
                    return base.FallbackInvokeMember(target, args, errorSuggestion);
                }
            }

            private class DupDetectionComparisonOperationBinder : VelocityComparisonOperationBinder
            {
                public DupDetectionComparisonOperationBinder(ComparisonOperation type)
                    : base(type, new ArgumentConverter())
                {
                }

                public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
                {
                    CallSiteBindCount++;
                    return base.FallbackBinaryOperation(target, arg, errorSuggestion);
                }
            }

        }

    }
}
