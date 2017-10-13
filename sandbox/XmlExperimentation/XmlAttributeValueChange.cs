using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlAttributeValueChange : XmlChange
	{
		public XmlAttributeValueChange(XmlNodePath elementPath, XName key, string newValue, string oldValue)
		{
			ElementPath = elementPath;
			Key = key;
			NewValue = newValue;
			OldValue = oldValue;
		}

		public override XmlChangeType Type => XmlChangeType.AttributeValue;

		public XmlNodePath ElementPath { get; }

		public XName Key { get; }

		public string NewValue { get; }

		public string OldValue { get; }

		public override bool TryApply(XDocument doc)
		{
			XAttribute attribute;
			if (!TryFindAttribute(doc, out attribute)) return false;

			if (attribute.Value == OldValue)
			{
				attribute.Value = NewValue;
				return true;
			}
			return false;
		}

		public override bool TryReverse(XDocument doc)
		{
			XAttribute attribute;
			if (!TryFindAttribute(doc, out attribute)) return false;

			if (attribute.Value == NewValue)
			{
				attribute.Value = OldValue;
				return true;
			}
			return false;
		}

		bool TryFindAttribute(XDocument doc, out XAttribute attribute)
		{
			XElement element;
			if (!ElementPath.TryFind(doc, out element))
			{
				attribute = null;
				return false;
			}

			attribute = element.Attribute(Key);

			if (attribute == null)
				return false;
			return true;
		}
	}
}