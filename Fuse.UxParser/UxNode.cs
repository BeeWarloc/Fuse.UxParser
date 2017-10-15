using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using Fuse.UxParser.Syntax;

namespace Fuse.UxParser
{
	public abstract class UxNode : UxObject
	{
		int _index = -1;
		internal IUxContainerInternals Container { get; private set; }

		// Needs to be updated by list wrapper on insertions and removals
		public int NodeIndex
		{
			get
			{
				// TODO: This is for debugging until tests for index updates are done
				var calculatedIndex = Container?.Nodes.IndexOf(this) ?? -1;
				if (calculatedIndex != _index)
					throw new InvalidOperationException("Index updates have failed");
				// END

				return _index;
			}
			private set => _index = value;
		}

		// depth 0 means a single element
		public int Depth => Parent?.Depth + 1 ?? 0;

		public int FullSpan => Syntax.FullSpan;

		public UxElement Parent => Container as UxElement;
		public UxDocument Document => Container as UxDocument ?? Parent?.Document;

		public UxNode NextNode => Container?.Nodes.ElementAtOrDefault(NodeIndex + 1);
		public UxNode PreviousNode => NodeIndex > 0 ? Container?.Nodes[NodeIndex - 1] : null;

		public UxNodePath NodePath => UxNodePath.From(this);

		public abstract NodeSyntax Syntax { get; }
		public abstract XmlNodeType NodeType { get; }

		public int SourceOffset
		{
			get
			{
				if (PreviousNode != null)
					return PreviousNode.SourceOffset + PreviousNode.Syntax.FullSpan;
				if (Container != null)
					return Container.NodesSourceOffset;
				return 0;
			}
		}

		internal virtual void SetDirty()
		{
			Container?.SetDirty();
		}

		public static UxNode FromSyntax(NodeSyntax arg)
		{
			switch (arg)
			{
				case ElementSyntaxBase element:
					return new UxElement(element);
				case CommentSyntax comment:
					return new UxComment(comment);
				case TextSyntax text:
					return new UxText(text);
				case CDataSyntax cdata:
					return new UxCData(cdata);
				default:
					throw new NotImplementedException();
			}
		}

		internal abstract UxNode DetachedNodeClone();

		public bool Remove()
		{
			if (Container == null)
				return false;
			Container.Nodes.Remove(this);
			return true;
		}

		public override string ToString()
		{
			return Syntax.ToString();
		}

		public IEnumerable<UxNode> DescendantNodesAndSelf()
		{
			return GetDescendantNodes(true);
		}

		public IEnumerable<UxNode> DescendantNodes()
		{
			return GetDescendantNodes(false);
		}

		public void AddAfterSelf(UxNode dstNode)
		{
			EnsureAttached();
			Container.Nodes.Insert(NodeIndex + 1, dstNode);
		}

		void EnsureAttached()
		{
			if (Container == null)
				throw new InvalidOperationException("TODO PROPER ERROR");
		}

		internal virtual IEnumerable<UxNode> GetDescendantNodes(bool self)
		{
			return Enumerable.Empty<UxNode>();
		}


		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		internal class NodeList : WrapperList<UxNode, NodeSyntax>
		{
			public NodeList(IUxContainerInternals container, IImmutableList<NodeSyntax> initialSyntaxList) : base(
				container,
				initialSyntaxList,
				FromSyntax) { }

			protected override UxNode Attach(UxNode item)
			{
				if (item.Container != null)
					item = item.DetachedNodeClone();
				item.Container = Container;
				return item;
			}

			protected override void SetIndexProperty(UxNode item, int index)
			{
				item.NodeIndex = index;
			}

			protected override int GetIndexProperty(UxNode item)
			{
				return item.NodeIndex;
			}

			protected override void Detach(UxNode item)
			{
				item.NodeIndex = -1;
				item.Container = null;
			}

			protected override void OnInsert(int index, UxNode item)
			{
				base.OnInsert(index, item);

				var changed = Container.Changed;
				if (changed != null)
					changed(new UxInsertNodeChange(item.NodePath, item.Syntax));
			}

			protected override void OnRemove(int index)
			{
				var changed = Container.Changed;
				UxRemoveNodeChange change = null;
				if (changed != null)
				{
					var removed = this[index];
					change = new UxRemoveNodeChange(removed.NodePath, removed.Syntax);
				}

				base.OnRemove(index);

				if (change != null)
					changed(change);
			}

			protected override void OnReplace(int index, UxNode item)
			{
				NodeSyntax oldSyntax = null;
				var changed = Container.Changed;
				if (changed != null)
					oldSyntax = this[index].Syntax;

				base.OnReplace(index, item);

				if (oldSyntax != null)
				{
					var path = item.NodePath;
					changed(new UxRemoveNodeChange(path, oldSyntax));
					changed(new UxInsertNodeChange(path, item.Syntax));
				}
			}
		}

