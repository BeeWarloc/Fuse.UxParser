using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public abstract class XmlTextValueChangeBase<TNode> : XmlChange
		where TNode : XNode
	{
		public XmlTextValueChangeBase(XmlNodePath path, string newValue, string oldValue)
		{
			if (path == null) throw new ArgumentNullException("path");
			Path = path;
			NewValue = newValue;
			OldValue = oldValue;
		}

		public XmlNodePath Path { get; }

		// Maybe as string, not element?? How will this work wrt. namespacing?
		public string OldValue { get; }

		public string NewValue { get; }

		public override bool TryApply(XDocument doc)
		{
			TNode textNode;
			if (Path.TryFind(doc, out textNode))
				if (GetValue(textNode) == OldValue)
				{
					SetValue(textNode, NewValue);
					return true;
				}
			return false;
		}

		public override bool TryReverse(XDocument doc)
		{
			XText textNode;
			if (Path.TryFind(doc, out textNode))
				if (textNode.Value == NewValue)
				{
					textNode.Value = OldValue;
					return true;
				}
			return false;
		}

		protected abstract string GetValue(TNode node);
		protected abstract void SetValue(TNode node, string value);
	}
}