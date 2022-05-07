using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using JLR.Utility.NET.Reflection;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

using Windows.Storage;

namespace JLR.Utility.WinUI.ViewModel
{
    /// <summary>
    /// <inheritdoc/>
    /// <see cref="ViewModelElement"/> is also capable of
    /// fully automatic XML serialization and deserialization.
    /// </summary>
    public abstract class ViewModelElement : ObservableRecipient, IXmlSerializable
    {
        #region Fields
        private string _name;
        private static bool IsSubclassInfoLoaded = false;
        private static Dictionary<Type, ViewModelSerializationInfo> SerializationInfo;
        private static Dictionary<string, Type> DeserializationInfo;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        [ViewModelObject(nameof(Name), XmlNodeType.Attribute)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, true);
        }

        /// <summary>
        /// Gets or sets a value that controls whether or not empty collections
        /// are to be serialized to XML in the form of an empty element.
        /// </summary>
        public bool WriteEmptyCollectionElements { get; set; }
        #endregion

        #region Constructors
        static ViewModelElement()
        {
            if (SerializationInfo == null)
                SerializationInfo = new Dictionary<Type, ViewModelSerializationInfo>();
            if (DeserializationInfo == null)
                DeserializationInfo = new Dictionary<string, Type>();

            if (!IsSubclassInfoLoaded)
            {
                foreach (var subclassType in ReflectiveEnumerator.GetSubclassTypes<ViewModelElement>())
                {
                    var info = ViewModelSerializationInfo.Create(subclassType);
                    SerializationInfo.Add(subclassType, info);
                    DeserializationInfo.Add(info.XmlName, subclassType);
                }

                IsSubclassInfoLoaded = true;
            }
        }

        protected ViewModelElement() : base(StrongReferenceMessenger.Default)
        {
            _name = string.Empty;
            WriteEmptyCollectionElements = false;
        }
        #endregion

        #region Public Methods
        public static async Task<ViewModelElement> ReadFromAsync(StorageFile file)
        {
            if (file == null || !file.IsAvailable)
                return null;

            XmlReader reader = null;
            ViewModelElement result = null;

            try
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true
                };

