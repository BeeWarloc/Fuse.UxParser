using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public class ElementStartTagSyntax : SyntaxBase
	{
		public ElementStartTagSyntax(
			LessThanToken lessThan,
			NameToken name,
			IImmutableList<AttributeSyntaxBase> attributes,
			GreaterThanToken greaterThan)
		{
			LessThan = lessThan;
			Name = name;
			Attributes = attributes;
			GreaterThan = greaterThan;
		}

		[NodeChild(0)]
		public LessThanToken LessThan { get; }

		[NodeChild(1)]
		public NameToken Name { get; }

		[NodeChild(2)]
		public IImmutableList<AttributeSyntaxBase> Attributes { get; }

		[NodeChild(3)]
		public GreaterThanToken GreaterThan { get; }

		public override int FullSpan =>
			LessThan.FullSpan + Name.FullSpan + Attributes.Sum(x => x.FullSpan) + GreaterThan.FullSpan;

		public override TriviaSyntax LeadingTrivia => LessThan.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => GreaterThan.TrailingTrivia;

		public override void Write(TextWriter writer)
		{
			LessThan.Write(writer);
			Name.Write(writer);
			foreach (var attribute in Attributes)
				attribute.Write(writer);
			GreaterThan.Write(writer);
		}

		public ElementStartTagSyntax With(NameToken name = null, IImmutableList<AttributeSyntaxBase> attributes = null)
		{
			name = name ?? Name;
			attributes = attributes ?? Attributes;
			if (attributes.Equals(Attributes) && name.Equals(Name))
				return this;
			return new ElementStartTagSyntax(LessThan, name, attributes, GreaterThan);
		}
	}
}