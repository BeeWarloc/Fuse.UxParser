using System.Xml.Linq;

namespace XmlExperimentation
{
	public abstract class XmlChange
	{
		public abstract XmlChangeType Type { get; }
		public abstract bool TryApply(XDocument doc);
		public abstract bool TryReverse(XDocument doc);

		internal static bool TryAddAttribute(XDocument doc, XmlNodePath elementPath, XName key, string value)
		{
			XElement element;
			if (!elementPath.TryFind(doc, out element))
				return false;

			// Fail if attribute already exists
			if (element.Attribute(key) != null)
				return false;

			// TODO: Think about attribute index?
			element.Add(new XAttribute(key, value));
			return true;
		}

		internal static bool TryRemoveAttribute(XDocument doc, XmlNodePath elementPath, XName key, string value)
		{
			XElement element;
			if (!elementPath.TryFind(doc, out element))
				return false;

			// Fail if attribute already exists
			var attribute = element.Attribute(key);
			if (attribute == null || attribute.Value != value)
				return false;

			attribute.Remove();

			// TODO: Think about attribute index?
			return true;
		}
	}
}