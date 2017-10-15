using System;

namespace Fuse.UxParser
{
	internal interface IUxContainerInternals : IUxContainer
	{
		Action<UxChange> Changed { get; }
		int NodesSourceOffset { get; }
		void SetDirty();
	}
}