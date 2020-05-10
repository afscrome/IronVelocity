using System.Collections.Generic;
using System.Linq;

namespace IronVelocity.CodeAnalysis.Tests
{
    internal static class Helper
    {
        public static IEnumerable<(T left, T right)> GetAllPairs<T>(IEnumerable<T> values)
        {
            return from left in values
                   from right in values
                   select (left, right);
        }
    }
}
