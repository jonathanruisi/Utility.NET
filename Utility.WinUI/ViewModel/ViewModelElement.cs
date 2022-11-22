using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using JLR.Utility.NET.Reflection;

using Windows.Storage;

namespace JLR.Utility.WinUI.ViewModel
{
    /// <summary>
    /// <inheritdoc/><br/>
    /// <see cref="ViewModelElement"/> can act as a base
    /// "leaf node" class in a hierarchical data structure, and
    /// is capable of identifying its parent, root, and siblings.<br/>
    /// <see cref="ViewModelElement"/> is also capable of
    /// fully automatic XML serialization and deserialization
    /// of itself and all derived classes.
    /// </summary>
    public abstract class ViewModelElement : ObservableRecipient, IXmlSerializable
    {
        #region Fields
        private string _name;
        private bool _isSelected;
        protected internal ViewModelNode _parent;
        private static readonly bool IsSubclassInfoLoaded = false;
        private static readonly Dictionary<Type, ViewModelSerializationInfo> SerializationInfo;
        private static readonly Dictionary<string, Type> DeserializationInfo;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        [ViewModelProperty(nameof(Name), XmlNodeType.Attribute)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, true);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this element
        /// is currently selected somewhere in the user interface.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value, true);
        }

        /// <summary>
        /// Gets or sets a reference to this element's parent node.
        /// </summary>
        public ViewModelNode Parent
        {
            get => _parent;
            set => SetProperty(_parent, value, newValue =>
            {
                _parent?.Children.Remove(this);
                newValue?.Children.Add(this);
                _parent = newValue;
            }, true);
        }

        /// <summary>
        /// Gets the depth of this element within the tree relative to the root node.
        /// A value of zero indicates that this element is the root.
        /// </summary>
        public int Depth => _parent?.Depth + 1 ?? 0;

        /// <summary>
        /// Gets a reference to the root node.
        /// </summary>
        public ViewModelNode Root => _parent == null ? (this as ViewModelNode) : _parent.Root;

        /// <summary>
        /// Exposes an enumerator which iterates over all elements that share this element's parent node
        /// (non-recursive).
        /// </summary>
        public IEnumerable<ViewModelElement> Siblings => from sibling in _parent?.Children
                                                         where sibling != this && sibling.Depth == Depth
                                                         select sibling;

        /// <summary>
        /// Gets or sets a value that controls whether or not empty collections
        /// are to be serialized to XML in the form of an empty element.
        /// </summary>
        public bool WriteEmptyCollectionElements { get; set; }
        #endregion

