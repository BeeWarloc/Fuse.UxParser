using System;

namespace Fuse.UxParser
{
	public abstract class UxChange
	{
		public bool TryApply(UxDocument document)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			return OnTryApply(document);
		}

		protected abstract UxChange Invert();

		protected abstract bool OnTryApply(UxDocument document);
	}
}