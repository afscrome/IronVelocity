using IronVelocity.Binders;
using NUnit.Framework;
using System.Dynamic;

namespace IronVelocity.Tests.Binders.Reuse
{
    public abstract class BinderReuseTestBase
    {
        private BinderHelper _oldHelper;
        public int CallSiteBindCount { get { return DuplicateBinderHelper.CallSiteBindCount; } }

        [SetUp]
        public void SetUp()
        {
            _oldHelper = BinderHelper.Instance;
            BinderHelper.Instance = new DuplicateBinderHelper();
        }

        [TearDown]
        public void TearDown()
        {
            if(_oldHelper != null)
                BinderHelper.Instance = _oldHelper;
        }

        private class DuplicateBinderHelper : BinderHelper
        {
            public static int CallSiteBindCount { get; set; }

            public DuplicateBinderHelper()
            {
                CallSiteBindCount = 0;
            }

            protected override GetMemberBinder CreateGetMemberBinder(string memberName)
            {
                return new DupDetectionGetMemberBinder(memberName);
            }
            protected override SetMemberBinder CreateSetMemberBinder(string memberName)
            {
                return new DupDetectionSetMemberBinder(memberName);
            }

            protected override InvokeMemberBinder CreateInvokeMemberBinder(string name, int argumentCount)
            {
                return new DupDetectionInvokeMemberBinder(name, new CallInfo(argumentCount));
            }

            private class DupDetectionGetMemberBinder : VelocityGetMemberBinder
            {
                public DupDetectionGetMemberBinder(string name)
                    : base(name)
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
                    : base(name)
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
                    : base(name, callInfo)
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
                    : base(type)
                {
                }

                public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
                {
                    CallSiteBindCount++;
                    return base.Bind(target, args);
                }
            }

        }

    }
}
