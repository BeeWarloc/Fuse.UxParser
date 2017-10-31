using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public class EmptyElementSyntax : ElementSyntaxBase
	{
		EmptyElementSyntax(
			LessThanToken lessThan,
			NameToken name,
			IImmutableList<AttributeSyntaxBase> attributes,
			SlashToken slash,
			GreaterThanToken greaterThan)
		{
			LessThan = lessThan ?? throw new ArgumentNullException(nameof(lessThan));
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
			Slash = slash ?? throw new ArgumentNullException(nameof(slash));
			GreaterThan = greaterThan ?? throw new ArgumentNullException(nameof(greaterThan));
		}

		public static EmptyElementSyntax Create(
			LessThanToken lessThan,
			NameToken name,
			IImmutableList<AttributeSyntaxBase> attributes,
			SlashToken slash,
			GreaterThanToken greaterThan)
		{
			return new EmptyElementSyntax(lessThan, name, attributes, slash, greaterThan);
		}

		[NodeChild(0)]
		public LessThanToken LessThan { get; }


		[NodeChild(1)]
		public override NameToken Name { get; }

		[NodeChild(2)]
		public override IImmutableList<AttributeSyntaxBase> Attributes { get; }

		[NodeChild(3)]
		public SlashToken Slash { get; }

		[NodeChild(4)]
		public GreaterThanToken GreaterThan { get; }

		public override IImmutableList<NodeSyntax> Nodes => ImmutableList<NodeSyntax>.Empty;
		public override bool IsEmpty => true;

		public override int FullSpan =>
			LessThan.FullSpan + Name.FullSpan + Attributes.Sum(x => x.FullSpan) + Slash.FullSpan + GreaterThan.FullSpan;

		public override int DescendantElementCount => 0;

		public override TriviaSyntax LeadingTrivia => LessThan.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => GreaterThan.TrailingTrivia;

		public override ElementSyntaxBase With(
			NameToken name = null,
			IImmutableList<AttributeSyntaxBase> attributes = null,
			IImmutableList<NodeSyntax> nodes = null,
			bool? isEmpty = null)
		{
			name = name ?? Name;
			nodes = nodes ?? Nodes;
			attributes = attributes ?? Attributes;

			if ((isEmpty ?? true) && nodes.Count == 0)
			{
				if (name.Equals(Name) &&
					(attributes.Equals(Attributes) || attributes.SequenceEqual(Attributes)))
					return this;

				return new EmptyElementSyntax(LessThan, name, attributes, Slash, GreaterThan);
			}

			return ElementSyntax.Create(
				ElementStartTagSyntax.Create(LessThan, name, attributes, GreaterThanToken.Default),
				nodes,
				ElementEndTagSyntax.Create(
					LessThanToken.Default,
					SlashToken.Default,
					name,
					GreaterThan));
		}

		public override void Write(TextWriter writer)
		{
			LessThan.Write(writer);
			Name.Write(writer);
			foreach (var attribute in Attributes)
				attribute.Write(writer);
			Slash.Write(writer);
			GreaterThan.Write(writer);
		}
	}
}