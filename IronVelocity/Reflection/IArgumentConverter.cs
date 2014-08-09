using System;

namespace IronVelocity.Reflection
{
    public interface IArgumentConverter
    {
        bool CanBeConverted(Type from, Type to);
    }
}
