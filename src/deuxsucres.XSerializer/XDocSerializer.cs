using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace deuxsucres.XSerializer
{
    /// <summary>
    /// Simple XDocument serializer/deserializer
    /// </summary>
    public class XDocSerializer
    {
        #region Private fields
        private CultureInfo _Culture;
        #endregion

        #region Ctors & Dests

        /// <summary>
        /// Create a new serializer
        /// </summary>
        public XDocSerializer()
        {
            _Culture = CultureInfo.InvariantCulture;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Singularize a name
        /// </summary>
        protected virtual String SingularizeName(String name)
        {
            if (!String.IsNullOrWhiteSpace(name))
            {
                if (name.EndsWith("s"))
                {
                    return name.Substring(0, name.Length - 1);
                }
            }
            return name;
        }

        /// <summary>
        /// Pluralize a name
        /// </summary>
        protected virtual String PluralizeName(String name)
        {
            if (!String.IsNullOrWhiteSpace(name))
            {
                if (!name.EndsWith("s"))
                {
                    return name + "s";
                }
            }
            return name;
        }

        /// <summary>
        /// Method call to determine the node tag name from the value type
        /// </summary>
        protected virtual String NodeNameFromType(Type type)
        {
            if (type.IsArray)
            {
                var r = NodeNameFromType(type.GetElementType());
                return PluralizeName(r);
            }
            else
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    var tint = type.GetInterfaces()
                        .Where(i => i == typeof(IEnumerable<>) || i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .FirstOrDefault();
                    if (tint != null)
                    {
                        Type tintArgType = tint.GetGenericArguments()[0];
                        if (tintArgType != typeof(Object))
                        {
                            var r = NodeNameFromType(tintArgType);
                            if (!r.EndsWith("s")) r += "s";
                            return PluralizeName(r);
                        }
                    }
                    return "Items";
                }
            }
            return type.Name;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize an object to an XElement
        /// </summary>
        public XElement Serialize(object value, String nodeName = null)
        {
            if (value == null) throw new ArgumentNullException("value");
            var type = value.GetType();
            if (nodeName == null) nodeName = NodeNameFromType(type);

            // Check is an object
            if (value is String || (!type.IsClass && type.IsValueType))
                throw new ArgumentException(String.Format("The value of type '{0}' is not an object.", type.FullName), "value");

            // Prepare element and serialize it
            XElement result = new XElement(nodeName);
            InternalSerialize(value, result, type);
            return result;
        }

        /// <summary>
        /// Serialize an object in a XElement
        /// </summary>
        public void Serialize(object value, XElement node)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (node == null) throw new ArgumentNullException("node");
            InternalSerialize(value, node, value.GetType());
        }

        /// <summary>
        /// Method to serialize a value to an XML element
        /// </summary>
        protected virtual void InternalSerialize(object value, XElement target, Type typeValue)
        {
            // Is a value ?
            if (value is String || (!typeValue.IsClass && typeValue.IsValueType))
            {
                if (value is DateTime)
                {
                    target.Value = String.Format(Culture, "{0:u}", value);
                }
                else
                {
                    target.Value = String.Format(Culture, "{0}", value);
                }
                return;
            }

            // Is a dictionary ?
            if (value is IDictionary<String, Object>)
            {
                foreach (var kvp in (IDictionary<String, Object>)value)
                {
                    var x = new XElement(kvp.Key);
                    target.Add(x);
                    InternalSerialize(kvp.Value, x, kvp.Value.GetType());
                }
                return;
            }

            // Is an enumerable
            if (value is IEnumerable)
            {
                String cName = target.Name.LocalName;
                cName = SingularizeName(cName);
                foreach (var item in (IEnumerable)value)
                {
                    var x = new XElement(cName);
                    target.Add(x);
                    if (item != null)
                        InternalSerialize(item, x, item.GetType());
                }
                return;
            }

            // Here it's an object

            // For each field
            foreach (var field in typeValue.GetFields().Where(f => !f.IsStatic))
            {
                var v = field.GetValue(value);
                if (v == null) continue;
                var x = new XElement(field.Name);
                target.Add(x);
                InternalSerialize(v, x, v.GetType());
            }

            // For each property
            foreach (var property in typeValue.GetProperties())
            {
                if (!property.CanWrite) continue;
                var v = property.GetValue(value, null);
                if (v == null) continue;
                var x = new XElement(property.Name);
                target.Add(x);
                InternalSerialize(v, x, v.GetType());
            }

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the culture used when reading XML. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public virtual CultureInfo Culture
        {
            get { return _Culture ?? CultureInfo.InvariantCulture; }
            set { _Culture = value; }
        }

        #endregion
    }
}
