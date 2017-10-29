using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public partial class UxElement : UxNode, IUxContainerInternals
	{
		AttributeList _attributes;
		bool _isDirty;
		bool _isEmpty;
		NameToken _name;
		NodeList _nodes;

		ElementSyntaxBase _syntax;

		public UxElement(ElementSyntaxBase syntax)
		{
			_syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
			_name = _syntax.Name;
			_isEmpty = syntax.IsEmpty;
		}

		public static UxElement Parse(string uxText)
		{
			if (uxText == null) throw new ArgumentNullException(nameof(uxText));
			return new UxElement((ElementSyntaxBase) SyntaxParser.ParseNode(uxText));
		}

		public IList<UxAttribute> Attributes => _attributes ?? (_attributes = new AttributeList(this));
		public IList<UxNode> Nodes => _nodes ?? (_nodes = new NodeList(this, _syntax.Nodes));
		public IEnumerable<UxElement> Elements => Nodes.OfType<UxElement>();

		public int DescendantElementCount => Syntax.DescendantElementCount;

		public bool IsEmpty
		{
			get => _isEmpty;
			set
			{
				if (value != IsEmpty)
				{
					// Trying to set IsEmpty to true when there are nodes will not do anything
					if (value && (_nodes?.Count ?? _syntax.Nodes.Count) > 0)
						return;

					_isEmpty = value;
					SetDirty();

					Changed?.Invoke(new UxElementIsEmptyChange(NodePath, _isEmpty));
				}
			}
		}

		//internal NameToken NameToken
		//{
		//	get => ((ElementSyntaxBase)Syntax).Name;
		//}

		public string Name
		{
			get => _name.Text;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				NameToken = NameToken.With(value);
			}
		}

		internal NameToken NameToken
		{
			get => _name;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				// TODO: Validate

				if (!value.Equals(_name))
				{
					var oldName = _name;

					_name = value;
					SetDirty();

					Changed?.Invoke(new UxElementNameChange(NodePath, oldName, _name));
				}
			}
		}

		protected override NodeSyntax NodeSyntax => Syntax;

		public new ElementSyntaxBase Syntax
		{
			get
			{
				if (_isDirty)
				{
					var attributes = _attributes?.Select(x => x.Syntax).ToImmutableArray() ?? _syntax.Attributes;
					var nodes = _nodes?.Select(x => x.Syntax).ToImmutableList() ?? _syntax.Nodes;
					_syntax = _syntax.With(_name, attributes, nodes, _isEmpty);
					_isEmpty = _syntax.IsEmpty;
					_isDirty = false;
				}
				return _syntax;
			}
		}

		public override XmlNodeType NodeType => XmlNodeType.Element;

		public UxNode FirstNode => Nodes.FirstOrDefault();
		public UxAttribute FirstAttribute => Attributes.FirstOrDefault();

		Action<UxChange> IUxContainerInternals.Changed => Changed;
		Action<UxChange> Changed => Container?.Changed;

		public void AddFirst(UxNode node)
		{
			Nodes.Insert(0, node);
		}

		public void Add(UxNode node)
		{
			Nodes.Add(node);
		}

		public void AddRawUx(string ux)
		{
			foreach (var nodeSyntax in SyntaxParser.ParseNodes(ux))
				Nodes.Add(FromSyntax(nodeSyntax));
		}

		public void SetAttributeValue(string name, string value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var attr = Attributes.FirstOrDefault(x => x.Name == name);

			if (attr != null)
			{
				if (value == null)
					Attributes.Remove(attr);
				else
					attr.Value = value;
			}
			else
			{
				// TODO: Introduce better factory methods for syntax
				var attributeSyntax = AttributeSyntax.Create(
					new NameToken(TriviaSyntax.Space, name, TriviaSyntax.Empty),
					EqualsToken.Default,
					new AttributeLiteralToken(
						TriviaSyntax.Empty,
						string.Format("\"{0}\"", UxTextEncoding.EncodeAttribute(value, '"')),
						TriviaSyntax.Empty));

				Attributes.Add(
					new UxAttribute(
						attributeSyntax));
			}
		}

		public IEnumerable<UxElement> DescendantsAndSelf()
		{
			return GetDescendants(true);
		}

		public IEnumerable<UxElement> Descendants()
		{
			return GetDescendants(false);
		}

		IEnumerable<UxElement> GetDescendants(bool self)
		{
			if (self)
				yield return this;

			if (Nodes.Count > 0)
				foreach (var el in Elements.SelectMany(x => x.GetDescendants(true)))
					yield return el;
		}

		internal override IEnumerable<UxNode> GetDescendantNodes(bool self)
		{
			if (self)
				yield return this;

			if (Nodes.Count > 0)
				foreach (var el in Nodes.SelectMany(x => x.GetDescendantNodes(true)))
					yield return el;
		}

		public override void ReplaceSyntax(NodeSyntax newSyntax)
		{
			// Could maybe support this by clearing both attributes and node lists and starting over
			// Or maybe it would be easier to just do merging directly here?
			throw new NotSupportedException();
		}

		internal override void SetDirty()
		{
			if (_isDirty)
				return;

			_isDirty = true;
			base.SetDirty();
		}

		internal override UxNode DetachedNodeClone()
		{
			return new UxElement(Syntax);
		}

		int IUxContainerInternals.NodesSourceOffset =>
			SourceOffset + (Syntax as ElementSyntax)?.StartTag.FullSpan ??
			throw new InvalidOperationException(
				"Bad code path. Noone should ask for the NodesSourceOffset while underlying syntax is not an ElementSyntax");

		void IUxContainerInternals.SetDirty()
		{
			SetDirty();
		}
	}
}