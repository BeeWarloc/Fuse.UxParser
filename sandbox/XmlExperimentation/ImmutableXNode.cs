using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	internal static class ImmutableXNode
	{
		public static IImmutableXNode Create(XNode node)
		{
			switch (node.NodeType)
			{
				case XmlNodeType.Element:
					return new ImmutableXElement((XElement) node);
				case XmlNodeType.CDATA:
					return new ImmutableXCData((XCData) node);
				case XmlNodeType.Comment:
					return new ImmutableXComment((XComment) node);
				case XmlNodeType.Text:
					return new ImmutableXText((XText) node);
				default:
					throw new NotSupportedException("Don't know how to clone node of type " + node.NodeType);
			}
		}

		static void EnforceImmutability(XNode node)
		{
			node.Changing += (s, e) => { throw new InvalidOperationException("Modifying immutable xml element not allowed."); };
		}

		class ImmutableXText : XText, IImmutableXNode
		{
			public ImmutableXText(XText other) : base(other) { }

			public XNode Clone()
			{
				return new XText(this);
			}
		}

		[SuppressMessage(
			"ReSharper",
			"InconsistentNaming",
			Justification =
				"If XCData is okay, then ImmutableXCData is also fine")]
		class ImmutableXCData : XCData, IImmutableXNode
		{
			public ImmutableXCData(XCData other) : base(other) { }

			public XNode Clone()
			{
				return new XCData(this);
			}
		}

		class ImmutableXComment : XComment, IImmutableXNode
		{
			public ImmutableXComment(XComment other) : base(other) { }

			public XNode Clone()
			{
				return new XComment(this);
			}
		}

		class ImmutableXElement : XElement, IImmutableXNode
		{
			public ImmutableXElement(XElement other) : base(other)
			{
				EnforceImmutability(this);
			}

			public XNode Clone()
			{
				return new XElement(this);
			}
		}
	}
}