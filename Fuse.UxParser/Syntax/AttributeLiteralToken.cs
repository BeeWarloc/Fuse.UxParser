using System;

namespace Fuse.UxParser.Syntax
{
	// TODO: Read 3.3.3 Attribute-Value Normalization in XML spec and see what it tells us
	public class AttributeLiteralToken : SyntaxToken
	{
		readonly string _escapedValue;

		// TODO: Uncertain about caching here. Maybe it's an unnecesarry optimization?
		string _cachedUnescapedValue;


		AttributeLiteralToken(
			TriviaSyntax leadingTrivia,
			string escapedValue,
			TriviaSyntax trailingTrivia,
			AttributeLiteralKind? literalKind,
			string cachedUnescapedValue) :
			this(leadingTrivia, escapedValue, trailingTrivia, literalKind)
		{
			_cachedUnescapedValue = cachedUnescapedValue;
		}

		public AttributeLiteralToken(
			TriviaSyntax leadingTrivia,
			string escapedValue,
			TriviaSyntax trailingTrivia,
			AttributeLiteralKind? literalKind = null)
			: base(leadingTrivia, trailingTrivia)
		{
			LiteralKind = literalKind ?? GetLiteralKind(escapedValue);
			_escapedValue = escapedValue ?? throw new ArgumentNullException(nameof(escapedValue));
		}

		public AttributeLiteralKind LiteralKind { get; }


		public string UnescapedValue => _cachedUnescapedValue ?? (_cachedUnescapedValue = Unescape());

		public override string Text => _escapedValue;

		public AttributeLiteralToken WithTrivia(TriviaSyntax? leadingTrivia = null, TriviaSyntax? trailingTrivia = null)
		{
			var leading = leadingTrivia ?? LeadingTrivia;
			var trailing = trailingTrivia ?? TrailingTrivia;

			if (leading == LeadingTrivia && trailing == TrailingTrivia)
				return this;

			return new AttributeLiteralToken(leading, _escapedValue, trailing, LiteralKind, _cachedUnescapedValue);
		}

		public AttributeLiteralToken With(string unescapedValue = null, AttributeLiteralKind? literalKind = null)
		{
			unescapedValue = unescapedValue ?? UnescapedValue;
			if ((literalKind == null || literalKind != LiteralKind) && unescapedValue == UnescapedValue)
				return this;

			switch (literalKind ?? LiteralKind)
			{
				case AttributeLiteralKind.DoubleQuoted:
				case AttributeLiteralKind.Invalid:
					return new AttributeLiteralToken(
						LeadingTrivia,
						string.Format("\"{0}\"", UxTextEncoding.EncodeAttribute(unescapedValue, '"')),
						TrailingTrivia,
						AttributeLiteralKind.DoubleQuoted,
						unescapedValue);

				case AttributeLiteralKind.SingleQuoted:
					return new AttributeLiteralToken(
						LeadingTrivia,
						string.Format("'{0}'", UxTextEncoding.EncodeAttribute(unescapedValue, '\'')),
						TrailingTrivia,
						AttributeLiteralKind.SingleQuoted,
						unescapedValue);

				case AttributeLiteralKind.Unquoted:
					if (SyntaxParser.IsValidUnquotedAttributeValue(unescapedValue))
						return new AttributeLiteralToken(
							LeadingTrivia,
							unescapedValue,
							TrailingTrivia,
							AttributeLiteralKind.Unquoted,
							unescapedValue);
					return new AttributeLiteralToken(
						LeadingTrivia,
						UxTextEncoding.EncodeAttribute(unescapedValue, '"'),
						TrailingTrivia,
						AttributeLiteralKind.DoubleQuoted,
						unescapedValue);

				default:
					throw new NotSupportedException("Attribute kind is unknown, don't know how to handle.");
			}
		}

		static AttributeLiteralKind GetLiteralKind(string escapedValue)
		{
			var len = escapedValue.Length;
			if (len > 1)
			{
				var quote = escapedValue[0];
				if (quote == '\"' || quote == '\'')
				{
					// Should find _one_ quote of same type at end of string
					if (escapedValue.IndexOf(quote, 1) != len - 1)
						return AttributeLiteralKind.Invalid;

					return quote == '\'' ? AttributeLiteralKind.SingleQuoted : AttributeLiteralKind.DoubleQuoted;
				}
			}
			return SyntaxParser.IsValidUnquotedAttributeValue(escapedValue)
				? AttributeLiteralKind.Unquoted
				: AttributeLiteralKind.Invalid;
		}

		string Unescape()
		{
			switch (LiteralKind)
			{
				case AttributeLiteralKind.SingleQuoted:
				case AttributeLiteralKind.DoubleQuoted:
					return UxTextEncoding.Unescape(_escapedValue, 1, _escapedValue.Length - 2);

				default:
					return _escapedValue;
			}
		}
	}
}