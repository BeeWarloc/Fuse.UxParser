namespace Fuse.UxParser
{
	/// <summary>
	/// Change between empty and non-empty element
	/// </summary>
	public class UxElementIsEmptyChange : UxChange
	{
		public UxElementIsEmptyChange(UxNodePath nodePath, bool isEmpty)
		{
			NodePath = nodePath;
			IsEmpty = isEmpty;
		}

		public UxNodePath NodePath { get; }
		public bool IsEmpty { get; }

		public override UxChange Invert()
		{
			return new UxElementIsEmptyChange(NodePath, !IsEmpty);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			if (!NodePath.TryFind(document, out UxElement node))
				return false;

			node.IsEmpty = IsEmpty;
			return true;
		}
	}
}