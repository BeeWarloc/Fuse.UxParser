using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlRemoveNodeChange : XmlChange
	{
		public XmlRemoveNodeChange(XmlNodePath path, IImmutableXNode node)
		{
			if (path == null) throw new ArgumentNullException("path");
			Path = path;
			Node = node;
		}

		public override XmlChangeType Type => XmlChangeType.RemoveNode;

		public XmlNodePath Path { get; }

		// Maybe as string, not element?? How will this work wrt. namespacing?
		public IImmutableXNode Node { get; }

		public override bool TryApply(XDocument doc)
		{
			return doc.TryRemove(Path, (XNode) Node);
		}

		public override bool TryReverse(XDocument doc)
		{
			return doc.TryInsert(Path, Node.Clone());
		}
	}
}