        #region Constructors
        static ViewModelElement()
        {
            SerializationInfo ??= new Dictionary<Type, ViewModelSerializationInfo>();
            DeserializationInfo ??= new Dictionary<string, Type>();

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
            _isSelected = false;
            WriteEmptyCollectionElements = false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the XML representation of this <see cref="ViewModelElement"/>
        /// to a specified <see cref="StorageFile"/>.
        /// </summary>
        /// <returns>
        /// <b><c>true</c></b> if successful, <b><c>false</c></b> otherwise.
        /// </returns>
        public virtual async Task<bool> SaveAsync(StorageFile file)
        {
            if (file == null || !file.IsAvailable)
                return false;

            // Create a temporary backup of the current save file,
            // if it exists, then erase the current save file.
            StorageFile backupFile = null;
            File.Delete(file.Path + ".bak");
            if (File.Exists(file.Path))
            {
                backupFile = await file.CopyAsync(await file.GetParentAsync(), file.Name + ".bak");
                await FileIO.WriteTextAsync(file, string.Empty);
            }

            var success = true;
            XmlWriter writer = null;
            try
            {
                var settings = new XmlWriterSettings
                {
                    Async = true,
                    Indent = true,
                    IndentChars = "\t",
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Document,
                    CloseOutput = true
                };

                writer = XmlWriter.Create(await file.OpenStreamForWriteAsync(), settings);
                WriteXml(writer);
                await writer.FlushAsync();
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                writer?.Close();
            }

            if (success)
            {
                // Delete the temporary backup file
                if (backupFile != null)
                    await backupFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                return true;
            }
            else
            {
                // Restore the previous save file from the backup
                if (backupFile != null)
                    await backupFile.MoveAndReplaceAsync(file);

                return false;
            }
        }

        public static async Task<XmlReader> GetXmlReaderForFileAsync(StorageFile file)
        {
            if (file == null || !file.IsAvailable)
                return null;

            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            var reader = XmlReader.Create(await file.OpenStreamForReadAsync(), settings);
            reader.MoveToContent();
            return reader;
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
        public static ViewModelElement FromXml(XmlReader reader)
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

            var element = InstantiateObjectFromXmlTagName(reader.Name);
            element.ReadXml(reader);
            return element;
        }

        public static string GetXmlTagForType(Type type)
        {
            return SerializationInfo.ContainsKey(type)
                ? SerializationInfo[type].XmlName
                : null;
        }

        public static ViewModelElement InstantiateObjectFromXmlTagName(string xmlName)
        {
            if (!DeserializationInfo.ContainsKey(xmlName))
                throw new ArgumentException(
                    $"{xmlName} does not represent a known ViewModelElement-derived type");

            return SerializationInfo[DeserializationInfo[xmlName]].Constructor();
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
        /// marked with <see cref="ViewModelPropertyAttribute.UseCustomParser"/>
        /// = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">The name of the property to parse.</param>
        /// <param name="content">The property's content as read from XML.</param>
        /// <param name="args">A list of optional parameter strings.</param>
        /// <remarks>
        /// Classes that override this function are only responsible for parsing
        /// the content passed to it. Do not set the property directly.
        /// Simply return the fully constructed property value.
        /// </remarks>
        protected virtual object CustomPropertyParser(string propertyName,
                                                      string content,
                                                      params string[] args)
        {
            return null;
        }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// during XML serialization when writing a property
        /// marked with <see cref="ViewModelPropertyAttribute.UseCustomWriter"/>
        /// = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">
        /// The <b><u>XML name</u></b> of the property to be written.
        /// </param>
        /// <param name="value">The object to be written.</param>
        /// <param name="args">A list of optional parameter strings.</param>
        /// <returns>
        /// A string representation of the object.
        /// </returns>
        /// <remarks>
        /// Use this method when a property is not a <see cref="ViewModelElement"/>,
        /// and <see cref="object.ToString"/> does not adequately represent the property.
        /// </remarks>
        protected virtual string CustomPropertyWriter(string propertyName,
                                                      object value,
                                                      params string[] args)
        {
            return null;
        }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// during XML deserialization when parsing a property marked with
        /// <see cref="ViewModelPropertyAttribute.HijackSerdes"/> = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that needs to be read.
        /// </param>
        /// <param name="reader">
        /// The <see cref="XmlReader"/> currently pointing to the start tag of the object to be read.
        /// </param>
        /// <param name="args">A list of optional parameter strings.</param>
        /// <returns>
        /// The fully constructed object which will be assigned to <paramref name="propertyName"/>.
        /// </returns>
        protected virtual object HijackDeserialization(string propertyName,
                                                       ref XmlReader reader,
                                                       params string[] args)
        {
            return null;
        }

        /// <summary>
        /// When overridden in a derived class, this method is called
        /// during XML serialization when writing a property marked with
        /// <see cref="ViewModelPropertyAttribute.HijackSerdes"/> = <b><c>true</c></b>.
        /// </summary>
        /// <param name="propertyName">
        /// The <b><u>XML name</u></b> of the property to be written.
        /// </param>
        /// <param name="value">The object to be written.</param>
        /// <param name="writer">
        /// The <see cref="XmlWriter"/> with which to write the object's contents.
        /// </param>
        /// <param name="args">A list of optional parameter strings.</param>
        protected virtual void HijackSerialization(string propertyName,
                                                   object value,
                                                   ref XmlWriter writer,
                                                   params string[] args) { }
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
            var isEmpty = reader.IsEmptyElement && !reader.HasAttributes;
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
                    object value;
                    var propertyInfo = info.MemberProperties[reader.Name];

                    if (propertyInfo.HijackSerdes)
                        value = HijackDeserialization(reader.Name, ref reader);
                    else if (propertyInfo.UseCustomParser)
                        value = CustomPropertyParser(reader.Name, reader.ReadContentAsString());
                    else
                        value = reader.ReadContentAs(propertyInfo.PropertyType, null);

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
                if (reader.IsEmptyElement && !reader.HasAttributes)
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
                        object value;
                        if (collectionInfo.HijackSerdes)
                        {
                            value = HijackDeserialization(collectionInfo.PropertyName,
                                                          ref reader,
                                                          collectionInfo.XmlChildName);
                        }
                        else if (collectionInfo.ChildType.IsAssignableTo(typeof(ViewModelElement)))
                        {
                            var element = InstantiateObjectFromXmlTagName(elementName);
                            element.ReadXml(reader);
                            value = element;
                        }
                        else if (collectionInfo.UseCustomParser)
                        {
                            value = CustomPropertyParser(collectionInfo.PropertyName,
                                                         reader.ReadElementContentAsString(),
                                                         collectionInfo.XmlChildName);
                        }
                        else
                        {
                            value = reader.ReadElementContentAs(collectionInfo.ChildType, null);
                        }

                        if (value == null)
                        {
                            throw new InvalidOperationException(
                                $"Unable to parse node \"{reader.Name}\" " +
                                $"in element \"{elementName}\" " +
                                $"to type \"{collectionInfo.ChildType.Name}\".");
                        }

                        // TODO: Figure out a way to handle generic arguments here
                        if (value is KeyValuePair<object, object> kvp)
                        {
                            ((IDictionary)collectionInfo.Getter(this)).Add(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            ((IList)collectionInfo.Getter(this)).Add(value);
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
                    object value;
                    var propertyInfo = info.MemberProperties[elementName];

                    if (propertyInfo.HijackSerdes)
                    {
                        value = HijackDeserialization(elementName, ref reader);
                    }
                    else if (propertyInfo.PropertyType.IsAssignableTo(typeof(ViewModelElement)))
                    {
                        var element = InstantiateObjectFromXmlTagName(elementName);
                        element.ReadXml(reader);
                        value = element;
                    }
                    else if (propertyInfo.UseCustomParser)
                    {
                        value = CustomPropertyParser(elementName, reader.ReadElementContentAsString());
                    }
                    else
                    {
                        value = reader.ReadElementContentAs(propertyInfo.PropertyType, null);
                    }

                    if (value == null)
                    {
                        throw new InvalidOperationException(
                            $"Unable to parse node \"{reader.Name}\" " +
                            $"in element \"{elementName}\" " +
                            $"to type \"{propertyInfo.PropertyType.Name}\".");
                    }

                    propertyInfo.Setter(this, value);

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
                if (kvp.Value.HijackSerdes)
                {
                    HijackSerialization(kvp.Key, kvp.Value.Getter(this), ref writer);
                }
                else
                {
                    var attributeString = kvp.Value.UseCustomWriter
                    ? CustomPropertyWriter(kvp.Key, kvp.Value.Getter(this))
                    : kvp.Value.Getter(this)?.ToString();

                    if (!string.IsNullOrEmpty(attributeString))
                        writer.WriteAttributeString(kvp.Key, attributeString);
                }
            }

            // Write elements
            foreach (var kvp in info.MemberProperties.Where(x => x.Value.TargetNodeType == XmlNodeType.Element))
            {
                if (kvp.Value.HijackSerdes)
                {
                    HijackSerialization(kvp.Key, kvp.Value.Getter(this), ref writer);
                }
                else if (kvp.Value.Getter(this) is ViewModelElement vme)
                {
                    vme.WriteXml(writer);
                }
                else
                {
                    var elementString = kvp.Value.UseCustomWriter
                        ? CustomPropertyWriter(kvp.Key, kvp.Value.Getter(this))
                        : kvp.Value.Getter(this)?.ToString();

                    if (!string.IsNullOrEmpty(elementString))
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
                    if (kvp.Value.HijackSerdes)
                    {
                        HijackSerialization(kvp.Value.PropertyName, item, ref writer, kvp.Value.XmlChildName);
                    }
                    else if (item is ViewModelElement vme)
                    {
                        vme.WriteXml(writer);
                    }
                    else
                    {
                        var itemString = kvp.Value.UseCustomWriter
                            ? CustomPropertyWriter(kvp.Value.PropertyName, item, kvp.Value.XmlChildName)
                            : item?.ToString();

                        if (!string.IsNullOrEmpty(itemString))
                            writer.WriteElementString(kvp.Value.XmlChildName, itemString);
                    }
                }

                writer.WriteEndElement();
            }

            // Write end tag
            writer.WriteEndElement();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Call this method from a derived class when a
        /// <see cref="SerializedPropertyChangedMessage"/> needs to
        /// be sent for a change within a member collection.
        /// </summary>
        /// <param name="collectionName">
        /// The name of the collection that changed.
        /// </param>
        protected void NotifySerializedCollectionChanged(string collectionName)
        {
            var serializationInfo = SerializationInfo[GetType()];
            if (serializationInfo.MemberCollections.Values.Any(x => x.PropertyName == collectionName))
                Messenger.Send(new SerializedPropertyChangedMessage(this, collectionName));
        }
        #endregion

        #region Method Overrides (ObservableObject)
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var serializationInfo = SerializationInfo[GetType()];
            if (serializationInfo.MemberProperties.Values.Any(x => x.PropertyName == e.PropertyName))
                Messenger.Send(new SerializedPropertyChangedMessage(this, e.PropertyName));
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
    /// A message used to broadcast a change notification
    /// for <see cref="ViewModelElement"/> properties and collections
    /// that are marked for serialization.
    /// </summary>
    /// <remarks>
    /// These messages can be useful in determining if the
    /// <see cref="ViewModelElement"/> in memory no longer
    /// matches its serialized state.
    /// </remarks>
    public sealed class SerializedPropertyChangedMessage
    {
        public ViewModelElement Sender { get; }
        public string PropertyName { get; }
        public SerializedPropertyChangedMessage(ViewModelElement sender, string propertyName)
        {
            Sender = sender;
            PropertyName = propertyName;
        }
    }
}