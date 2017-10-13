using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlTextValueChange : XmlTextValueChangeBase<XText>
	{
		public XmlTextValueChange(XmlNodePath path, string newValue, string oldValue) : base(path, newValue, oldValue) { }

		public override XmlChangeType Type => XmlChangeType.TextChanged;

		protected override string GetValue(XText node)
		{
			return node.Value;
		}

		protected override void SetValue(XText node, string value)
		{
			node.Value = value;
		}
	}
}