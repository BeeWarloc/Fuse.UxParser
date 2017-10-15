using System;
using System.Linq;
using System.Xml;

namespace Fuse.UxParser
{
	public static class UxNodeMergeExtensions
	{
		public static void Merge(this UxDocument dst, UxDocument src)
		{
			if (dst == null) throw new ArgumentNullException(nameof(dst));
			if (src == null) throw new ArgumentNullException(nameof(src));

			MergeChildNodes(dst, src);
		}

		public static void Merge(this UxNode dst, UxNode src)
		{
			if (dst == null) throw new ArgumentNullException(nameof(dst));
			if (src == null) throw new ArgumentNullException(nameof(src));

			if (dst.NodeType != src.NodeType)
				throw new InvalidOperationException("Node type of merge source must be same as destination.");
			switch (src.NodeType)
			{
				case XmlNodeType.Element:
					MergeElement((UxElement) dst, (UxElement) src);
					break;
				case XmlNodeType.Comment:
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
					dst.ReplaceSyntax(src.Syntax);
					break;
				//case XmlNodeType.Document:
				//	Merge(((XDocument)dst).Root, ((XDocument)src).Root);
				//	break;
				default:
					throw new NotImplementedException("Don't know how to merge ");
			}
		}

		static void MergeElement(UxElement dst, UxElement src)
		{
			// TODO: This does not retain NameToken trivia, maybe directly modify token instead?
			if (dst.Name != src.Name)
				dst.Name = src.Name;

			MergeAttributes(dst, src);
			MergeChildNodes(dst, src);

			if (dst.IsEmpty != src.IsEmpty)
				dst.IsEmpty = src.IsEmpty;
		}

		static void MergeChildNodes(IUxContainerInternals dst, IUxContainerInternals src)
		{
			var dstNodes = WorkList.Create(dst);
			var srcNodes = WorkList.Create(src);

			// First skip all _identical_ nodes
			// (doesn't actually merge anything, just removes identical nodes from work list)
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				DeepEquals);

			// First merge all where name matches
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				(dstNode, srcNode) =>
				{
					if (dstNode.NodeType != srcNode.NodeType)
						return false;
					if (dstNode.NodeType == XmlNodeType.Element)
						if (((UxElement) dstNode).Name != ((UxElement) srcNode).Name)
							return false;
					Merge(dstNode, srcNode);
					return true;
				});

			// Then merge all where only node type matches
			MergeNodesBothDirections(
				dstNodes,
				srcNodes,
				(dstNode, srcNode) =>
				{
					if (dstNode.NodeType != srcNode.NodeType)
						return false;
					Merge(dstNode, srcNode);
					return true;
				});

			while (!dstNodes.IsEmpty)
			{
				var node = dstNodes.Pop(Position.Tail);
				node.Remove();
			}

			while (!srcNodes.IsEmpty)
			{
				// We need to know where to insert this.
				var srcNode = srcNodes.Pop(Position.Head);
				var dstNode = CloneNode(srcNode);
				dst.Nodes.Insert(srcNode.NodeIndex, dstNode);
			}
		}

		static UxNode CloneNode(UxNode srcNode)
		{
			return srcNode.DetachedNodeClone();
		}

		static void MergeNodesBothDirections<T>(WorkList<T> dst, WorkList<T> src, Func<T, T, bool> merge) where T : class
		{
			// Merge as far as we can down using the given merge strategy
			MergeNodeSequenceUntilStrategyFails(dst, src, Position.Head, merge);

			// Then merge backwards..
			MergeNodeSequenceUntilStrategyFails(dst, src, Position.Tail, merge);
		}

		static void MergeNodeSequenceUntilStrategyFails<T>(
			WorkList<T> dst,
			WorkList<T> src,
			Position popPos,
			Func<T, T, bool> merge) where T : class
		{
			while (!dst.IsEmpty && !src.IsEmpty)
			{
				var srcNode = src.Peek(popPos);
				var dstNode = dst.Peek(popPos);
				if (merge(dstNode, srcNode))
				{
					dst.Pop(popPos);
					src.Pop(popPos);
				}
				else
				{
					break;
				}
			}
		}

		static void MergeAttributes(UxElement dst, UxElement src)
		{
			// Since we have better control over ordering of UxAttribute(s) than XAttribute
			// we could change this.

			// However it's not really that bad to say an attribute is removed and added instead
			// of changed after being reordered, since it's not that common.
			var matchCount = 0;
			foreach (var pair in dst.Attributes.Zip(src.Attributes, (dstAttr, srcAttr) => new { dstAttr, srcAttr }))
			{
				var dstAttr = pair.dstAttr;
				var srcAttr = pair.srcAttr;

				if (dstAttr.Name != srcAttr.Name)
					break;

				if (!dstAttr.Syntax.Equals(srcAttr.Syntax))
					dstAttr.ReplaceSyntax(srcAttr.Syntax);

				matchCount++;
			}

			// Remove all attributes below the matching ones
			foreach (var dstAttr in dst.Attributes.Skip(matchCount).ToArray())
				dstAttr.Remove();

			foreach (var srcAttr in src.Attributes.Skip(matchCount))
				dst.Attributes.Add(new UxAttribute(srcAttr.Syntax));
		}

		public static bool DeepEquals(this UxDocument a, UxDocument b)
		{
			return a.Syntax.Equals(b.Syntax);
		}

		public static bool DeepEquals(this UxNode a, UxNode b)
		{
			return a.Syntax.Equals(b.Syntax);
		}

		enum Position
		{
			Head,
			Tail
		}

		static class WorkList
		{
			public static WorkList<UxNode> Create(IUxContainerInternals container)
			{
				return new WorkList<UxNode>(
					container.Nodes.FirstOrDefault(),
					container.Nodes.LastOrDefault(),
					x => x.NextNode,
					x => x.PreviousNode);
			}
		}

		class WorkList<T>
			where T : class
		{
			readonly Func<T, T> _next;
			readonly Func<T, T> _prev;
			T _head;
			T _tail;

			public WorkList(T head, T tail, Func<T, T> next, Func<T, T> prev)
			{
				_head = head;
				_tail = tail;
				_next = next;
				_prev = prev;
			}

			public bool IsEmpty => _head == null;

			public T Peek(Position position)
			{
				if (_head == null)
					throw new InvalidOperationException("List is empty.");
				return position == Position.Head ? _head : _tail;
			}

			public T Pop(Position position)
			{
				if (IsEmpty)
					throw new InvalidOperationException("Nothing left to remove");

				// When we only have one element left we're empty.
				T node;
				if (_head == _tail)
				{
					node = _head;
					_head = null;
					_tail = null;
				}
				else if (position == Position.Head)
				{
					node = _head;
					_head = _next(node);
					if (_head == null)
						throw new InvalidOperationException(
							"Inconsistency detected. There should be a next node at this point, as we've already checked that the list is not of size 0");
				}
				else
				{
					node = _tail;
					_tail = _prev(node);
					if (_tail == null)
						throw new InvalidOperationException(
							"Inconsistency detected. There should be a next node at this point, as we've already checked that the list is not of size 0");
				}
				return node;
			}
		}
	}
}