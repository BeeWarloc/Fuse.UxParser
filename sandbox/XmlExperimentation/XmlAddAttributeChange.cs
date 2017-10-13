using System;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlAddAttributeChange : XmlChange
	{
		readonly XmlNodePath _elementPath;

		public XmlAddAttributeChange(XmlNodePath elementPath, XName key, string value)
		{
			_elementPath = elementPath;
			Key = key;
			Value = value;
		}

		public override XmlChangeType Type => XmlChangeType.AddAttribute;

		public string Value { get; }

		public XName Key { get; }

		public override bool TryApply(XDocument doc)
		{
			return TryAddAttribute(doc, _elementPath, Key, Value);
		}

		public override bool TryReverse(XDocument doc)
		{
			throw new NotImplementedException();
		}
	}
}