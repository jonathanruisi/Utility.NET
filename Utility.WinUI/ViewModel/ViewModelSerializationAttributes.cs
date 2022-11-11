using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JLR.Utility.WinUI.ViewModel
{
    /// <summary>
    /// Customizes the serialization and deserialization of a
    /// <see cref="ViewModelElement"/> to/from XML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ViewModelTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the name used to represent the target in XML.
        /// </summary>
        /// <remarks>
        /// This value will be ignored when applied to a
        /// property of type <see cref="ViewModelElement"/>.
        /// In this case, the <see cref="ViewModelElement"/>
        /// type's <see cref="XmlName"/> value will be used instead.
        /// </remarks>
        public string XmlName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelTypeAttribute"/> class.
        /// </summary>
        /// <param name="xmlName">Name used to represent the target in XML.</param>
        public ViewModelTypeAttribute(string xmlName)
        {
            XmlName = xmlName;
        }
    }

    /// <summary>
    /// Customizes the serialization and deserialization of a
    /// <see cref="ViewModelElement"/> member property to/from XML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ViewModelPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the name used to represent the target in XML.
        /// </summary>
        /// <remarks>
        /// This value will be ignored when applied to a
        /// property of type <see cref="ViewModelElement"/>.
        /// In this case, the <see cref="ViewModelElement"/>
        /// type's <see cref="XmlName"/> value will be used instead.
        /// </remarks>
        public string XmlName { get; }

        /// <summary>
        /// Gets the type of XML node to which this item belongs.
        /// </summary>
        /// <remarks>
        /// Only <see cref="XmlNodeType.Attribute"/> and
        /// <see cref="XmlNodeType.Element"/> node types are valid.
        /// </remarks>
        public XmlNodeType TargetNodeType { get; }

        /// <summary>
        /// Gets a value indicating whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyParser"/>
        /// during deserialization.
        /// </summary>
        public bool UseCustomParser { get; }

        /// <summary>
        /// Gets a value indicating whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyWriter"/>
        /// during serialization.
        /// </summary>
        public bool UseCustomWriter { get; }

        /// <summary>
        /// Gets a value indicating how serialization / deserialization
        /// is handled for this property. If<c>true</c>, serialization
        /// of this property will be handled by
        /// <see cref="ViewModelElement.HijackSerialization"/> and
        /// deserialization by <see cref="ViewModelElement.HijackDeserialization"/>.
        /// If <c>false</c>, serialization/deserialization is handled internally
        /// by <see cref="ViewModelElement"/>.
        /// </summary>
        public bool HijackSerdes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelPropertyAttribute"/> class.
        /// </summary>
        /// <param name="xmlName">Name used to represent the target in XML.</param>
        /// <param name="targetNodeType">
        /// The type of XML node to which this item belongs.
        /// <para>
        /// Only <see cref="XmlNodeType.Attribute"/> and
        /// <see cref="XmlNodeType.Element"/> node types are valid.
        /// </para>
        /// </param>
        /// <param name="useCustomParser">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyParser"/>
        /// on this property during deserialization.
        /// </param>
        /// <param name="useCustomWriter">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyWriter"/>
        /// during serialization.
        /// </param>
        /// <param name="hijackSerdes">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.HijackSerialization"/> and
        /// <see cref="ViewModelElement.HijackDeserialization"/>.
        /// </param>
        public ViewModelPropertyAttribute(string xmlName,
                                          XmlNodeType targetNodeType,
                                          bool useCustomParser = false,
                                          bool useCustomWriter = false,
                                          bool hijackSerdes = false)
        {
            if (targetNodeType is not XmlNodeType.Attribute and not XmlNodeType.Element)
            {
                throw new ArgumentException("Valid node types are limited to \"Attribute\" and \"Element\" only");
            }

            XmlName = xmlName;
            TargetNodeType = targetNodeType;
            UseCustomParser = useCustomParser;
            UseCustomWriter = useCustomWriter;
            HijackSerdes = hijackSerdes;
        }
    }

    /// <summary>
    /// Specifies the name used to represent a member collection in XML,
    /// as well as the possible XML names of items within the collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ViewModelCollectionAttribute : ViewModelPropertyAttribute
    {
        /// <summary>
        /// Gets the XML name of items within the collection.
        /// </summary>
        public string XmlChildName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCollectionAttribute"/> class.
        /// </summary>
        /// <param name="xmlName">Name used to represent the collection in XML.</param>
        /// <param name="xmlChildName">Name used to represent collection items in XML.</param>
        /// <param name="useCustomParser">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyParser"/>
        /// on this collection during deserialization.
        /// <para>
        /// Each object in the collection is passed to the
        /// custom parsing function individually.
        /// </para>
        /// </param>
        /// <param name="useCustomWriter">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.CustomPropertyWriter"/>
        /// on this collection during serialization.
        /// <para>
        /// The custom writing function will be called
        /// for each item in the collection.
        /// </para>
        /// </param>
        /// <param name="hijackSerdes">
        /// Indicates whether or not to use
        /// <see cref="ViewModelElement.HijackSerialization"/> and
        /// <see cref="ViewModelElement.HijackDeserialization"/> on this
        /// collection during serialization and deserialization respectively.
        /// <para>
        /// The respective method will be called
        /// for each item in the collection.
        /// </para>
        /// </param>
        public ViewModelCollectionAttribute(string xmlName,
                                            string xmlChildName = null,
                                            bool useCustomParser = false,
                                            bool useCustomWriter = false,
                                            bool hijackSerdes = false)
            : base(xmlName, XmlNodeType.Element, useCustomParser, useCustomWriter, hijackSerdes)
        {
            XmlChildName = xmlChildName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ViewModelPropertyOverrideAttribute : ViewModelPropertyAttribute
    {
        public string PropertyToOverride { get; }

        public ViewModelPropertyOverrideAttribute(string propertyToOverride,
                                                  string xmlName,
                                                  XmlNodeType targetNodeType,
                                                  bool useCustomParser = false,
                                                  bool useCustomWriter = false,
                                                  bool hijackSerdes = false)
            : base(xmlName, targetNodeType, useCustomParser, useCustomWriter, hijackSerdes)
        {
            PropertyToOverride = propertyToOverride;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ViewModelCollectionOverrideAttribute : ViewModelCollectionAttribute
    {
        public string CollectionToOverride { get; }

        public ViewModelCollectionOverrideAttribute(string collectionToOverride,
                                                    string xmlName,
                                                    string xmlChildName = null,
                                                    bool useCustomParser = false,
                                                    bool useCustomWriter = false,
                                                    bool hijackSerdes = false)
            : base(xmlName, xmlChildName, useCustomParser, useCustomWriter, hijackSerdes)
        {
            CollectionToOverride = collectionToOverride;
        }
    }
}