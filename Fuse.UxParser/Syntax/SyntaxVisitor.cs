using System;
using System.Collections.Immutable;

namespace Fuse.UxParser.Syntax
{
	public class SyntaxVisitor
	{
		protected virtual SyntaxBase VisitAttribute(AttributeSyntax syntax)
		{
			return syntax;
		}

		protected virtual SyntaxBase VisitCData(CDataSyntax syntax)
		{
			return syntax;
		}

		protected virtual SyntaxBase VisitComment(CommentSyntax syntax)
		{
			return syntax;
		}

		protected virtual SyntaxBase VisitDocument(DocumentSyntax syntax)
		{
			var nodes = VisitAndConvert(syntax.Nodes);
			if (Equals(syntax.Nodes, nodes)) return syntax;
			return new DocumentSyntax(nodes);
		}

		protected virtual SyntaxBase VisitElementEndTag(ElementEndTagSyntax syntax)
		{
			return syntax;
		}

		protected virtual SyntaxBase VisitElementStartTag(ElementStartTagSyntax syntax)
		{
			var attributes = VisitAndConvert(syntax.Attributes);
			if (Equals(syntax.Attributes, attributes)) return syntax;
			return new ElementStartTagSyntax(syntax.LessThan, syntax.Name, attributes, syntax.GreaterThan);
		}

		protected virtual SyntaxBase VisitElement(ElementSyntax syntax)
		{
			var startTag = VisitAndConvert(syntax.StartTag);
			var nodes = VisitAndConvert(syntax.Nodes);
			var endTag = VisitAndConvert(syntax.EndTag);
			if (Equals(syntax.StartTag, startTag) && Equals(syntax.Nodes, nodes) && Equals(syntax.EndTag, endTag)) return syntax;
			return new ElementSyntax(startTag, nodes, endTag);
		}

		protected virtual SyntaxBase VisitEmptyElement(EmptyElementSyntax syntax)
		{
			var attributes = VisitAndConvert(syntax.Attributes);
			if (Equals(syntax.Attributes, attributes)) return syntax;
			return new EmptyElementSyntax(syntax.LessThan, syntax.Name, attributes, syntax.Slash, syntax.GreaterThan);
		}

		protected virtual SyntaxBase VisitImplicitAttribute(ImplicitAttributeSyntax syntax)
		{
			return syntax;
		}

		protected virtual SyntaxBase VisitInlineScriptElement(InlineScriptElementSyntax syntax)
		{
			var startTag = VisitAndConvert(syntax.StartTag);
			var endTag = VisitAndConvert(syntax.EndTag);
			if (Equals(syntax.StartTag, startTag) && Equals(syntax.EndTag, endTag)) return syntax;
			return new InlineScriptElementSyntax(startTag, syntax.Script, endTag);
		}

		protected virtual SyntaxBase VisitText(TextSyntax syntax)
		{
			return syntax;
		}

		public virtual SyntaxBase Visit(SyntaxBase syntax)
		{
			switch (syntax)
			{
				case AttributeSyntax s:
					return VisitAttribute(s);
				case CDataSyntax s:
					return VisitCData(s);
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
				case ImplicitAttributeSyntax s:
					return VisitImplicitAttribute(s);
				case InlineScriptElementSyntax s:
					return VisitInlineScriptElement(s);
				case TextSyntax s:
					return VisitText(s);
				default:
					throw new InvalidOperationException("syntax not recognized");
			}
		}

		public IImmutableList<T> VisitAndConvert<T>(IImmutableList<T> list)
			where T : SyntaxBase
		{
			ImmutableList<T>.Builder builder = null;
			for (var i = 0; i < list.Count; i++)
			{
				var visitedChild = VisitAndConvert(list[i]);
				if (builder == null)
				{
					if (list[i].Equals(visitedChild))
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
			where T : SyntaxBase
		{
			return (T) Visit(syntax);
		}
	}
}