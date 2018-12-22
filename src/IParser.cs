using IronVelocity.Compilation;
using System.IO;
using System.Linq.Expressions;

namespace IronVelocity
{
    public interface IParser
    {
        Expression<VelocityTemplateMethod> Parse(TextReader input, string name);
    }
}
