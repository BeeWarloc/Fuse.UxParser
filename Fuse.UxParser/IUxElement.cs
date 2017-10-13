using System.Collections.Generic;

namespace Fuse.UxParser
{
	public interface IUxElement : IUxNode
	{
		IList<IUxAttribute> Attributes { get; }
		IList<IUxNode> Nodes { get; }
		IUxAttribute SetAttributeValue(string key, string value);
	}
}