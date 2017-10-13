using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlCDataValueChange : XmlTextValueChangeBase<XCData>
	{
		public XmlCDataValueChange(XmlNodePath path, string newValue, string oldValue) : base(path, newValue, oldValue) { }

		public override XmlChangeType Type => XmlChangeType.CDataChanged;

		protected override string GetValue(XCData node)
		{
			return node.Value;
		}

		protected override void SetValue(XCData node, string value)
		{
			node.Value = value;
		}
	}
}