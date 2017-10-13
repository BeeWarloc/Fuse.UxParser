using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Fuse.UxParser.Syntax
{
	public class DocumentSyntax : SyntaxBase
	{
		public DocumentSyntax(IImmutableList<NodeSyntax> nodes)
		{
			Nodes = nodes;
		}

		[NodeChild(0)]
		public IImmutableList<NodeSyntax> Nodes { get; }

		public ElementSyntaxBase Root => Nodes.OfType<ElementSyntaxBase>().FirstOrDefault();

		public override TriviaSyntax LeadingTrivia => Nodes.Count > 0 ? Nodes[0].LeadingTrivia : TriviaSyntax.Empty;

		public override TriviaSyntax TrailingTrivia =>
			Nodes.Count > 0 ? Nodes[Nodes.Count - 1].TrailingTrivia : TriviaSyntax.Empty;

		public override int FullSpan => Nodes.Sum(x => x.FullSpan);

		public override void Write(TextWriter writer)
		{
			foreach (var node in Nodes)
				node.Write(writer);
		}
	}
}