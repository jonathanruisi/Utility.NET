using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using JLR.Utility.NET.Expressions;

namespace JLR.Utility.WinUI.ViewModel
{
    /// <summary>
    /// Contains information necessary for the serialization of
    /// <see cref="ViewModelElement"/>s to, and deserialization from XML.
    /// </summary>
    public sealed class ViewModelSerializationInfo
    {
        #region Properties
        /// <summary>
        /// Gets the name used to represent the object in XML.
        /// </summary>
        public string XmlName { get; private set; }

        /// <summary>
        /// Gets a delegate used to instantiate the object.
        /// </summary>
        public Func<ViewModelElement> Constructor { get; private set; }

        /// <summary>
        /// Gets a dictionary containing the names of all relevant
        /// member properties as they are represented in XML,
        /// as well as the <see cref="Type"/> of the property,
        /// its getter, and its intended XML node type.
        /// </summary>
        public Dictionary<string, ViewModelSerializationPropertyInfo> MemberProperties { get; }

        /// <summary>
        /// Gets a dictionary containing the names of all relevant
        /// member collections as they are represented in XML,
        /// as well as the getter for the collection instance,
        /// and the possible XML names of objects in the collection.
        /// </summary>
        public Dictionary<string, ViewModelSerializationCollectionInfo> MemberCollections { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationInfo"/> class.
        /// </summary>
        public ViewModelSerializationInfo()
        {
            MemberProperties = new Dictionary<string, ViewModelSerializationPropertyInfo>();
            MemberCollections = new Dictionary<string, ViewModelSerializationCollectionInfo>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates an <see cref="ViewModelSerializationInfo"/> object based on
        /// the specified <see cref="ViewModelElement"/> derived type.
        /// </summary>
        /// <param name="type">The type of <see cref="ViewModelElement"/> derived type.</param>
        /// <returns>An initialized <see cref="ViewModelSerializationInfo"/> object.</returns>
        public static ViewModelSerializationInfo Create(Type type)
        {
            var classAttribute = (ViewModelTypeAttribute)Attribute.GetCustomAttribute(
                type, typeof(ViewModelTypeAttribute));

            var result = new ViewModelSerializationInfo
            {
                XmlName = classAttribute.XmlName,
                Constructor = (Func<ViewModelElement>)type.Constructor()
            };

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propertyAttribute = (ViewModelPropertyAttribute)Attribute.GetCustomAttribute(property,
                    typeof(ViewModelPropertyAttribute));

                if (propertyAttribute == null)
                    continue;

                if (propertyAttribute is ViewModelCollectionAttribute collectionAttribute)
                {
                    result.MemberCollections.Add(collectionAttribute.XmlName, new ViewModelSerializationCollectionInfo
                    {
                        PropertyName = property.Name,
                        Getter = (Func<ViewModelElement, IList>)property.PropertyGetter<IList>(),
                        XmlChildName = collectionAttribute.XmlChildName,
                        ChildType = property.PropertyType.GetGenericArguments().Single(),
                        UseCustomParser = collectionAttribute.UseCustomParser,
                        UseCustomWriter = collectionAttribute.UseCustomWriter,
                        HijackSerdes = collectionAttribute.HijackSerdes
                    });
                }
                else
                {
                    var xmlName = propertyAttribute.XmlName;

                    // For properties of type ViewModelElement, make sure they use the
                    // XmlName defined in that class' XmlTypeAttribute.
                    if (property.PropertyType.IsSubclassOf(typeof(ViewModelElement)))
                    {
                        var attribute = (ViewModelTypeAttribute)Attribute.GetCustomAttribute(
                            property.PropertyType, typeof(ViewModelTypeAttribute));
                        xmlName = attribute.XmlName;
                    }

                    result.MemberProperties.Add(xmlName, new ViewModelSerializationPropertyInfo
                    {
                        PropertyName = property.Name,
                        PropertyType = property.PropertyType,
                        TargetNodeType = propertyAttribute.TargetNodeType,
                        Getter = (Func<ViewModelElement, object>)property.PropertyGetter<object>(),
                        Setter = (Action<ViewModelElement, object>)property.PropertySetter(),
                        UseCustomParser = propertyAttribute.UseCustomParser,
                        UseCustomWriter = propertyAttribute.UseCustomWriter,
                        HijackSerdes = propertyAttribute.HijackSerdes
                    });
                }
            }

            return result;
        }
        #endregion

        #region Nested Types
        public sealed class ViewModelSerializationPropertyInfo
        {
            /// <summary>
            /// Gets the type of XML node to which this property belongs.
            /// </summary>
            /// <remarks>
            /// Only <see cref="XmlNodeType.Attribute"/> and
            /// <see cref="XmlNodeType.Element"/> node types are valid.
            /// </remarks>
            public XmlNodeType TargetNodeType { get; internal set; }

            /// <summary>
            /// Gets the name of the property (as it appears in the code).
            /// </summary>
            public string PropertyName { get; internal set; }

            /// <summary>
            /// Gets the type of the property.
            /// </summary>
            public Type PropertyType { get; internal set; }

            /// <summary>
            /// Gets a delegate for the property's getter.
            /// </summary>
            public Func<ViewModelElement, object> Getter { get; internal set; }

            /// <summary>
            /// Gets a delegate for the property's setter.
            /// </summary>
            public Action<ViewModelElement, object> Setter { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether or not to use
            /// <see cref="ViewModelElement.CustomPropertyParser"/>
            /// on this property during deserialization.
            /// </summary>
            public bool UseCustomParser { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether or not to use
            /// <see cref="ViewModelElement.CustomPropertyWriter"/>
            /// on this property during serialization.
            /// </summary>
            public bool UseCustomWriter { get; internal set; }

            /// <summary>
            /// Indicates whether or not to use
            /// <see cref="ViewModelElement.HijackSerialization"/> and
            /// <see cref="ViewModelElement.HijackDeserialization"/>.
            /// </summary>
            public bool HijackSerdes { get; internal set; }
        }

        public sealed class ViewModelSerializationCollectionInfo
        {
            /// <summary>
            /// Gets the name of the collection
            /// (its property name as it appears in the code).
            /// </summary>
            public string PropertyName { get; internal set; }

            /// <summary>
            /// Gets the XML name of items within the collection.
            /// </summary>
            public string XmlChildName { get; internal set; }

            /// <summary>
            /// Gets the type of objects in the collection.
            /// </summary>
            public Type ChildType { get; internal set; }

            /// <summary>
            /// Gets a delegate for the collection's getter.
            /// </summary>
            public Func<ViewModelElement, IList> Getter { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether or not to use
            /// <see cref="ViewModelElement.CustomPropertyParser"/>
            /// on this collection during deserialization.
            /// </summary>
            /// <remarks>
            /// Each object in the collection is passed to the
            /// custom parsing function individually.
            /// </remarks>
            public bool UseCustomParser { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether or not to use
            /// <see cref="ViewModelElement.CustomPropertyWriter"/>
            /// on this collection during serialization.
            /// </summary>
            /// <remarks>
            /// The custom writing function will be called
            /// for each item in the collection.
            /// </remarks>
            public bool UseCustomWriter { get; internal set; }

            /// <summary>
            /// Indicates whether or not to use
            /// <see cref="ViewModelElement.HijackSerialization"/> and
            /// <see cref="ViewModelElement.HijackDeserialization"/> on this
            /// collection during serialization and deserialization respectively.
            /// </summary>
            /// <remarks>
            /// The respective method will be called
            /// for each item in the collection.
            /// </remarks>
            public bool HijackSerdes { get; internal set; }
        }
        #endregion
    }
}