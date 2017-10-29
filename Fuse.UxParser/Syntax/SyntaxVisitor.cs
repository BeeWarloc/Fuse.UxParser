using System;
using System.Collections.Immutable;

namespace Fuse.UxParser.Syntax
{
	public class SyntaxVisitor
	{
		protected virtual SyntaxToken VisitAttributeLiteral(AttributeLiteralToken token)
		{
			return token;
		}

		protected virtual SyntaxBase VisitAttribute(AttributeSyntax syntax)
		{
			var name = VisitAndConvert(syntax.Name);
			var eq = VisitAndConvert(syntax.Eq);
			var literal = VisitAndConvert(syntax.Literal);
			if (SyntaxEquals(syntax.Name, name) &&
				SyntaxEquals(syntax.Eq, eq) &&
				SyntaxEquals(syntax.Literal, literal))
				return syntax;
			return AttributeSyntax.Create(name, eq, literal);
		}

		protected virtual SyntaxToken VisitCDataEnd(CDataEndToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitCDataStart(CDataStartToken token)
		{
			return token;
		}

		protected virtual SyntaxBase VisitCData(CDataSyntax syntax)
		{
			var start = VisitAndConvert(syntax.Start);
			var value = VisitAndConvert(syntax.Value);
			var end = VisitAndConvert(syntax.End);
			if (SyntaxEquals(syntax.Start, start) &&
				SyntaxEquals(syntax.Value, value) &&
				SyntaxEquals(syntax.End, end))
				return syntax;
			return new CDataSyntax(start, value, end);
		}

		protected virtual SyntaxToken VisitCommentEnd(CommentEndToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitCommentStart(CommentStartToken token)
		{
			return token;
		}

		protected virtual SyntaxBase VisitComment(CommentSyntax syntax)
		{
			var start = VisitAndConvert(syntax.Start);
			var value = VisitAndConvert(syntax.Value);
			var end = VisitAndConvert(syntax.End);
			if (SyntaxEquals(syntax.Start, start) &&
				SyntaxEquals(syntax.Value, value) &&
				SyntaxEquals(syntax.End, end))
				return syntax;
			return new CommentSyntax(start, value, end);
		}

		protected virtual SyntaxBase VisitDocument(DocumentSyntax syntax)
		{
			var nodes = VisitAndConvert(syntax.Nodes);
			if (ReferenceEquals(syntax.Nodes, nodes))
				return syntax;
			return new DocumentSyntax(nodes);
		}

		protected virtual SyntaxBase VisitElementEndTag(ElementEndTagSyntax syntax)
		{
			var lessThan = VisitAndConvert(syntax.LessThan);
			var slash = VisitAndConvert(syntax.Slash);
			var name = VisitAndConvert(syntax.Name);
			var greaterThan = VisitAndConvert(syntax.GreaterThan);
			if (SyntaxEquals(syntax.LessThan, lessThan) &&
				SyntaxEquals(syntax.Slash, slash) &&
				SyntaxEquals(syntax.Name, name) &&
				SyntaxEquals(syntax.GreaterThan, greaterThan))
				return syntax;
			return new ElementEndTagSyntax(lessThan, slash, name, greaterThan);
		}

		protected virtual SyntaxBase VisitElementStartTag(ElementStartTagSyntax syntax)
		{
			var lessThan = VisitAndConvert(syntax.LessThan);
			var name = VisitAndConvert(syntax.Name);
			var attributes = VisitAndConvert(syntax.Attributes);
			var greaterThan = VisitAndConvert(syntax.GreaterThan);
			if (SyntaxEquals(syntax.LessThan, lessThan) &&
				SyntaxEquals(syntax.Name, name) &&
				ReferenceEquals(syntax.Attributes, attributes) &&
				SyntaxEquals(syntax.GreaterThan, greaterThan))
				return syntax;
			return ElementStartTagSyntax.Create(lessThan, name, attributes, greaterThan);
		}

		protected virtual SyntaxBase VisitElement(ElementSyntax syntax)
		{
			var startTag = VisitAndConvert(syntax.StartTag);
			var nodes = VisitAndConvert(syntax.Nodes);
			var endTag = VisitAndConvert(syntax.EndTag);
			if (SyntaxEquals(syntax.StartTag, startTag) &&
				ReferenceEquals(syntax.Nodes, nodes) &&
				SyntaxEquals(syntax.EndTag, endTag))
				return syntax;
			return new ElementSyntax(startTag, nodes, endTag);
		}

		protected virtual SyntaxBase VisitEmptyElement(EmptyElementSyntax syntax)
		{
			var lessThan = VisitAndConvert(syntax.LessThan);
			var name = VisitAndConvert(syntax.Name);
			var attributes = VisitAndConvert(syntax.Attributes);
			var slash = VisitAndConvert(syntax.Slash);
			var greaterThan = VisitAndConvert(syntax.GreaterThan);
			if (SyntaxEquals(syntax.LessThan, lessThan) &&
				SyntaxEquals(syntax.Name, name) &&
				ReferenceEquals(syntax.Attributes, attributes) &&
				SyntaxEquals(syntax.Slash, slash) &&
				SyntaxEquals(syntax.GreaterThan, greaterThan))
				return syntax;
			return new EmptyElementSyntax(lessThan, name, attributes, slash, greaterThan);
		}

		protected virtual SyntaxToken VisitEncodedText(EncodedTextToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitEquals(EqualsToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitGreaterThan(GreaterThanToken token)
		{
			return token;
		}

		protected virtual SyntaxBase VisitImplicitAttribute(ImplicitAttributeSyntax syntax)
		{
			var name = VisitAndConvert(syntax.Name);
			if (SyntaxEquals(syntax.Name, name))
				return syntax;
			return new ImplicitAttributeSyntax(name);
		}

		protected virtual SyntaxBase VisitInlineScriptElement(InlineScriptElementSyntax syntax)
		{
			var startTag = VisitAndConvert(syntax.StartTag);
			var script = VisitAndConvert(syntax.Script);
			var endTag = VisitAndConvert(syntax.EndTag);
			if (SyntaxEquals(syntax.StartTag, startTag) &&
				SyntaxEquals(syntax.Script, script) &&
				SyntaxEquals(syntax.EndTag, endTag))
				return syntax;
			return new InlineScriptElementSyntax(startTag, script, endTag);
		}

		protected virtual SyntaxToken VisitLessThan(LessThanToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitName(NameToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitScript(ScriptToken token)
		{
			return token;
		}

		protected virtual SyntaxToken VisitSlash(SlashToken token)
		{
			return token;
		}

		protected virtual SyntaxBase VisitText(TextSyntax syntax)
		{
			var value = VisitAndConvert(syntax.Value);
			if (SyntaxEquals(syntax.Value, value))
				return syntax;
			return new TextSyntax(value);
		}

		protected virtual bool SyntaxEquals(ISyntax a, ISyntax b)
		{
			return Equals(a, b);
		}

		public virtual ISyntax Visit(ISyntax syntax)
		{
			switch (syntax)
			{
				case AttributeLiteralToken s:
					return VisitAttributeLiteral(s);
				case AttributeSyntax s:
					return VisitAttribute(s);
				case CDataEndToken s:
					return VisitCDataEnd(s);
				case CDataStartToken s:
					return VisitCDataStart(s);
				case CDataSyntax s:
					return VisitCData(s);
				case CommentEndToken s:
					return VisitCommentEnd(s);
				case CommentStartToken s:
					return VisitCommentStart(s);
				case CommentSyntax s:
					return VisitComment(s);
				case DocumentSyntax s:
					return VisitDocument(s);
				case ElementEndTagSyntax s:
					return VisitElementEndTag(s);
				case ElementStartTagSyntax s:
					return VisitElementStartTag(s);
				case ElementSyntax s:
					return VisitElement(s);
				case EmptyElementSyntax s:
					return VisitEmptyElement(s);
				case EncodedTextToken s:
					return VisitEncodedText(s);
				case EqualsToken s:
					return VisitEquals(s);
				case GreaterThanToken s:
					return VisitGreaterThan(s);
				case ImplicitAttributeSyntax s:
					return VisitImplicitAttribute(s);
				case InlineScriptElementSyntax s:
					return VisitInlineScriptElement(s);
				case LessThanToken s:
					return VisitLessThan(s);
				case NameToken s:
					return VisitName(s);
				case ScriptToken s:
					return VisitScript(s);
				case SlashToken s:
					return VisitSlash(s);
				case TextSyntax s:
					return VisitText(s);
				default:
					throw new InvalidOperationException("syntax not recognized");
			}
		}

		public IImmutableList<T> VisitAndConvert<T>(IImmutableList<T> list)
			where T : ISyntax
		{
			ImmutableList<T>.Builder builder = null;
			for (var i = 0; i < list.Count; i++)
			{
				var visitedChild = VisitAndConvert(list[i]);
				if (builder == null)
				{
					if (SyntaxEquals(list[0], visitedChild))
						continue;
					builder = ImmutableList.CreateBuilder<T>();
					for (var u = 0; u < i; u++)
						builder.Add(list[u]);
				}
				builder.Add(visitedChild);
			}

			return builder != null ? builder.ToImmutable() : list;
		}

		public T VisitAndConvert<T>(T syntax)
			where T : ISyntax
		{
			return (T) Visit(syntax);
		}
	}
}