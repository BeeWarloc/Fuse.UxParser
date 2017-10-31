using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public class SyntaxParser
	{
		// Taken from XmlPreprocessor.IsNameChar(char c) method, should all these characters be legal?
		readonly Scanner _scanner;

		readonly Dictionary<ISyntax, ISyntax> _dedupMap = new Dictionary<ISyntax, ISyntax>();

		SyntaxParser(Scanner scanner)
		{
			_scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
		}

		public static NodeSyntax ParseNode(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));
			if (!new SyntaxParser(new Scanner(uxText)).TryParseNode(out var child))
				throw new InvalidOperationException("Unable to parse UX");
			return child;
		}

		public static IEnumerable<NodeSyntax> ParseNodes(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));
			var parser = new SyntaxParser(new Scanner(uxText));
			while (parser.TryParseNode(out var node))
				yield return node;
		}

		public static DocumentSyntax ParseDocument(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));
			var parser = new SyntaxParser(new Scanner(uxText));
			if (!parser.TryParseDocumentSyntax(out var doc))
				throw new InvalidOperationException("TODO: Proper error handling. Unable to parse doc");
			return doc;
		}

		T Dedup<T>(T syntax) where T : ISyntax
		{
			// return syntax;
			if (syntax.FullSpan < 42)
				if (_dedupMap.TryGetValue(syntax, out var dedupSyntax))
					syntax = (T) dedupSyntax;
				else
					_dedupMap[syntax] = syntax;
			return syntax;
		}

		bool TryParseNode(out NodeSyntax childBase)
		{
			return
				TryParseTextSyntax(out childBase) ||
				TryParseElementSyntax(out childBase) ||
				TryParseCDataSyntax(out childBase) ||
				TryParseCommentSyntax(out childBase);
		}

		bool TryParseCommentSyntax(out NodeSyntax node)
		{
			if (!TryParseCommentStartToken(out var start))
			{
				node = null;
				return false;
			}
			CommentEndToken end = null;
			if (!_scanner.ScanUntilMatch("-->", out var encodedText) ||
				!TryParseCommentEndToken(out end))
				_scanner.ThrowError();
			node = Dedup(CommentSyntax.Create(start, new EncodedTextToken(encodedText), end));
			return true;
		}

		bool TryParseCommentStartToken(out CommentStartToken token)
		{
			return TryParseString(
				out token,
				"<!--",
				(leadingWs, trailingWs) => Dedup(new CommentStartToken(leadingWs, trailingWs)),
				false);
		}

		bool TryParseCommentEndToken(out CommentEndToken token)
		{
			return TryParseString(
				out token,
				"-->",
				(leadingWs, trailingWs) => Dedup(new CommentEndToken(leadingWs, trailingWs)),
				false);
		}

		bool TryParseLessThanToken(out LessThanToken lessThan)
		{
			return TryParseChar(out lessThan, '<', LessThanToken.Create);
		}

		bool TryParseEqualsToken(out EqualsToken eqToken)
		{
			return TryParseChar(
				out eqToken,
				'=',
				EqualsToken.Create);
		}

		bool TryParseGreaterThanToken(
			out GreaterThanToken greaterThanToken,
			bool includeTrailingTrivia = false)
		{
			return TryParseChar(
				out greaterThanToken,
				'>',
				GreaterThanToken.Create,
				includeTrailingTrivia);
		}

		bool TryParseEndTag(out ElementEndTagSyntax endTag)
		{
			using (var scope = _scanner.Scope())
			{
				if (TryParseLessThanToken(out var lessThan) &&
					TryParseSlashToken(out var slashToken) &&
					TryParseNameToken(out var name) &&
					TryParseGreaterThanToken(out var greaterThan))
				{
					scope.Commit();
					endTag = Dedup(ElementEndTagSyntax.Create(lessThan, slashToken, name, greaterThan));
					return true;
				}

				endTag = null;
				return false;
			}
		}

		TriviaSyntax ParseWhitespace()
		{
			_scanner.ScanZeroOrMore(char.IsWhiteSpace, out var ws);
			if (ws == string.Empty)
				return TriviaSyntax.Empty;
			if (ws == " ")
				return TriviaSyntax.Space;
			return Dedup(TriviaSyntax.Create(ws));
		}


		bool TryParseTextSyntax(out NodeSyntax childBase)
		{
			if (_scanner.ScanOneOrMore(c => c != '<', out var text))
			{
				childBase = TextSyntax.Create(new EncodedTextToken(text));
				return true;
			}
			childBase = null;
			return false;
		}


		bool TryParseSlashToken(out SlashToken slashToken)
		{
			return TryParseChar(out slashToken, '/', SlashToken.Create);
		}

		bool TryParseNameToken(out NameToken token, bool includeTrailingTrivia = true)
		{
			using (var scope = _scanner.Scope())
			{
				var first = true;
				var leadingWhitespace = ParseWhitespace();
				if (!_scanner.ScanOneOrMore(
					c =>
					{
						var isMatch = first
							? char.IsLetter(c) || c == '_' || c == ':'
							: char.IsLetterOrDigit(c) || c == '_' || c == ':' || c == '.' ||
							c == '-'; // TODO  |  CombiningChar | Extender  http://cs.lmu.edu/~ray/notes/xmlgrammar/
						first = false;
						return isMatch;
					},
					out var text))
				{
					token = null;
					return false;
				}

				scope.Commit();

				var trailingWhitespace = includeTrailingTrivia ? ParseWhitespace() : TriviaSyntax.Empty;
				token = Dedup(new NameToken(leadingWhitespace, text, trailingWhitespace));
				return true;
			}
		}

		bool TryParseDocumentSyntax(out DocumentSyntax document)
		{
			var childNodes = ImmutableArray.CreateBuilder<NodeSyntax>();

			while (TryParseNode(out var childBase))
				childNodes.Add(childBase);

			document = DocumentSyntax.Create(childNodes.ToImmutable());
			return true;
		}

		bool TryParseElementSyntax(out NodeSyntax node)
		{
			using (var scope = _scanner.Scope())
			{
				if (!TryParseLessThanToken(out var lessThan) || !TryParseNameToken(out var name, false))
				{
					node = null;
					return false;
				}
				scope.Commit();

				// We use ImmutableArray instead of ImmutableList here since performance should be
				// better than ImmutableList when N < 16, which N usually is.
				var attributesBuilder = ImmutableArray.CreateBuilder<AttributeSyntaxBase>();
				while (TryParseAttribute(_scanner, out var attribute))
					attributesBuilder.Add(attribute);

				var attributes = attributesBuilder.ToImmutable();

				var isEmpty = TryParseSlashToken(out var slashToken);
				if (!TryParseGreaterThanToken(out var greaterThan))
					_scanner.ThrowError();

				if (isEmpty)
				{
					node = EmptyElementSyntax.Create(lessThan, name, attributes, slashToken, greaterThan);
					return true;
				}

				var startTag = ElementStartTagSyntax.Create(lessThan, name, attributes, greaterThan);

				var childNodes = ImmutableArray.CreateBuilder<NodeSyntax>();

				// Special handling of <JavaScript> element
				if (name.Text == "JavaScript")
				{
					if (!_scanner.ScanUntilMatch("</" + name.Text, out var content))
						_scanner.ThrowError();

					if (!TryParseEndTag(out var scriptEndTag) || scriptEndTag.Name.Text != startTag.Name.Text)
						_scanner.ThrowError();

					node = InlineScriptElementSyntax.Create(startTag, new ScriptToken(content), scriptEndTag);
					return true;
				}

				while (TryParseNode(out var childBase))
					childNodes.Add(childBase);

				ElementEndTagSyntax endTag;
				using (var endTagScope = _scanner.Scope())
				{
					if (!TryParseEndTag(out endTag) || endTag.Name.Text != startTag.Name.Text)
						endTag = ElementEndTagSyntax.Create(
							LessThanToken.Missing,
							SlashToken.Missing,
							NameToken.Missing,
							GreaterThanToken.Missing);
					else
						endTagScope.Commit();
				}
				node = Dedup(ElementSyntax.Create(startTag, childNodes.ToImmutable(), endTag));
				return true;
			}
		}

		bool TryParseCDataStartToken(out CDataStartToken token)
		{
			return TryParseString(
				out token,
				"<![CDATA[",
				(leadingWs, trailingWs) => new CDataStartToken(leadingWs, trailingWs),
				false);
		}

		bool TryParseCDataSyntax(out NodeSyntax node)
		{
			if (!TryParseCDataStartToken(out var start))
			{
				node = null;
				return false;
			}
			CDataEndToken end = null;
			if (!_scanner.ScanUntilMatch("]]>", out var encodedText) ||
				!TryParseCDataEndToken(out end))
				_scanner.ThrowError();
			node = CDataSyntax.Create(start, new EncodedTextToken(encodedText), end);
			return true;
		}

		bool TryParseCDataEndToken(out CDataEndToken token)
		{
			return TryParseString(
				out token,
				"]]>",
				(leadingWs, trailingWs) => new CDataEndToken(leadingWs, trailingWs),
				false);
		}

		internal bool TryParseChar<T>(
			out T lessThan,
			char c,
			Func<TriviaSyntax, TriviaSyntax, T> ctor,
			bool includeTrailingTrivia = true)
			where T : SyntaxToken
		{
			using (var scope = _scanner.Scope())
			{
				lessThan = null;
				var leadingWhitespace = ParseWhitespace();
				if (!_scanner.Scan(c)) return false;
				scope.Commit();
				var trailingWhitespace = includeTrailingTrivia ? ParseWhitespace() : TriviaSyntax.Empty;
				lessThan = ctor(leadingWhitespace, trailingWhitespace);
				return true;
			}
		}

		internal bool TryParseString<T>(
			out T token,
			string str,
			Func<TriviaSyntax, TriviaSyntax, T> ctor,
			bool scanTrailingWhitespace = true)
			where T : SyntaxToken
		{
			using (var scope = _scanner.Scope())
			{
				token = null;
				var leadingWhitespace = ParseWhitespace();
				if (!_scanner.Scan(str)) return false;
				scope.Commit();
				var trailingWhitespace = scanTrailingWhitespace ? ParseWhitespace() : TriviaSyntax.Empty;
				token = ctor(leadingWhitespace, trailingWhitespace);
				return true;
			}
		}

		bool TryParseAttributeLiteralToken(Scanner scanner, out AttributeLiteralToken token)
		{
			var leadingWhitespace = ParseWhitespace();
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

			var trailingWhitespace = ParseWhitespace();
			token = Dedup(new AttributeLiteralToken(leadingWhitespace, text, trailingWhitespace));
			return true;
		}


		bool TryParseAttribute(Scanner scanner, out AttributeSyntaxBase attribute)
		{
			using (var scope = scanner.Scope())
			{
				if (!TryParseNameToken(out var name))
				{
					attribute = null;
					return false;
				}
				scope.Commit();
				if (!TryParseEqualsToken(out var eqToken))
				{
					attribute = ImplicitAttributeSyntax.Create(name);
					return true;
				}

				if (!TryParseAttributeLiteralToken(scanner, out var literalToken))
					scanner.ThrowError();

				attribute = Dedup(AttributeSyntax.Create(name, eqToken, literalToken));
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

		internal static bool IsValidUnquotedAttributeValue(string str)
		{
			return str.Length > 0 && str.All(IsValidUnquotedAttributeValueChar);
		}
	}
}