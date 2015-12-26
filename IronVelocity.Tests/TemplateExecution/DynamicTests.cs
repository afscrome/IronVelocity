using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void ShouldInvokeOnCustomDynamic()
        {
            var dynamic = new CustomDynamic();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("$dynamic.Foo('hello')", context);

            Assert.That(execution.Output, Is.EqualTo("Invoke Foo: (hello)"));
        }

        [Test]
        public void ShouldGetMemberOnCustomDynamic()
        {
            dynamic dynamic = new CustomDynamic();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("$dynamic.Fizz", context);

            Assert.That(execution.Output, Is.EqualTo("GetMember: Fizz"));
        }

        [Test]
        public void ShouldSetMemberOnCustomDynamic()
        {
            var dynamic = new CustomDynamic();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("#set($dynamic.Bar = 7.45)", context);

            Assert.That(dynamic.SetValues.Keys, Contains.Item("Bar"));
            Assert.That(dynamic.SetValues["Bar"], Is.EqualTo(7.45f));
        }


        [Test]
        public void ShouldBinaryOnCustomDynamicForMathematicalExpression()
        {
            var dynamic = new CustomDynamic();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("#set($result = $dynamic + 1)", context);

            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Binary Add: 1"));
        }


        [Test]
        public void ShouldBinaryOnCustomDynamicForRelationalExpression()
        {
            var dynamic = new CustomDynamic();
            var context = new { Dynamic = dynamic };

            var execution = ExecuteTemplate("#set($result = ($dynamic > 472))", context);

            var result = execution.Context["result"];
            Assert.That(result, Is.EqualTo("Binary GreaterThan: 472"));
        }



        public class CustomDynamic : IDynamicMetaObjectProvider
        {
            public IDictionary<string, object> SetValues { get; } = new Dictionary<string, object>();

            public DynamicMetaObject GetMetaObject(Expression parameter)
            {
                return new CustomDynamicMetaObject(parameter, this);
            }

            private class CustomDynamicMetaObject : DynamicMetaObject
            {
                private readonly CustomDynamic _customDynamic;

                public CustomDynamicMetaObject(Expression expression, CustomDynamic value)
                    : base(expression, BindingRestrictions.Empty, value)
                {
                    _customDynamic = value;
                }

                public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
                    => ConstantDynamicMetaObject($"GetMember: {binder.Name}");

                public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
                    => ConstantDynamicMetaObject($"Invoke {binder.Name}: ({string.Join(", ", args.Select(x => x.Value))})");

                public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
                    => ConstantDynamicMetaObject($"Binary {binder.Operation}: {arg.Value}");


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

        public class MyDynamicObject : DynamicObject
        {
            public override DynamicMetaObject GetMetaObject(Expression parameter)
            {
                return base.GetMetaObject(parameter);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = $"GET {binder.Name}";
                return true;
            }

            public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
            {
                result = $"{binder.Operation} + arg";
                return true;
            }

        }
    }
}
