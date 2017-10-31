using System;
using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class ElementEndTagSyntax : SyntaxBase
	{
		ElementEndTagSyntax(LessThanToken lessThan, SlashToken slash, NameToken name, GreaterThanToken greaterThan)
		{
			LessThan = lessThan ?? throw new ArgumentNullException(nameof(lessThan));
			Slash = slash ?? throw new ArgumentNullException(nameof(slash));
			Name = name ?? throw new ArgumentNullException(nameof(name));
			GreaterThan = greaterThan ?? throw new ArgumentNullException(nameof(greaterThan));
		}

		public static ElementEndTagSyntax Create(
			LessThanToken lessThan,
			SlashToken slash,
			NameToken name,
			GreaterThanToken greaterThan)
		{
			return new ElementEndTagSyntax(lessThan, slash, name, greaterThan);
		}

		[NodeChild(0)]
		public LessThanToken LessThan { get; }

		[NodeChild(1)]
		public SlashToken Slash { get; }

		[NodeChild(2)]
		public NameToken Name { get; }

		[NodeChild(3)]
		public GreaterThanToken GreaterThan { get; }

		public override int FullSpan => LessThan.FullSpan + Slash.FullSpan + Name.FullSpan + GreaterThan.FullSpan;

		public override TriviaSyntax LeadingTrivia => LessThan.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => GreaterThan.TrailingTrivia;

		public override void Write(TextWriter writer)
		{
			LessThan.Write(writer);
			Slash.Write(writer);
			Name.Write(writer);
			GreaterThan.Write(writer);
		}

		public ElementEndTagSyntax With(NameToken name)
		{
			if (name == null || name.Equals(Name))
				return this;

			return new ElementEndTagSyntax(LessThan, Slash, name, GreaterThan);
		}
	}
}