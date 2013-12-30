using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests;

namespace IronVelocity.Tests
{
    public class Playground
    {
        [Test]
        public void atest()
        {
            var context = new Dictionary<string, object>();
            context["x"] = new Test();

            Utility.TestExpectedMarkupGenerated("#set($y = $x.GetCallCount()) $y $y", " 0 0", context);


            context["x"] = new Test();
            Utility.TestExpectedMarkupGenerated("$x.GetCallCount() $x.GetCallCount()", "0 1", context);
            //Utility.TestExpectedMarkupGenerated("$x.CallCount $x.CallCount", "0 1", context);
        }

        public struct Test
        {
            private int _callCount;

            public int GetCallCount()
            {
                return _callCount++;
            }

            public int CallCount
            {
                get
                {
                    return _callCount++;
                }
            }
        }


        public class MyEnumerable : IEnumerable
        {
            private readonly object[] _items;
            public MyEnumerable(params object[] items)
            {
                _items = items;
            }

            public IEnumerator GetEnumerator()
            {
                return new MyEnumerator();
            }

            private struct MyEnumerator : IEnumerator
            {
                private readonly object[] _items;
                private int _currentIndex;

                public MyEnumerator(params object[] items)
                {
                    _currentIndex = -1;
                    _items = items;
                }


                public object Current
                {
                    get {
                        if (_currentIndex < 0)
                            throw new NotImplementedException();
                        return _items[_currentIndex];
                    }
                }

                public bool MoveNext()
                {
                    if (_currentIndex >= _items.Length)
                        return false;

                    _currentIndex++;
                    return true;
                }

                public void Reset()
                {
                    _currentIndex = -1;
                }
            }

        }
    }
}