                reader = XmlReader.Create(await file.OpenStreamForReadAsync(), settings);
                reader.MoveToContent();
                result = ReadFrom(reader);
            }
            finally
            {
                reader?.Close();
            }

            return result;
        }

        /// <summary>
        /// Instantiates an <see cref="ViewModelElement"/> from its XML representation.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="XmlReader"/> stream from which the object is to be deserialized.
        /// </param>
        /// <returns>
        /// Fully instantiated <see cref="ViewModelElement"/> subclass.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static ViewModelElement ReadFrom(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.IsEmptyElement ||
               (reader.NodeType != XmlNodeType.Attribute &&
                reader.NodeType != XmlNodeType.Element))
            {
                throw new InvalidOperationException(
                    "The current XML element is either empty or invalid");
            }

            if (!DeserializationInfo.ContainsKey(reader.Name))
            {
                throw new InvalidOperationException(
                    $"The current XML element \"{reader.Name}\" is not recognized and/or cannot be instantiated");
            }

            var element = SerializationInfo[DeserializationInfo[reader.Name]].Constructor();
            element.ReadXml(reader);
            return element;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// When overridden in a derived class, this method is called
        /// immediately after XML deserialization, providing an opportunity
        /// to perform any required processing.
        /// </summary>
        protected virtual void OnReadXmlComplete() { }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// immediately before XML serialization, providing an opportunity
        /// to perform any required pre-processing.
        /// </summary>
        protected virtual void OnWriteXml() { }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// during XML deserialization when parsing a property
        /// marked with <see cref="ViewModelObjectAttribute.UseCustomParser"/>
        /// = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">The name of the property to parse.</param>
        /// <param name="content">The property's content as read from XML.</param>
        /// <remarks>
        /// Classes that override this function are only responsible for parsing
        /// the content passed to it. Do not set the property directly.
        /// Simply return the fully constructed property value.
        /// </remarks>
        protected virtual object CustomPropertyParser(string propertyName, string content)
        {
            return null;
        }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// during XML serialization when writing a property
        /// marked with <see cref="ViewModelObjectAttribute.UseCustomWriter"/>
        /// = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">
        /// The <b><u>XML name</u></b> of the property to be written.
        /// </param>
        /// <param name="value">The object to be written.</param>
        /// <returns>
        /// A string representation of the object.
        /// </returns>
        /// <remarks>
        /// Use this method when a property is not a <see cref="ViewModelElement"/>,
        /// and <see cref="object.ToString"/> does not adequately represent the property.
        /// </remarks>
        protected virtual string CustomPropertyWriter(string propertyName, object value)
        {
            return null;
        }
        #endregion

        #region Interface Implementation (IXmlSerializable)
        /// <inheritdoc cref="IXmlSerializable.GetSchema()"/>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <inheritdoc cref="IXmlSerializable.ReadXml(XmlReader)"/>
        public void ReadXml(XmlReader reader)
        {
            ViewModelSerializationInfo.ViewModelSerializationCollectionInfo collectionInfo = null;
            var info = SerializationInfo[GetType()];

            // Read start tag and return if this element is empty
            reader.MoveToElement();
            var isEmpty = reader.IsEmptyElement;
            var name = reader.Name;
            if (isEmpty)
            {
                reader.ReadStartElement();
                return;
            }

            // Deserialize attributes
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    var propertyInfo = info.MemberProperties[reader.Name];
                    var value = propertyInfo.UseCustomParser
                        ? CustomPropertyParser(reader.Name, reader.ReadContentAsString())
                        : reader.ReadContentAs(propertyInfo.PropertyType, null);

                    if (value == null)
                    {
                        throw new InvalidOperationException(
                            $"Unable to parse the attribute \"{reader.Name}\" " +
                            $"in element \"{name}\" " +
                            $"to type \"{propertyInfo.PropertyType.Name}\".");
                    }

                    propertyInfo.Setter(this, value);
                } while (reader.MoveToNextAttribute());
            }

            // Deserialize elements
            reader.ReadStartElement();
            while (reader.NodeType == XmlNodeType.Element ||
                  (collectionInfo != null && reader.NodeType == XmlNodeType.EndElement))
            {
                var elementName = reader.Name;

                // Element is empty, advance to the next XML node
                if (reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    continue;
                }

                // Currently processing a collection
                if (collectionInfo != null)
                {
                    // Element is an item in the collection
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (collectionInfo.ChildType.IsSubclassOf(typeof(ViewModelElement)))
                        {
                            var element = SerializationInfo[DeserializationInfo[elementName]].Constructor();
                            element.ReadXml(reader);
                            collectionInfo.Getter(this).Add(element);
                        }
                        else
                        {
                            var value = collectionInfo.UseCustomParser
                                ? CustomPropertyParser(collectionInfo.XmlChildName, reader.ReadElementContentAsString())
                                : reader.ReadElementContentAs(collectionInfo.ChildType, null);

                            if (value == null)
                            {
                                throw new InvalidOperationException(
                                    $"Unable to parse node \"{reader.Name}\" " +
                                    $"in element \"{elementName}\" " +
                                    $"to type \"{collectionInfo.ChildType.Name}\".");
                            }

                            collectionInfo.Getter(this).Add(value);
                        }
                    }
                    // Element marks the end of the collection
                    else
                    {
                        reader.ReadEndElement();
                        collectionInfo = null;
                    }

                    continue;
                }

                // Element marks the beginning of a known collection
                if (info.MemberCollections.ContainsKey(elementName))
                {
                    collectionInfo = info.MemberCollections[elementName];
                    reader.ReadStartElement();
                    continue;
                }

                // Element is a known property
                if (info.MemberProperties.ContainsKey(elementName))
                {
                    var propertyInfo = info.MemberProperties[elementName];
                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(ViewModelElement)))
                    {
                        var element = SerializationInfo[DeserializationInfo[elementName]].Constructor();
                        element.ReadXml(reader);
                        propertyInfo.Setter(this, element);
                    }
                    else
                    {
                        var value = propertyInfo.UseCustomParser
                            ? CustomPropertyParser(elementName, reader.ReadElementContentAsString())
                            : reader.ReadElementContentAs(propertyInfo.PropertyType, null);

                        if (value == null)
                        {
                            throw new InvalidOperationException(
                                $"Unable to parse node \"{reader.Name}\" " +
                                $"in element \"{elementName}\" " +
                                $"to type \"{propertyInfo.PropertyType.Name}\".");
                        }

                        propertyInfo.Setter(this, value);
                    }

                    continue;
                }
            }

            reader.ReadEndElement();

            // Perform optional post-instantiation processing
            OnReadXmlComplete();
        }

        /// <inheritdoc cref="IXmlSerializable.WriteXml(XmlWriter)"/>
        public void WriteXml(XmlWriter writer)
        {
            var info = SerializationInfo[GetType()];

            // Perform optional pre-serialization processing
            OnWriteXml();

            // Write start tag
            writer.WriteStartElement(info.XmlName);

            // Write attributes
            foreach (var kvp in info.MemberProperties.Where(x => x.Value.TargetNodeType == XmlNodeType.Attribute))
            {
                var attributeString = kvp.Value.UseCustomWriter
                    ? CustomPropertyWriter(kvp.Key, kvp.Value.Getter(this))
                    : kvp.Value.Getter(this).ToString();
                writer.WriteAttributeString(kvp.Key, attributeString);
            }

            // Write elements
            foreach (var kvp in info.MemberProperties.Where(x => x.Value.TargetNodeType == XmlNodeType.Element))
            {
                if (kvp.Value.Getter(this) is ViewModelElement observableElement)
                    observableElement.WriteXml(writer);
                else
                {
                    var elementString = kvp.Value.UseCustomWriter
                        ? CustomPropertyWriter(kvp.Key, kvp.Value.Getter(this))
                        : kvp.Value.Getter(this).ToString();
                    writer.WriteElementString(kvp.Key, elementString);
                }
            }

            // Write collections
            foreach (var kvp in info.MemberCollections)
            {
                // Skip empty collections (won't write empty elements)
                if (kvp.Value.Getter(this).Count == 0 && !WriteEmptyCollectionElements)
                    continue;

                writer.WriteStartElement(kvp.Key);

                foreach (var item in kvp.Value.Getter(this))
                {
                    if (item is ViewModelElement observableElement)
                        observableElement.WriteXml(writer);
                    else
                    {
                        var itemString = kvp.Value.UseCustomWriter
                            ? CustomPropertyWriter(kvp.Value.XmlChildName, item)
                            : item.ToString();
                        writer.WriteElementString(kvp.Value.XmlChildName, itemString);
                    }
                }

                writer.WriteEndElement();
            }
            
            // Write end tag
            writer.WriteEndElement();
        }
        #endregion

        #region Method Overrides (ObservableObject)
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            Messenger.Send(new ViewModelGeneralChangeNotificationMessage(GetType(), e.PropertyName));
        }
        #endregion

        #region Method Overrides (System.Object)
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }

    /// <summary>
    /// A message used to broadcast the fact that ANY property or
    /// collection has changed within a <see cref="ViewModelElement"/>.
    /// </summary>
    public sealed class ViewModelGeneralChangeNotificationMessage
    {
        public Type SenderType { get; }
        public string PropertyName { get; }
        public ViewModelGeneralChangeNotificationMessage(Type senderType, string propertyName)
        {
            SenderType = senderType;
            PropertyName = propertyName;
        }
    }
}