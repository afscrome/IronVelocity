using Antlr4.Runtime;
using IronVelocity.Directives;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IronVelocity.Parser
{
    public partial class VelocityParser
    {
		public IImmutableList<CustomDirectiveBuilder> DirectiveBuilders { get; set; }

		private bool IsBlockDirective()
			=> GetDirectiveBuilder()?.IsBlockDirective == true;

		private bool IsSingleLineDirective()
			=> GetDirectiveBuilder()?.IsBlockDirective == false;

		private bool IsUnknownDirective()
			=> GetDirectiveBuilder() == null;

		private CustomDirectiveBuilder GetDirectiveBuilder() => GetCurrentDirective(RuleContext).LoadDirectiveBuilder(DirectiveBuilders);

		private string GetDirectiveName()
		{
			if (!(RuleContext is DirectiveBodyContext))
				throw new InvalidOperationException("Should be in a DirectiveBodyContext rule context");

			return ((CustomDirectiveContext)RuleContext.parent).DirectiveName().GetText();
		}


		private static CustomDirectiveContext GetCurrentDirective(RuleContext context)
		{
			while(context != null)
			{
				var directiveContext = context as CustomDirectiveContext;
				if (directiveContext != null)
					return directiveContext;

				context = context.Parent;
			}
			return null;
		}

		public partial class CustomDirectiveContext
		{
			public CustomDirectiveBuilder Builder { get; private set; }
			public CustomDirectiveBuilder LoadDirectiveBuilder(IEnumerable<CustomDirectiveBuilder> defaultBuilders)
			{
				if (Builder == null)
				{
					var name = DirectiveName().GetText();

					var parentDirective = GetCurrentDirective(Parent);
					while(parentDirective != null && Builder == null)
					{
						Builder = parentDirective.Builder.NestedBuilder(name);
						parentDirective = GetCurrentDirective(parentDirective.Parent);
					}

					if (Builder == null)
						Builder = defaultBuilders.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
				}
				return Builder;
			}

		}

	}



}
