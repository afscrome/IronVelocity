using System;

namespace IronVelocity.Reflection
{
    public interface IArgumentConverter
    {
        //[Obsolete("remove", true)]
        bool CanBeConverted(Type from, Type to);
        IExpressionConverter GetConverter(Type from, Type to);
    }
}
