using System.Xml.Linq;

namespace XmlExperimentation
{
	public interface IImmutableXNode
	{
		XNode Clone();
	}
}