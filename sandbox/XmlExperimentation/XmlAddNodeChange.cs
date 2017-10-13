using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlAddNodeChange : XmlChange
	{
		public XmlAddNodeChange(XmlNodePath path, IImmutableXNode node)
		{
			if (path == null) throw new ArgumentNullException("path");
			Path = path;
			Node = node;
		}

		public override XmlChangeType Type => XmlChangeType.AddNode;

		public XmlNodePath Path { get; }

		// Maybe as string, not element?? How will this work wrt. namespacing?
		public IImmutableXNode Node { get; }

		public override bool TryApply(XDocument doc)
		{
			return doc.TryInsert(Path, Node.Clone());
		}

		public override bool TryReverse(XDocument doc)
		{
			return doc.TryRemove(Path, (XNode) Node);
		}
	}
}