		internal abstract class WrapperList<TItem, TSyntax> : IList<TItem>
		{
			readonly IList<TItem> _inner;

			protected WrapperList(
				IUxContainerInternals container,
				IEnumerable<TSyntax> initialSyntaxList,
				Func<TSyntax, TItem> itemCtor)
			{
				Container = container;
				_inner = initialSyntaxList.Select((syntax, i) => Attach(itemCtor(syntax), i)).ToList();
			}

			protected IUxContainerInternals Container { get; }


			public IEnumerator<TItem> GetEnumerator()
			{
				return _inner.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) _inner).GetEnumerator();
			}

			public void Add(TItem item)
			{
				Insert(_inner.Count, item);
			}


			public void Clear()
			{
				if (_inner.Count == 0)
					return;

				foreach (var item in _inner)
					Detach(item);
				_inner.Clear();
			}


			public bool Contains(TItem item)
			{
				return _inner.Contains(item);
			}

			public void CopyTo(TItem[] array, int arrayIndex)
			{
				_inner.CopyTo(array, arrayIndex);
			}

			public bool Remove(TItem item)
			{
				var index = IndexOf(item);
				if (index == -1)
					return false;
				RemoveAt(index);
				return true;
			}

			public int Count => _inner.Count;

			public bool IsReadOnly => _inner.IsReadOnly;

			public int IndexOf(TItem item)
			{
				return _inner.IndexOf(item);
			}

			public void Insert(int index, TItem item)
			{
				if (index < 0 || index > _inner.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				OnInsert(index, item);
			}

			public void RemoveAt(int index)
			{
				if (index < 0 || index >= _inner.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				OnRemove(index);
			}

			public TItem this[int index]
			{
				get => _inner[index];
				set
				{
					if (index < 0 || index >= _inner.Count)
						throw new ArgumentOutOfRangeException(nameof(index));
					OnReplace(index, value);
				}
			}

			void UpdateIndexes(int startIndex)
			{
				for (var i = startIndex; i < _inner.Count; i++)
					SetIndexProperty(_inner[i], i);
			}

			TItem Attach(TItem item, int index)
			{
				item = Attach(item);
				SetIndexProperty(item, index);
				return item;
			}

			protected abstract TItem Attach(TItem item);
			protected abstract void Detach(TItem item);

			protected abstract void SetIndexProperty(TItem item, int index);
			protected abstract int GetIndexProperty(TItem item);

			protected virtual void OnInsert(int index, TItem item)
			{
				_inner.Insert(index, Attach(item, index));
				UpdateIndexes(index + 1);
				SetDirty();
			}

			protected virtual void OnRemove(int index)
			{
				var item = _inner[index];
				_inner.RemoveAt(index);
				Detach(item);
				UpdateIndexes(index);
				SetDirty();
			}

			protected virtual void OnReplace(int index, TItem item)
			{
				var old = _inner[index];
				if (Equals(old, item))
					return;

				_inner[index] = Attach(item, index);
				Detach(old);
				SetDirty();
			}

			void SetDirty()
			{
				// NOCOMMIT! for debugging!!!

				for (var i = 0; i < _inner.Count; i++)
					if (GetIndexProperty(_inner[i]) != i)
						throw new InvalidOperationException("Ids not properly updated..");

				// NOCOMMIT! end debugging!

				Container.SetDirty();
			}
		}

		public abstract void ReplaceSyntax(NodeSyntax newSyntax);
	}
}