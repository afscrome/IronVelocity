using IronVelocity.Compilation;
using System.IO;
using System.Linq.Expressions;

namespace IronVelocity
{
    public interface IParser
    {
        Expression<VelocityTemplateMethod> Parse(string input, string name);
        Expression<VelocityTemplateMethod> Parse(Stream input, string name);
    }
}
