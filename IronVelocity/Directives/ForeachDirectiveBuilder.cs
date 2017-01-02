using IronVelocity.Compilation.AST;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace IronVelocity.Directives
{
    public class ForeachDirectiveBuilder : CustomDirectiveBuilder
    {
		private static readonly ImmutableDictionary<ForeachSection, CustomDirectiveBuilder> _builders;

		static ForeachDirectiveBuilder()
		{
			var builder = ImmutableDictionary.CreateBuilder<ForeachSection, CustomDirectiveBuilder>();

			foreach(ForeachSection section in Enum.GetValues(typeof(ForeachSection)))
			{
				builder[section] = new ForeachSeperatorBuilder(section);
			}
			_builders = builder.ToImmutable();
		}

        public override bool IsBlockDirective => true;
        public override string Name => "foreach";

        public override Expression Build(IImmutableList<Expression> arguments, Expression body)
            => new ForeachDirective(arguments[0], arguments[2], body);

		public override CustomDirectiveBuilder NestedBuilder(string name)
		{
			ForeachSection section;
			if (Enum.TryParse(name, true, out section))
			{
				return _builders[section];
			}
			return null;
		}

	}

	public class ForeachSeperatorBuilder : CustomDirectiveBuilder
	{
		private readonly ForeachSeperator _seperator;

		public ForeachSeperatorBuilder(ForeachSection section)
		{
			_seperator = new ForeachSeperator(section);
		}

		public override bool IsBlockDirective => false;
		public override string Name => _seperator.Section.ToString();

		public override Expression Build(IImmutableList<Expression> arguments, Expression body)
			=> _seperator;
	}

	public class ForeachSeperator : Directive
	{
		public ForeachSection Section { get; }

		public ForeachSeperator(ForeachSection section)
		{
			Section = section;
		}

		public override Expression Reduce()
		{
			throw new NotSupportedException();
		}
	}
}
