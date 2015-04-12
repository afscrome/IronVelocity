using IronVelocity.Compilation;
using System.Linq.Expressions;

namespace IronVelocity
{
    public interface IParser
    {
        Expression<VelocityTemplateMethod> Parse(string input, string name);
    }
}
