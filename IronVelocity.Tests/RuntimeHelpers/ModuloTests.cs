﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronVelocity.RuntimeHelpers;

namespace IronVelocity.Tests.RuntimeHelpers
{
    public class ModuloTests
    {
        [TestCase(10, 3, 1, TestName = "Modulo Positive Integer")]
        [TestCase(-10, -3, -1, TestName = "Modulo Negative Integer")]
        [TestCase(14, -6, 2, TestName = "Modulo Mixed Integers")]
        [TestCase(null, 5, null, TestName = "Modulo Null Left")]
        [TestCase(2, null, null, TestName = "Modulo Null Right")]
        [TestCase(null, null, null, TestName = "Modulo Null Both")]
        [TestCase(6f, 4, 2f, TestName = "Modulo Integer Float")]
        [TestCase(2, 1.5f, 0.5f, TestName = "Modulo Float Integer")]
        [TestCase(5, 0, null, TestName = "Modulo By Integer 0")]
        [TestCase(1.5f, 0f, float.NaN, TestName = "Modulo float by positive 0")]
        [TestCase(1.5f, -0f, float.NaN, TestName = "Modulo float by negative 0")]
        public void BasicTest(object left, object right, object expected)
        {
            var result = Operators.Modulo(left, right);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ModuloOperatorOverload()
        {
            var left = new OverloadedModulo(6);
            var right = new OverloadedModulo(5);
            var result = Operators.Modulo(left, right);

            Assert.IsInstanceOf<OverloadedModulo>(result);
            Assert.AreEqual(1, ((OverloadedModulo)result).Value);
        }


        public class OverloadedModulo
        {
            public int Value { get; private set; }
            public OverloadedModulo(int value)
            {
                Value = value;
            }

            public static OverloadedModulo operator %(OverloadedModulo left, OverloadedModulo right)
            {
                return new OverloadedModulo(left.Value % right.Value);
            }
        }
    }
}
