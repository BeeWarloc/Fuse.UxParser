using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Xml;
using System.Xml.Linq;

namespace XmlExperimentation
{
	public class XmlChangeListener : IDisposable
	{
		readonly Dictionary<XAttribute, string> _attributeOldValueMap = new Dictionary<XAttribute, string>();
		readonly ReplaySubject<XmlChange> _changes = new ReplaySubject<XmlChange>();
		readonly Dictionary<XNode, XmlRemoveNodeChange> _pendingRemovalEvents = new Dictionary<XNode, XmlRemoveNodeChange>();
		XDocument _document;

		public XmlChangeListener(XDocument document)
		{
			_document = document;
			_document.Changing += OnChanging;
			_document.Changed += OnChanged;
		}

		public IObservable<XmlChange> Changes => _changes;

		public void Dispose()
		{
			_document = null;
			_changes.Dispose();
		}

		void OnChanging(object sender, XObjectChangeEventArgs args)
		{
			CheckDisposed();
			var xobj = (XObject) sender;
			switch (xobj.NodeType)
			{
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.Comment:
				case XmlNodeType.Element:
					OnNodeChanging((XNode) sender, args);
					break;
				case XmlNodeType.Attribute:
					OnAttributeChanging((XAttribute) sender, args);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		void OnChanged(object sender, XObjectChangeEventArgs args)
		{
			CheckDisposed();
			var xobj = (XObject) sender;
			switch (xobj.NodeType)
			{
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.Comment:
				case XmlNodeType.Element:
					OnNodeChanged((XNode) sender, args);
					break;
				case XmlNodeType.Attribute:
					OnAttributeChanged((XAttribute) sender, args);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		void OnAttributeChanging(XAttribute attribute, XObjectChangeEventArgs args)
		{
			switch (args.ObjectChange)
			{
				case XObjectChange.Remove:
					throw new NotImplementedException();
				case XObjectChange.Value:
					_attributeOldValueMap[attribute] = attribute.Value;
					break;
			}
		}

		void OnAttributeChanged(XAttribute attribute, XObjectChangeEventArgs args)
		{
			switch (args.ObjectChange)
			{
				case XObjectChange.Add:
					_changes.OnNext(new XmlAddAttributeChange(attribute.Parent.NodePath(), attribute.Name, attribute.Value));
					break;

				case XObjectChange.Remove:
					throw new NotImplementedException();

				case XObjectChange.Name:
					throw new NotSupportedException("Name of XAttribute is expected to be immutable.");

				case XObjectChange.Value:
					_changes.OnNext(
						new XmlAttributeValueChange(
							attribute.Parent.NodePath(),
							attribute.Name,
							attribute.Value,
							_attributeOldValueMap[attribute]));
					break;
			}
		}

		void OnNodeChanging(XNode node, XObjectChangeEventArgs args)
		{
			if (args.ObjectChange == XObjectChange.Remove)
			{
				// TODO May not want to actually emit the event before Changed has also run
				if (node.Parent == null)
					throw new InvalidOperationException("Removed event only expected for non-root nodes");
				_pendingRemovalEvents[node] = new XmlRemoveNodeChange(node.NodePath(), ImmutableXNode.Create(node));
			}
		}

		void OnNodeChanged(XNode node, XObjectChangeEventArgs args)
		{
			switch (args.ObjectChange)
			{
				case XObjectChange.Add:
					_changes.OnNext(new XmlAddNodeChange(node.NodePath(), ImmutableXNode.Create(node)));
					break;
				case XObjectChange.Remove:
					XmlRemoveNodeChange change;
					if (!_pendingRemovalEvents.TryGetValue(node, out change))
						throw new InvalidOperationException("Element removed, but parent not found in ");
					_changes.OnNext(change);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		void CheckDisposed()
		{
			if (_document == null)
				throw new ObjectDisposedException(GetType().FullName);
		}
	}
}