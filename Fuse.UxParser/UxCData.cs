using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxCData : UxNode
	{
		public UxCData(CDataSyntax syntax)
		{
			Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => Syntax.Value.Text;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				// TODO: Validate that text is valid for comment, ie does not contain "]]>"
				// throw an exception if not.
				if (value.Contains("]]>"))
					throw new ArgumentException("Value must not contain CData end marker");

				if (Value != value)
					ReplaceSyntax(new CDataSyntax(Syntax.Start, new EncodedTextToken(value), Syntax.End));
			}
		}

		protected override NodeSyntax NodeSyntax => Syntax;
		public new CDataSyntax Syntax { get; set; }

		public override XmlNodeType NodeType => XmlNodeType.CDATA;

		internal override UxNode DetachedNodeClone()
		{
			return new UxCData(Syntax);
		}

		public override void ReplaceSyntax(NodeSyntax newSyntax)
		{
			if (newSyntax == null) throw new ArgumentNullException(nameof(newSyntax));
			if (!(newSyntax is CDataSyntax newCDataSyntax))
				throw new ArgumentException("UxText can only have its syntax replaced by CDataSyntax object");

			var oldSyntax = Syntax;
			Syntax = newCDataSyntax;
			SetDirty();

			Container?.Changed?.Invoke(new UxReplaceNodeChange(NodePath, oldSyntax, newCDataSyntax));
		}
	}
}