using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	/// <summary>
	/// Change between empty and non-empty element
	/// </summary>
	public class UxElementNameChange : UxChange
	{
		public UxElementNameChange(UxNodePath nodePath, NameToken oldName, NameToken newName)
		{
			NodePath = nodePath;
			OldName = oldName;
			NewName = newName;
		}

		public UxNodePath NodePath { get; }

		public NameToken OldName { get; }
		public NameToken NewName { get; }

		public override UxChange Invert()
		{
			return new UxElementNameChange(NodePath, NewName, OldName);
		}

		protected override bool OnTryApply(UxDocument document)
		{
			if (!NodePath.TryFind(document, out UxElement node))
				return false;

			if (!node.Syntax.Name.Equals(OldName))
				return false;

			node.NameToken = NewName;

			return true;
		}
	}
}