using System;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public class UxCData : UxNode
	{
		CDataSyntax _syntax;

		public UxCData(CDataSyntax syntax)
		{
			_syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
		}

		public string Value
		{
			get => _syntax.Value.Text;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				// TODO: Validate that text is valid for comment, ie does not contain "]]>"
				// throw an exception if not.
				if (value.Contains("]]>"))
					throw new ArgumentException("Value must not contain CData end marker");

				if (Value != value)
				{
					_syntax = new CDataSyntax(_syntax.Start, new EncodedTextToken(value), _syntax.End);
					SetDirty();
				}
			}
		}

		public override NodeSyntax Syntax => _syntax;
		public override XmlNodeType NodeType => XmlNodeType.CDATA;

		internal override UxNode DetachedNodeClone()
		{
			return new UxCData(_syntax);
		}
	}
}