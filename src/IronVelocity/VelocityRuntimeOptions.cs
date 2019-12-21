using IronVelocity.Binders;
using IronVelocity.Directives;
using System.Collections.Generic;

namespace IronVelocity
{
	public class VelocityRuntimeOptions
	{
		public IBinderFactory BinderFactory { get; set; }
		public bool CompileInDebugMode { get; set; }
		public ICollection<CustomDirectiveBuilder> Directives { get; set; }
		public IDictionary<string, object> Globals { get; set; }
		public bool OptimizeConstantTypes { get; set; } = true;
		public bool ReduceWhitespace { get; set; }
	}
}
