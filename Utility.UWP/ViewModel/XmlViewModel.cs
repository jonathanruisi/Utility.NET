using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using JLR.Utility.NETFramework;

namespace JLR.Utility.UWP.ViewModel
{
	/// <summary>
	/// <inheritdoc cref="ViewModel"/>
	/// These types are capable of serialization to and deserialization from XML.
	/// </summary>
	public abstract class XmlViewModel : ViewModel, IXmlSerializable
	{
		#region Static Members
		protected static readonly string ViewModelFlagName = "ViewModelFlag";

		protected static readonly Dictionary<Type, string>               SerializationInfo;
		protected static readonly Dictionary<string, Func<XmlViewModel>> DeserializationInfo;

		static XmlViewModel()
		{
			if (SerializationInfo == null)
				SerializationInfo = new Dictionary<Type, string>();
			if (DeserializationInfo == null)
				DeserializationInfo = new Dictionary<string, Func<XmlViewModel>>();
		}

		/// <summary>
		/// Instantiates an <see cref="XmlViewModel"/> object based on its XML representation.
		/// </summary>
		/// <param name="reader">
		/// The <see cref="XmlReader"/> stream from which the object is deserialized.
		/// </param>
		/// <returns>
		/// Fully initialized <see cref="XmlViewModel"/> subclass.
		/// </returns>
		/// <remarks>
		/// It is necessary to call <see cref="RegisterXmlSerializationInfo{T}"/>
		/// for each <see cref="XmlViewModel"/> type once per application instance.
		/// </remarks>
		public static XmlViewModel ReadFrom(XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			reader.MoveToElement();
			if (reader.IsEmptyElement)
				throw new XmlException("Cannot deserialize an XmlViewModel from an empty XML element");

			ValidateTypeForXmlDeserialization(reader.Name);

			var element = DeserializationInfo[reader.Name]();
			element.ReadXml(reader);
			return element;
		}

		/// <summary>
		/// Registers an <see cref="XmlViewModel"/> type for serialization
		/// by associating it with an XML element name.
		/// This allows the type to be deserialized and instantiated
		/// using its XML representation.
		/// </summary>
		/// <typeparam name="T">
		/// The <see cref="XmlViewModel"/> type being registered.
		/// </typeparam>
		/// <param name="xmlName">
		/// The XML element name that will be used to identify this type.
		/// </param>
		/// <remarks>
		/// A 1-to-1 relationship between an <see cref="XmlViewModel"/>
		/// type and its XML element name is enforced by this mechanism.
		/// </remarks>
		public static void RegisterXmlSerializationInfo<T>(string xmlName)
			where T : XmlViewModel, new()
		{
			var type = typeof(T);

			// Enforce 1-to-1 relationship between XML name and XmlNodeViewModel type
			if (SerializationInfo.ContainsKey(type) || SerializationInfo.ContainsValue(xmlName))
				return;

			SerializationInfo.Add(type, xmlName);
			DeserializationInfo.Add(xmlName, ExpressionHelper.GetConstructor<T>());
		}

		/// <summary>
		/// Disassociates an <see cref="XmlViewModel"/> type
		/// from an XML element name.
		/// </summary>
		/// <typeparam name="T">
		/// The <see cref="XmlViewModel"/> type being unregistered.
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

		/// <summary>
		/// Throws an exception of the specified <paramref name="type"/> is not registered.
		/// </summary>
		/// <param name="type">The type to be validated.</param>
		protected static void ValidateTypeForXmlSerialization(Type type)
		{
			if (SerializationInfo.ContainsKey(type))
				return;

			throw new InvalidOperationException(
				$"The XmlViewModel type \"{type.Name}\" has not been registered for XML serialization");
		}

		/// <summary>
		/// Throws an exception of the specified <paramref name="xmlName"/> is not registered.
		/// </summary>
		/// <param name="xmlName">The XML element name to be validated.</param>
		protected static void ValidateTypeForXmlDeserialization(string xmlName)
		{
			if (DeserializationInfo.ContainsKey(xmlName))
				return;

			throw new XmlException(
				$"The XML element name \"{xmlName}\" has not been registered for XML serialization");
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// is called to interpret any attributes present in this object's XML representation.
		/// </summary>
		/// <param name="name">The name of the attribute.</param>
		/// <param name="value">The attribute's value represented as a string.</param>
		protected abstract void ReadAttribute(string name, string value);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// reads any elements (class members) present in this object's XML representation.
		/// </summary>
		/// <param name="name">The name of the element.</param>
		/// <param name="reader">
		/// The <see cref="XmlReader"/> stream from which the object is deserialized.
		/// </param>
		protected abstract void ReadElement(string name, XmlReader reader);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// writes any attributes required by this object's XML representation.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="XmlWriter"/> stream to which the object is serialized.
		/// </param>
		protected abstract void WriteAttributes(XmlWriter writer);

		/// <summary>
		/// Callback method that, when overridden in a derived class,
		/// writes any elements (class members) required by this object's XML representation.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="XmlWriter"/> stream to which the object is serialized.
		/// </param>
		protected abstract void WriteElements(XmlWriter writer);

		/// <summary>
		/// Callback method which is invoked if a recognized (registered)
		/// <see cref="XmlViewModel"/> element is found during deserialization.
		/// The element is initialized and passed to this method for further action.
		/// </summary>
		/// <param name="element">
		/// A fully initialized <see cref="XmlViewModel"/> object.
		/// </param>
		/// <param name="flag">
		/// A string containing <see cref="XmlViewModel"/>-specific information.
		/// </param>
		protected abstract void ProcessMemberElement(XmlViewModel element, string flag);
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
					if (reader.Name != ViewModelFlagName)
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

			// Deserialize elements
			while (reader.NodeType == XmlNodeType.Element)
			{
				// If the element's name is registered, deserialize the element
				// to an XmlViewModel and hand it off to the subclass for use.
				// If not, the subclass will deserialize the element.
				if (reader.Name == name || DeserializationInfo.ContainsKey(reader.Name))
				{
					// Check for XmlViewModel-specific attribute
					string flag = null;
					if (reader.MoveToAttribute(ViewModelFlagName))
						flag = reader.Value;

					// Deserialize the element
					reader.MoveToElement();
					var element = DeserializationInfo[reader.Name]();
					element.ReadXml(reader);
					ProcessMemberElement(element, flag);
				}
				else
				{
					ReadElement(reader.Name, reader);
				}
			}

			// Read end tag
			reader.ReadEndElement();
		}

		public virtual void WriteXml(XmlWriter writer)
		{
			// Verify that this type is registered for XML serialization
			var type = GetType();
			ValidateTypeForXmlSerialization(type);

			// Write start tag
			writer.WriteStartElement(SerializationInfo[type]);

			// Serialize the object
			WriteAttributes(writer);
			WriteElements(writer);

			// Write end tag
			writer.WriteEndElement();
		}
		#endregion
	}
}