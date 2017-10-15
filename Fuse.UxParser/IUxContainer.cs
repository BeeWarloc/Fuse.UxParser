using System.Collections.Generic;

namespace Fuse.UxParser
{
	public interface IUxContainer
	{
		IList<UxNode> Nodes { get; }
	}
}