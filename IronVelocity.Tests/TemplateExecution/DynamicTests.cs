using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace IronVelocity.Tests.TemplateExecution
{
    [TestFixture(StaticTypingMode.AsProvided)]
    [TestFixture(StaticTypingMode.PromoteContextToGlobals)]
    public class DynamicTests : TemplateExeuctionBase
    {
        public DynamicTests(StaticTypingMode mode) : base(mode)
        {
        }

        [Test]
        public void ShouldInvokeInvokeMemberOperationOnCustomDynamic()
        {
            var dynamic = new TestDynamicMetaObjectProvider();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("$dynamic.Foo('hello')", context);

            Assert.That(execution.Output, Is.EqualTo("DMO InvokeMember Foo: (hello)"));
        }

        [Test]
        public void ShouldInvokeGetMemberOperationOnCustomDynamic()
        {
            dynamic dynamic = new TestDynamicMetaObjectProvider();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("$dynamic.Fizz", context);

            Assert.That(execution.Output, Is.EqualTo("DMO GetMember: Fizz"));
        }

        [Test]
        public void ShouldInvokeSetMemberOperationOnCustomDynamic()
        {
            var dynamic = new TestDynamicMetaObjectProvider();
            var context = new { Dynamic = dynamic };

            ExecuteTemplate("#set($dynamic.Bar = 7.45)", context);

            Assert.That(dynamic.SetValues.Keys, Contains.Item("Bar"));
            Assert.That(dynamic.SetValues["Bar"], Is.EqualTo(7.45f));
        }


		[TestCase("+", "Add")]
		[TestCase("-", "Subtract")]
		[TestCase("*", "Multiply")]
		[TestCase("/", "Divide")]
		[TestCase("%", "Modulo")]
		[TestCase("==", "Equal")]
		[TestCase("!=", "NotEqual")]
		[TestCase(">", "GreaterThan")]
		[TestCase(">=", "GreaterThanOrEqual")]
		[TestCase("<", "LessThan")]
		[TestCase("<=", "LessThanOrEqual")]
		public void ShouldInvokeBinaryOperationOnCustomDynamicForBinaryExpression(string op, string opName)
        {
            var dynamic = new TestDynamicMetaObjectProvider();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate($"#set($result = $dynamic {op} 1)", context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            Assert.That(execution.Context["result"], Is.EqualTo($"DMO Binary {opName}: 1"));
        }


        [Test]
        public void ShouldInvokeGetMemberOnDynamicObject()
        {
            var dynamic = new CustomDynamicObject();

            var context = new { Dynamic = dynamic };
            var execution = ExecuteTemplate("$dynamic.Prop", context);

            Assert.That(execution.Output, Is.EqualTo("DO GetMember: Prop"));
        }

        [Test]
        public void ShouldInvokeInvokeMemberOnDynamicObject()
        {
            var dynamic = new CustomDynamicObject();

            var context = new { Dynamic = dynamic };
            var execution = ExecuteTemplate("$dynamic.Double(87)", context);

            Assert.That(execution.Output, Is.EqualTo("DO InvokeMember Double: (87)"));
        }

        [Test]
        public void ShouldInvokeSetMemberOnDynamicObject()
        {
            var dynamic = new CustomDynamicObject();

            var context = new { Dynamic = dynamic };
            ExecuteTemplate("#set($dynamic.SetMe = true)", context);

            Assert.That(dynamic.SetMembers.Keys, Contains.Item("SetMe"));
            Assert.That(dynamic.SetMembers["SetMe"], Is.EqualTo(true));
        }

		[TestCase("+", "Add")]
		[TestCase("-", "Subtract")]
		[TestCase("*", "Multiply")]
		[TestCase("/", "Divide")]
		[TestCase("%", "Modulo")]
		[TestCase("==", "Equal")]
		[TestCase("!=", "NotEqual")]
		[TestCase(">", "GreaterThan")]
		[TestCase(">=", "GreaterThanOrEqual")]
		[TestCase("<", "LessThan")]
		[TestCase("<=", "LessThanOrEqual")]
		public void ShouldInvokeBinaryOnDynamicObjectForBinaryExpression(string op, string opName)
        {
            var dynamic = new CustomDynamicObject();

            var context = new { Dynamic = dynamic };
            var execution = ExecuteTemplate($"#set($result = $dynamic {op} false)", context);

            Assert.That(execution.Context.Keys, Contains.Item("result"));
            Assert.That(execution.Context["result"], Is.EqualTo($"DO Binary {opName}: False"));
        }


        public class CustomDynamicObject : DynamicObject
        {
            public IDictionary<string, object> SetMembers { get; } = new Dictionary<string, object>();

			public override DynamicMetaObject GetMetaObject(Expression parameter)
			{
				return base.GetMetaObject(parameter);
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = $"DO GetMember: {binder.Name}";
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                SetMembers[binder.Name] = value;
                return true;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                result = $"DO InvokeMember {binder.Name}: ({string.Join(", ", args)})";
                return true;
            }

            public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
            {
                result = $"DO Binary {binder.Operation}: {arg}";
                return true;
            }

        }


        public class TestDynamicMetaObjectProvider : IDynamicMetaObjectProvider
        {
            public IDictionary<string, object> SetValues { get; } = new Dictionary<string, object>();

            public DynamicMetaObject GetMetaObject(Expression parameter)
            {
                return new CustomDynamicMetaObject(parameter, this);
            }

            private class CustomDynamicMetaObject : DynamicMetaObject
            {
                private readonly TestDynamicMetaObjectProvider _customDynamic;

                public CustomDynamicMetaObject(Expression expression, TestDynamicMetaObjectProvider value)
                    : base(expression, BindingRestrictions.Empty, value)
                {
                    _customDynamic = value;
                }

                public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
                    => ConstantDynamicMetaObject($"DMO GetMember: {binder.Name}");

                public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
                    => ConstantDynamicMetaObject($"DMO InvokeMember {binder.Name}: ({string.Join(", ", args.Select(x => x.Value))})");

                public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
                    => ConstantDynamicMetaObject($"DMO Binary {binder.Operation}: {arg.Value}");

                public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
                {
                    _customDynamic.SetValues[binder.Name] = value.Value;
                    return ConstantDynamicMetaObject($"Set {binder.Name}: {value.Value}");
                }

                private static DynamicMetaObject ConstantDynamicMetaObject(string output)
                {
                    var expr = Expression.Constant(output);
                    var restriction = BindingRestrictions.GetExpressionRestriction(Constants.True);
                    return new DynamicMetaObject(expr, restriction, output);
                }

            }

        }
    }
}
