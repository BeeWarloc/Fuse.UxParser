﻿namespace Fuse.UxParser.Syntax
{
	public class NameToken : SyntaxToken
	{
		public NameToken(TriviaSyntax leadingTrivia, string text, TriviaSyntax trailingTrivia)
			: base(leadingTrivia, trailingTrivia)
		{
			Text = text;
		}

		public static NameToken Missing { get; } = new NameToken(TriviaSyntax.Empty, string.Empty, TriviaSyntax.Empty);

		public override string Text { get; }
		public override bool IsMissing => Text.Length == 0;

		public NameToken With(string text)
		{
			// TODO: Validation here or ignore errors?
			if (text == null || text == Text)
				return this;
			return new NameToken(LeadingTrivia, text, TrailingTrivia);
		}

		public NameToken WithTrivia(TriviaSyntax? leadingTrivia = null, TriviaSyntax? trailingTrivia = null)
		{
			var leading = leadingTrivia ?? LeadingTrivia;
			var trailing = trailingTrivia ?? TrailingTrivia;

			if (leading == LeadingTrivia && trailing == TrailingTrivia)
				return this;

			return new NameToken(leading, Text, trailing);
		}
	}
}