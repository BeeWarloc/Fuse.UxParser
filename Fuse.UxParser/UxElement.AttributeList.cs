using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public partial class UxElement
	{
		class AttributeList : WrapperList<UxAttribute, AttributeSyntaxBase>
		{
			// TODO: IMPLEMENT HERE 😀

			public AttributeList(UxElement container) : base(
				container,
				container._syntax.Attributes,
				syntax => new UxAttribute(syntax)) { }

			protected override UxAttribute Attach(UxAttribute item)
			{
				if (item.Parent != null)
					item = new UxAttribute(item.Syntax);
				item.Parent = (UxElement) Container;
				return item;
			}

			protected override void SetIndexProperty(UxAttribute item, int index)
			{
				item.AttributeIndex = index;
			}

			protected override int GetIndexProperty(UxAttribute item)
			{
				return item.AttributeIndex;
			}

			protected override void Detach(UxAttribute item)
			{
				item.AttributeIndex = -1;
				item.Parent = null;
			}

			protected override void OnInsert(int index, UxAttribute item)
			{
				base.OnInsert(index, item);
				Container.Changed?.Invoke(new UxInsertAttributeChange(item.Parent.NodePath, item.AttributeIndex, item.Syntax));
			}

			protected override void OnRemove(int index)
			{
				UxRemoveAttributeChange change = null;
				var changed = Container.Changed;
				if (changed != null)
				{
					var item = this[index];
					change = new UxRemoveAttributeChange(item.Parent.NodePath, item.AttributeIndex, item.Syntax);
				}

				base.OnRemove(index);

				if (change != null)
					changed(change);
			}

			protected override void OnReplace(int index, UxAttribute item)
			{
				var changed = Container.Changed;
				UxRemoveAttributeChange change = null;
				if (changed != null)
				{
					var oldItem = this[index];
					change = new UxRemoveAttributeChange(oldItem.Parent.NodePath, oldItem.AttributeIndex, oldItem.Syntax);
				}

				base.OnReplace(index, item);

				if (change != null)
				{
					var insertedItemSyntax = item.Syntax;
					changed(change);
					changed(new UxInsertAttributeChange(change.NodePath, change.AttributeIndex, insertedItemSyntax));
				}
			}
		}
	}
}