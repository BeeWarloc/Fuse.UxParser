using System;
using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class TextSyntax : NodeSyntax
	{
		TextSyntax(EncodedTextToken value)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public static TextSyntax Create(EncodedTextToken value)
		{
			return new TextSyntax(value);
		}

		[NodeChild(0)]
		public EncodedTextToken Value { get; }

		public override int FullSpan => Value.FullSpan;

		public override TriviaSyntax LeadingTrivia => Value.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => Value.TrailingTrivia;

		public NodeSyntax With(EncodedTextToken value)
		{
			if (value == null || value.Equals(Value))
				return this;

			return new TextSyntax(value);
		}

		public override void Write(TextWriter writer)
		{
			Value.Write(writer);
		}
	}
}