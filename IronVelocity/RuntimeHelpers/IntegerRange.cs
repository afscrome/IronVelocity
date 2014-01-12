using System.Collections.Generic;

namespace IronVelocity.RuntimeHelpers
{
    public static class IntegerRange
    {
        public static List<int> Range(object left, object right)
        {
            if (!(left is int && right is int))
                return null;

            int leftInt = (int)left;
            int rightInt = (int)right;

            int delta = leftInt > rightInt
                ? -1
                : +1;

            int count = delta > 0
                ? rightInt - leftInt
                : leftInt - rightInt;
            count++;

            var result = new List<int>(count);

            int current = leftInt;
            for (int i = 0; i < count; i++)
            {
                result.Add(current);
                current += delta;
            }

            return result;
        }
    }
}
