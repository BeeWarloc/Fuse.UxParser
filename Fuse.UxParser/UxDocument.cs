using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxDocument : UxObject, IUxContainerInternals
	{
		bool _isDirty;
		UxNode.NodeList _nodeList;
		DocumentSyntax _syntax;

		protected UxDocument(DocumentSyntax syntax)
		{
			_syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public static UxDocument FromSyntax(DocumentSyntax syntax)
		{
			return new UxDocument(syntax);
		}

		public DocumentSyntax Syntax
		{
			get
			{
				if (_isDirty)
				{
					_syntax = DocumentSyntax.Create(Nodes.Select(x => x.Syntax).ToImmutableList());
					_isDirty = false;
				}
				return _syntax;
			}
		}

		public UxElement Root => Nodes.OfType<UxElement>().FirstOrDefault();


		void IUxContainerInternals.SetDirty()
		{
			_isDirty = true;
		}

		string IUxContainerInternals.PrefixToNamespace(string prefix)
		{
			return Constants.DefaultNamespacePrefixes.Where(x => x.Key == prefix).Select(x => x.Value).FirstOrDefault() ??
				Constants.DefaultFuseNamespaceList;
		}

		public IList<UxNode> Nodes => _nodeList ?? (_nodeList = new UxNode.NodeList(this, Syntax.Nodes));

		Action<UxChange> IUxContainerInternals.Changed => Changed;
		int IUxContainerInternals.NodesSourceOffset => 0;

		public event Action<UxChange> Changed;

		public static UxDocument Parse(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));
			return new UxDocument(SyntaxParser.ParseDocument(uxText));
		}

		public IEnumerable<UxNode> DescendantNodes()
		{
			return Nodes.SelectMany(x => x.DescendantNodesAndSelf());
		}

		public override string ToString()
		{
			return Syntax.ToString();
		}
	}
}