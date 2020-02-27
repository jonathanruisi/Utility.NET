using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using JLR.Utility.NET;

namespace JLR.Utility.UWP.ViewModel
{
	/// <summary>
	/// Represents a node in a hierarchical tree of <see cref="NodeViewModel"/> objects,
	/// capable of serialization to and deserialization from XML.
	/// </summary>
	public abstract class XmlNodeViewModel : NodeViewModel, IXmlSerializable
	{
		#region Static Members
		private static readonly Dictionary<Type, string>                   SerializationInfo;
		private static readonly Dictionary<string, Func<XmlNodeViewModel>> DeserializationInfo;

		static XmlNodeViewModel()
		{
			if (SerializationInfo == null)
				SerializationInfo = new Dictionary<Type, string>();
			if (DeserializationInfo == null)
				DeserializationInfo = new Dictionary<string, Func<XmlNodeViewModel>>();
		}

		/// <summary>
		/// Instantiates an <see cref="XmlNodeViewModel"/> subclass
		/// based on its XML representation.
		/// </summary>
		/// <param name="reader">
		/// The <see cref="XmlReader"/> stream from which the node is deserialized.
		/// </param>
		/// <returns>
		/// Fully initialized <see cref="XmlNodeViewModel"/> subclass.
		/// </returns>
		/// <remarks>
		/// It is necessary to call <see cref="RegisterXmlSerializationInfo{T}"/>
		/// for each derived type once per application instance.
		/// </remarks>
		public static XmlNodeViewModel ReadFrom(XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			reader.MoveToElement();
			if (reader.IsEmptyElement)
				throw new XmlException("Cannot create an XmlNodeViewModel from an empty XML node");

			ValidateTypeForXmlDeserialization(reader.Name);

			var node = DeserializationInfo[reader.Name]();
			node.ReadXml(reader);
			return node;
		}

		/// <summary>
		/// Registers an <see cref="XmlNodeViewModel"/> type for serialization
		/// by associating its type with an XML element name.
		/// This allows the type to be deserialized and instantiated
		/// using its XML representation.
		/// </summary>
		/// <typeparam name="T">
		/// The <see cref="XmlNodeViewModel"/> type being registered.
		/// </typeparam>
		/// <param name="xmlName">
		/// The XML element name that will be used to identify this type.
		/// </param>
		/// <remarks>
		/// A 1-to-1 relationship between an <see cref="XmlNodeViewModel"/>
		/// type and its XML element name is enforced by this mechanism.
		/// </remarks>
		public static void RegisterXmlSerializationInfo<T>(string xmlName)
			where T : XmlNodeViewModel, new()
		{
			var type = typeof(T);

			// Enforce 1-to-1 relationship between XML name and XmlNodeViewModel type
			if (SerializationInfo.ContainsKey(type) || SerializationInfo.ContainsValue(xmlName))
				return;

			SerializationInfo.Add(type, xmlName);
			DeserializationInfo.Add(xmlName, ExpressionHelper.GetConstructor<T>());
		}

		/// <summary>
		/// Disassociates an <see cref="XmlNodeViewModel"/> type from the
		/// name used in its XML representation.
		/// </summary>
		/// <typeparam name="T">
		/// The <see cref="XmlNodeViewModel"/> type being unregistered.
		/// </typeparam>
		public static void UnregisterXmlSerializationInfo<T>()
		{
			var type = typeof(T);
			if (!SerializationInfo.ContainsKey(type))
				return;

			var name = SerializationInfo[type];
			SerializationInfo.Remove(type);
			DeserializationInfo.Remove(name);
		}

		private static void ValidateTypeForXmlSerialization(Type type)
		{
			if (SerializationInfo.ContainsKey(type))
				return;

			throw new InvalidOperationException(
				$"The XmlNodeViewModel type \"{type.Name}\" has not been registered for XML serialization");
		}

