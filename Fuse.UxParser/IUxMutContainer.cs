using System;
using System.Collections.Generic;

namespace Fuse.UxParser
{
	internal interface IUxMutContainer
	{
		IList<UxNode> Nodes { get; }
		Action<UxChange> Changed { get; }
		int NodesSourceOffset { get; }
		void SetDirty();
	}
}