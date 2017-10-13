using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public static class SyntaxParser
	{
		// Taken from XmlPreprocessor.IsNameChar(char c) method, should all these characters be legal?


		public static NodeSyntax ParseNode(string uxText)
		{
			if (!TryParseNode(new Scanner(uxText), out var child))
				throw new InvalidOperationException("Unable to parse UX");
			return child;
		}

		public static IEnumerable<NodeSyntax> ParseNodes(string uxText)
		{
			var scanner = new Scanner(uxText);
			while (TryParseNode(scanner, out var node))
				yield return node;
		}

		public static DocumentSyntax ParseDocument(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));

			if (!TryParseDocumentSyntax(new Scanner(uxText), out var doc))
				throw new InvalidOperationException("TODO: Proper error handling. Unable to parse doc");
			return doc;
		}

		public static bool TryParse(Scanner scanner, out NodeSyntax node)
		{
			CommentStartToken start;
			if (!CommentStartToken.TryParseCommentStartToken(scanner, out start))
			{
				node = null;
				return false;
			}
			string encodedText;
			CommentEndToken end = null;
			if (!scanner.ScanUntilMatch("-->", out encodedText) ||
				!CommentEndToken.TryParseCommentEndToken(scanner, out end))
				scanner.ThrowError();
			node = new CommentSyntax(start, new EncodedTextToken(encodedText), end);
			return true;
		}

		static bool TryParseEndTag(Scanner scanner, out ElementEndTagSyntax endTag)
		{
			using (var scope = scanner.Scope())
			{
				LessThanToken lessThan;
				SlashToken slashToken;
				NameToken name;
				GreaterThanToken greaterThan;
				if (TryParseLessThanToken(scanner, out lessThan) &&
					TryParseSlashToken(scanner, out slashToken) &&
					TryParseNameToken(scanner, out name) &&
					TryParseGreaterThanToken(scanner, out greaterThan))
				{
					scope.Commit();
					endTag = new ElementEndTagSyntax(lessThan, slashToken, name, greaterThan);
					return true;
				}

				endTag = null;
				return false;
			}
		}

		public static TriviaSyntax ParseWhitespace(Scanner scanner)
		{
			string ws;
			scanner.ScanZeroOrMore(char.IsWhiteSpace, out ws);
			if (ws == string.Empty)
				return TriviaSyntax.Empty;
			if (ws == " ")
				return TriviaSyntax.Space;
			return new TriviaSyntax(ws);
		}


		public static bool TryParseTextSyntax(Scanner scanner, out NodeSyntax childBase)
		{
			string text;
			if (scanner.ScanOneOrMore(c => c != '<', out text))
			{
				childBase = new TextSyntax(new EncodedTextToken(text));
				return true;
			}
			childBase = null;
			return false;
		}


		public static bool TryParseSlashToken(Scanner scanner, out SlashToken slashToken)
		{
			return TryParseChar(scanner, out slashToken, '/', (leadingWs, trailingWs) => new SlashToken(leadingWs, trailingWs));
		}

		public static bool TryParseNameToken(Scanner scanner, out NameToken token, bool includeTrailingTrivia = true)
		{
			using (var scope = scanner.Scope())
			{
				var first = true;
				string text;
				var leadingWhitespace = scanner.ScanWhitespace();
				if (!scanner.ScanOneOrMore(
					c =>
					{
						var isMatch = first
							? char.IsLetter(c) || c == '_' || c == ':'
							: char.IsLetterOrDigit(c) || c == '_' || c == ':' || c == '.' ||
							c == '-'; // TODO  |  CombiningChar | Extender  http://cs.lmu.edu/~ray/notes/xmlgrammar/
						first = false;
						return isMatch;
					},
					out text))
				{
					token = null;
					return false;
				}

				scope.Commit();

				var trailingWhitespace = includeTrailingTrivia ? scanner.ScanWhitespace() : TriviaSyntax.Empty;
				token = new NameToken(leadingWhitespace, text, trailingWhitespace);
				return true;
			}
		}

		public static bool TryParseLessThanToken(Scanner scanner, out LessThanToken lessThan)
		{
			return TryParseChar(scanner, out lessThan, '<', (leadingWs, trailingWs) => new LessThanToken(leadingWs, trailingWs));
		}

		public static bool TryParseEqualsToken(Scanner scanner, out EqualsToken eqToken)
		{
			return TryParseChar(
				scanner,
				out eqToken,
				'=',
				(leadingWs, trailingWs) => new EqualsToken(leadingWs, trailingWs));
		}

		public static bool TryParseGreaterThanToken(
			Scanner scanner,
			out GreaterThanToken greaterThanToken,
			bool includeTrailingTrivia = false)
		{
			return TryParseChar(
				scanner,
				out greaterThanToken,
				'>',
				(leadingWs, trailingWs) => new GreaterThanToken(leadingWs, trailingWs),
				includeTrailingTrivia);
		}

		public static bool TryParseDocumentSyntax(Scanner scanner, out DocumentSyntax document)
		{
			var childNodes = ImmutableArray.CreateBuilder<NodeSyntax>();

			NodeSyntax childBase;
			while (TryParseNode(scanner, out childBase))
				childNodes.Add(childBase);

			document = new DocumentSyntax(childNodes.ToImmutable());
			return true;
		}

		public static bool TryParseElementSyntax(Scanner scanner, out NodeSyntax node)
		{
			using (var scope = scanner.Scope())
			{
				LessThanToken lessThan;
				NameToken name;
				if (!TryParseLessThanToken(scanner, out lessThan) || !TryParseNameToken(scanner, out name, false))
				{
					node = null;
					return false;
				}
				scope.Commit();

				// We use ImmutableArray instead of ImmutableList here since performance should be
				// better than ImmutableList when N < 16, which N usually is.
				var attributesBuilder = ImmutableArray.CreateBuilder<AttributeSyntaxBase>();
				AttributeSyntaxBase attribute;
				while (TryParseAttribute(scanner, out attribute))
					attributesBuilder.Add(attribute);

				var attributes = attributesBuilder.ToImmutable();

				SlashToken slashToken;
				var isEmpty = TryParseSlashToken(scanner, out slashToken);
				GreaterThanToken greaterThan;
				if (!TryParseGreaterThanToken(scanner, out greaterThan, false))
					scanner.ThrowError();

				if (isEmpty)
				{
					node = new EmptyElementSyntax(lessThan, name, attributes, slashToken, greaterThan);
					return true;
				}

				var startTag = new ElementStartTagSyntax(lessThan, name, attributes, greaterThan);

				var childNodes = ImmutableList.CreateBuilder<NodeSyntax>();

				// Special handling of <JavaScript> element
				if (name.Text == "JavaScript")
				{
					// TODO: Support whitespace inside endtag
					string content;
					if (!scanner.ScanUntilMatch("</" + name.Text, out content))
						scanner.ThrowError();
					// TODO: Need a separate node type for this

					ElementEndTagSyntax scriptEndTag;
					if (!TryParseEndTag(scanner, out scriptEndTag) || scriptEndTag.Name.Text != startTag.Name.Text)
						scanner.ThrowError();

					node = new InlineScriptElementSyntax(startTag, new ScriptToken(content), scriptEndTag);
					return true;
				}

				NodeSyntax childBase;
				while (TryParseNode(scanner, out childBase))
					childNodes.Add(childBase);

				ElementEndTagSyntax endTag;
				if (!TryParseEndTag(scanner, out endTag) || endTag.Name.Text != startTag.Name.Text)
					scanner.ThrowError();
				node = new ElementSyntax(startTag, childNodes.ToImmutable(), endTag);
				return true;
			}
		}

		public static bool TryParseCDataSyntax(Scanner scanner, out NodeSyntax node)
		{
			CDataStartToken start;
			if (!CDataStartToken.TryParseCDataStartToken(scanner, out start))
			{
				node = null;
				return false;
			}
			string encodedText;
			CDataEndToken end = null;
			if (!scanner.ScanUntilMatch("]]>", out encodedText) ||
				!TryParseCDataEndToken(scanner, out end))
				scanner.ThrowError();
			node = new CDataSyntax(start, new EncodedTextToken(encodedText), end);
			return true;
		}

		public static bool TryParseNode(Scanner scanner, out NodeSyntax childBase)
		{
			return
				TryParseTextSyntax(scanner, out childBase) ||
				TryParseElementSyntax(scanner, out childBase) ||
				TryParseCDataSyntax(scanner, out childBase) ||
				TryParse(scanner, out childBase);
		}

		public static bool TryParseCDataEndToken(Scanner scanner, out CDataEndToken token)
		{
			return TryParseString(
				scanner,
				out token,
				"]]>",
				(leadingWs, trailingWs) => new CDataEndToken(leadingWs, trailingWs),
				false);
		}

		internal static bool TryParseChar<T>(
			Scanner scanner,
			out T lessThan,
			char c,
			Func<TriviaSyntax, TriviaSyntax, T> ctor,
			bool includeTrailingTrivia = true)
			where T : SyntaxToken
		{
			using (var scope = scanner.Scope())
			{
				lessThan = null;
				var leadingWhitespace = ParseWhitespace(scanner);
				if (!scanner.Scan(c)) return false;
				scope.Commit();
				var trailingWhitespace = includeTrailingTrivia ? ParseWhitespace(scanner) : TriviaSyntax.Empty;
				lessThan = ctor(leadingWhitespace, trailingWhitespace);
				return true;
			}
		}

		internal static bool TryParseString<T>(
			Scanner scanner,
			out T token,
			string str,
			Func<TriviaSyntax, TriviaSyntax, T> ctor,
			bool scanTrailingWhitespace = true)
			where T : SyntaxToken
		{
			using (var scope = scanner.Scope())
			{
				token = null;
				var leadingWhitespace = ParseWhitespace(scanner);
				if (!scanner.Scan(str)) return false;
				scope.Commit();
				var trailingWhitespace = scanTrailingWhitespace ? ParseWhitespace(scanner) : TriviaSyntax.Empty;
				token = ctor(leadingWhitespace, trailingWhitespace);
				return true;
			}
		}

		public static bool TryParseAttributeLiteralToken(Scanner scanner, out AttributeLiteralToken token)
		{
			var leadingWhitespace = ParseWhitespace(scanner);
			string text;

			var quote = scanner.Peek();
			if (quote == '\'' || quote == '\"')
			{
				var remainingQuotes = 2;
				if (!scanner.ScanOneOrMore(
					c =>
					{
						if (c == quote)
						{
							remainingQuotes--;
							return true;
						}
						return remainingQuotes > 0;
					},
					out text))
					scanner.ThrowError();
			}
			else
			{
				if (!scanner.ScanOneOrMore(IsValidUnquotedAttributeValueChar, out text))
					scanner.ThrowError();
			}

			var trailingWhitespace = ParseWhitespace(scanner);
			token = new AttributeLiteralToken(leadingWhitespace, text, trailingWhitespace);
			return true;
		}


		public static bool TryParseAttribute(Scanner scanner, out AttributeSyntaxBase attribute)
		{
			using (var scope = scanner.Scope())
			{
				NameToken name;
				EqualsToken eqToken;
				if (!TryParseNameToken(scanner, out name))
				{
					attribute = null;
					return false;
				}
				scope.Commit();
				if (!TryParseEqualsToken(scanner, out eqToken))
				{
					attribute = new ImplicitAttributeSyntax(name);
					return true;
				}

				AttributeLiteralToken literalToken;
				if (!TryParseAttributeLiteralToken(scanner, out literalToken))
					scanner.ThrowError();

				attribute = new AttributeSyntax(name, eqToken, literalToken);
				return true;
			}
		}

		static bool IsValidUnquotedAttributeValueChar(char c)
		{
			switch (c)
			{
				case '.':
				case ':':
				case '-':
				case '_':
				case '!':
				case '[':
				case ']':
				case '(':
				case ')':
				case '{':
				case '}':
				case '*':
					return true;
				default:
					return char.IsLetterOrDigit(c);
			}
		}

		public static bool IsValidUnquotedAttributeValue(string str)
		{
			return str.Length > 0 && str.All(IsValidUnquotedAttributeValueChar);
		}
	}
}