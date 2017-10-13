using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlCommentValueChange : XmlTextValueChangeBase<XComment>
	{
		public XmlCommentValueChange(XmlNodePath path, string newValue, string oldValue) : base(path, newValue, oldValue) { }

		public override XmlChangeType Type => XmlChangeType.CommentChanged;

		protected override string GetValue(XComment node)
		{
			return node.Value;
		}

		protected override void SetValue(XComment node, string value)
		{
			node.Value = value;
		}
	}
}