		private static void ValidateTypeForXmlDeserialization(string xmlName)
		{
			if (DeserializationInfo.ContainsKey(xmlName))
				return;

			throw new XmlException(
				$"The XML name \"{xmlName}\" has not been registered for XML deserialization");
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// is called to interpret any attributes present in this node's XML representation.
		/// </summary>
		/// <param name="name">The name of the attribute</param>
		/// <param name="value">The attribute's value represented as a string</param>
		protected abstract void ReadAttribute(string name, string value);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// is called to read any non-child elements present in this node's XML representation.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="reader">The <see cref="XmlReader"/> stream from which the node is deserialized.</param>
		protected abstract void ReadElement(string name, XmlReader reader);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// writes any attributes required by this node's XML representation.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="XmlWriter"/> stream to which the node is serialized.
		/// </param>
		protected abstract void WriteAttributes(XmlWriter writer);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// writes any non-child elements required by this node's XML representation.
		/// <b>Do not</b> use this method to serialize <see cref="XmlNodeViewModel"/> child nodes
		/// (this is handled automatically).
		/// </summary>
		/// <param name="writer">
		/// The <see cref="XmlWriter"/> stream to which the node is serialized.
		/// </param>
		protected abstract void WriteElements(XmlWriter writer);

		/// <summary>
		/// Callback method which is invoked during deserialization if an
		/// <see cref="XmlNodeViewModel"/> node is found and it is determined
		/// that this node is not a child of the element in which it was found.
		/// It is therefore the responsibility of that element to determine its use.
		/// </summary>
		/// <param name="node">
		/// A fully initialized <see cref="XmlNodeViewModel"/> node.
		/// </param>
		protected abstract void MemberNodeAction(XmlNodeViewModel node);
		#endregion

		#region Interface Implementation (IXmlSerializable)
		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			// Deserialize attributes
			if (reader.MoveToFirstAttribute())
			{
				do
				{
					if (reader.Name != "IsChild")
						ReadAttribute(reader.Name, reader.Value);
				} while (reader.MoveToNextAttribute());
			}

			// Read start tag and return if this element is empty
			reader.MoveToElement();
			var isEmpty = reader.IsEmptyElement;
			var name    = reader.Name;
			reader.ReadStartElement();
			if (isEmpty)
				return;

			// Deserialize both child and non-child elements
			while (reader.NodeType == XmlNodeType.Element)
			{
				var isChild            = false;
				var isXmlNodeViewModel = false;

				if (reader.Name == name)
				{
					// Element is the same XmlNodeViewModel type as this node
					isChild            = true;
					isXmlNodeViewModel = true;
				}
				else if (reader.MoveToFirstAttribute() && reader.Name == "IsChild")
				{
					// Element is a derivative of XmlNodeViewModel
					// and may or may not be a child in this node's hierarchy.
					isXmlNodeViewModel = true;
					isChild            = reader.Value == "True";
					reader.MoveToElement();
				}

				// Deserialize the XmlNodeViewModel node
				if (isXmlNodeViewModel)
				{
					ValidateTypeForXmlDeserialization(reader.Name);
					var node = DeserializationInfo[reader.Name]();
					node.ReadXml(reader);

					// If the node is a child, add it to the collection of child nodes.
					// Otherwise it is passed to, and assumed to be a member of, the subclass. 
					if (isChild)
						Children.Add(node);
					else
						MemberNodeAction(node);
				}
				else
				{
					// Subclass reads element's contents
					ReadElement(reader.Name, reader);
				}
			}

			// Read end tag
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			WriteXml(writer, false);
		}

		/// <summary>
		/// Converts an <see cref="XmlNodeViewModel"/> into its XML representation
		/// using the string associated with its type as the XML element name.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="XmlWriter"/> stream to which the node is serialized.
		/// </param>
		/// <param name="includeStartEndTags">
		/// If <b>true</b>, this node's XML start/end tags will be written automatically.
		/// </param>
		/// <param name="omitIsChildAttribute">
		/// If <b>true</b>, this node will not be marked with an <c>IsChild</c> attribute
		/// regardless of its relationship to other <see cref="XmlNodeViewModel"/> nodes
		/// in a given hierarchy.
		/// </param>
		public void WriteXml(XmlWriter writer, bool includeStartEndTags, bool omitIsChildAttribute = false)
		{
			// Verify that this node's type is registered for XML serialization
			var type = GetType();
			ValidateTypeForXmlSerialization(type);

			// Write start tag (if applicable)
			if (includeStartEndTags)
				writer.WriteStartElement(SerializationInfo[type]);

			if (!omitIsChildAttribute)
			{
				// If this is a root node, explicitly indicate it is not a child.
				// This allows for derived classes to include members of their own
				// type as serializable properties which are not members of its hierarchy.
				// If this is a child node and its parent is a different type,
				// explicitly indicate that it is a child.
				if (this == Root && Children.Count == 0)
					writer.WriteAttributeString("IsChild", "False");
				else if (Parent != null &&
						 SerializationInfo[Parent.GetType()]
					  != SerializationInfo[type])
					writer.WriteAttributeString("IsChild", "True");
			}

			// Serialize this node's attributes and non-child elements
			WriteAttributes(writer);
			WriteElements(writer);

			// Recursively serialize all child nodes
			foreach (var child in Children.OfType<XmlNodeViewModel>())
			{
				child.WriteXml(writer, true);
			}

			// Write end tag (if applicable)
			if (includeStartEndTags)
				writer.WriteEndElement();
		}
		#endregion
	}
}