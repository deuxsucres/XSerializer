using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        /// Clean the name to be a valid XML tag name
        /// </summary>
        protected virtual String CleanupName(String name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w-]", String.Empty);
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
                var r = CleanupName(NodeNameFromType(type.GetElementType()));
                return PluralizeName(r);
            }
            else if (!type.IsValueType && type != typeof(String))
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
                            var r = CleanupName(NodeNameFromType(tintArgType));
                            if (!r.EndsWith("s")) r += "s";
                            return PluralizeName(r);
                        }
                    }
                    return "Items";
                }
            }
            return CleanupName(type.Name);
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

            // Check is an object
            if (value is String || (!type.IsClass && type.IsValueType))
                throw new ArgumentException(String.Format("The value of type '{0}' is not an object.", type.FullName), "value");

            // Prepare element and serialize it
            if (nodeName == null) nodeName = NodeNameFromType(type);
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
                var v = property.GetValue(value, null);
                if (v == null) continue;
                var x = new XElement(property.Name);
                target.Add(x);
                InternalSerialize(v, x, v.GetType());
            }

        }

        #endregion

        #region Object populate

        /// <summary>
        /// Populate object values from XML element
        /// </summary>
        public void Populate(XElement node, object target)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (target == null) throw new ArgumentNullException("target");
            InternalPopulate(node, target);
        }

        /// <summary>
        /// Populate object valeurs from XML text
        /// </summary>
        public void Populate(String xml, object target)
        {
            Populate(XDocument.Parse(xml).Root, target);
        }

        /// <summary>
        /// Internal object values population from XML element
        /// </summary>
        protected void InternalPopulate(XElement node, object target)
        {
            var tt = target.GetType();

            // Check target is an object
            if (target is String || (!tt.IsClass && tt.IsValueType))
                throw new ArgumentException(String.Format("The target of type '{0}' is not an object.", tt.FullName), "target");

            // Target is dictionary ?
            if (target is IDictionary<String, Object>)
            {
                PopulateDictionary(node, (IDictionary<String, Object>)target);
                return;
            }
            // Target is array ?
            if (target is Array)
            {
                var stt = tt.GetElementType();
                int i = 0; int arrLength = ((Array)target).Length;
                foreach (var child in node.Nodes().OfType<XElement>())
                {
                    if (i >= arrLength) break;
                    ((Array)target).SetValue(Deserialize(child, stt), i++);
                }
                return;
            }
            // Target is list ?
            if (target is IList)
            {
                var stt = tt.GetGenericArguments()[0];
                foreach (var child in node.Nodes().OfType<XElement>())
                {
                    ((IList)target).Add(Deserialize(child, stt));
                }
                return;
            }

            // Extract target properties
            Dictionary<String, MemberInfo> members = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);
            var writableMembers = tt
                .GetFields()
                .Where(f => !f.IsStatic)
                .Cast<MemberInfo>()
                .Concat(tt.GetProperties());
            foreach (var m in writableMembers)
                members[m.Name] = m;

            MemberInfo member;
            // Browse attributes
            foreach (var attr in node.Attributes())
            {
                // Find member with same name than attribute name
                if (members.TryGetValue(attr.Name.LocalName, out member))
                {
                    // Write value
                    WriteToMember(target, member, attr);
                }
            }
            // Browse child nodes
            foreach (var child in node.Nodes().OfType<XElement>())
            {
                // Find member with same name than tag name
                if (members.TryGetValue(child.Name.LocalName, out member))
                {
                    // Write value
                    WriteToMember(target, member, child);
                }
            }
        }

        /// <summary>
        /// Populate a dictionary from a XML element
        /// </summary>
        protected void PopulateDictionary(XElement node, IDictionary<string, object> dictionary)
        {
            foreach (var attr in node.Attributes())
            {
                dictionary[attr.Name.LocalName] = InternalDeserializeObject(attr);
            }
            foreach (var child in node.Nodes().OfType<XElement>())
            {
                dictionary[child.Name.LocalName] = InternalDeserializeObject(child);
            }
        }

        /// <summary>
        /// Write an attribute value to a member
        /// </summary>
        private void WriteToMember(object target, MemberInfo member, XAttribute attr)
        {
            PropertyInfo pi; FieldInfo fi;
            // If member is a property we affect only if it's a writable property
            if (member is PropertyInfo)
            {
                pi = (PropertyInfo)member;
                if (pi.CanWrite)
                {
                    pi.SetValue(target, InternalDeserialize(attr, pi.PropertyType), null);
                }
            }
            else
            {
                fi = (FieldInfo)member;
                fi.SetValue(target, InternalDeserialize(attr, fi.FieldType));
            }
        }

        /// <summary>
        /// Write a node value/object to a member
        /// </summary>
        private void WriteToMember(object target, MemberInfo member, XElement node)
        {
            PropertyInfo pi; FieldInfo fi;
            // If member is a property we affect only if it's a writable property
            if (member is PropertyInfo)
            {
                pi = (PropertyInfo)member;
                if (pi.CanWrite)
                {
                    pi.SetValue(target, InternalDeserialize(node, pi.PropertyType), null);
                }
            }
            else
            {
                fi = (FieldInfo)member;
                fi.SetValue(target, InternalDeserialize(node, fi.FieldType));
            }
        }

        #endregion

        #region Deserialization

        /// <summary>
        /// Deserialize to a typed value
        /// </summary>
        public T Deserialize<T>(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return (T)Deserialize(node, typeof(T));
        }

        /// <summary>
        /// Deserialize to a typed value
        /// </summary>
        public T Deserialize<T>(String xml)
        {
            return Deserialize<T>(XDocument.Parse(xml).Root);
        }

        /// <summary>
        /// Deserialize to a typed value
        /// </summary>
        public object Deserialize(XElement node, Type type)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (type == null) throw new ArgumentNullException("type");
            return InternalDeserialize(node, type);
        }

        /// <summary>
        /// Deserialize to a typed value
        /// </summary>
        public object Deserialize(String xml, Type type)
        {
            return Deserialize(XDocument.Parse(xml).Root, type);
        }

        /// <summary>
        /// Deserialize to an untyped value
        /// </summary>
        public object Deserialize(XElement node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return InternalDeserialize(node, typeof(Object));
        }

        /// <summary>
        /// Deserialize to an untyped value
        /// </summary>
        public object Deserialize(String xml)
        {
            return Deserialize(XDocument.Parse(xml).Root);
        }

        /// <summary>
        /// Internal XML node deserialization to an object
        /// </summary>
        object InternalDeserialize(XElement node, Type objectType)
        {
            // If target type is Object we deserialize as a value or an Anonymous Object (Dictionary<String,Object>)
            if (objectType == typeof(Object)) return InternalDeserializeObject(node);

            // We try to convert the value node to an simple value (string, date, int, etc.)
            object value = null;
            if (TryToConvertValue(objectType, node.Value, out value)) return value;

            // If dictionary
            if (objectType == typeof(IDictionary<String, Object>))
            {
                value = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else if (objectType.IsInterface && (objectType == typeof(IList<>) || objectType.GetGenericTypeDefinition() == typeof(IList<>))) 
                // If list
            {
                var lt = typeof(List<>).MakeGenericType(objectType.GetGenericArguments()[0]);
                value = Activator.CreateInstance(lt);
            }
            else if (typeof(Array).IsAssignableFrom(objectType)) 
                // If array
            {
                var stt = objectType.GetElementType();
                System.Collections.IList lt = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stt));
                Populate(node, lt);
                Array res = Array.CreateInstance(stt, lt.Count);
                for (int i = 0; i < lt.Count; i++)
                    res.SetValue(lt[i], i);
                return res;
            }

            // Create an instance and populate it
            if (value == null) value = Activator.CreateInstance(objectType);
            Populate(node, value);
            return value;
        }

        /// <summary>
        /// Internal XML attribute deserialization to an object
        /// </summary>
        object InternalDeserialize(XAttribute attr, Type objectType)
        {
            // Attributes can contains only value, so if we can convert to value we raise an exception
            object value = null;
            if (TryToConvertValue(objectType, attr.Value, out value)) return value;

            throw new ArgumentException(String.Format("Type '{0}' can't be deserialized from an attribute value.", objectType.FullName));
        }

        /// <summary>
        /// Internal XML node to anonymous object deserialisation
        /// </summary>
        object InternalDeserializeObject(XElement node)
        {
            // If node contains no children, it's a value type
            if (!node.HasElements)
            {
                // Check the type from attrbute
                var attrType = node.Attribute("type");
                if (attrType != null)
                {
                    switch (attrType.Value.ToLower())
                    {
                        case "int":
                        case "integer":
                            return InternalDeserialize(node, typeof(Int64));
                        case "float":
                        case "double":
                        case "number":
                            return InternalDeserialize(node, typeof(Decimal));
                        case "date":
                        case "datetime":
                            return InternalDeserialize(node, typeof(DateTime));
                        default:
                            return InternalDeserialize(node, typeof(String));
                    }
                }
                else
                {
                    return InternalDeserializeValue(node.Value);
                }
            }
            // Anonymous object
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            InternalPopulate(node, result);
            return result;
        }

        /// <summary>
        /// Internal XML attribute to value deserialisation
        /// </summary>
        object InternalDeserializeObject(XAttribute attr)
        {
            return InternalDeserializeValue(attr.Value);
        }

        /// <summary>
        /// Try to convert a string to value (bool, date, number, etc.), if we can't we consider it's a string
        /// </summary>
        object InternalDeserializeValue(String value)
        {
            if (String.Equals("true", value, StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals("false", value, StringComparison.OrdinalIgnoreCase)) return false;
            Int64 tint;
            if (Int64.TryParse(value, NumberStyles.Any, Culture, out tint)) return tint;
            Decimal tdbl;
            if (Decimal.TryParse(value, NumberStyles.Any, Culture, out tdbl)) return tdbl;
            DateTime tdt;
            if (DateTime.TryParse(value, Culture, DateTimeStyles.AssumeLocal, out tdt)) return tdt;
            return value;
        }

        Int64? TryParseInt(String value)
        {
            Int64 result;
            if (String.IsNullOrWhiteSpace(value)) return null;
            if (Int64.TryParse(value, out result))
                return result;
            return null;
        }

        UInt64? TryParseUInt(String value)
        {
            UInt64 result;
            if (String.IsNullOrWhiteSpace(value)) return null;
            if (UInt64.TryParse(value, out result))
                return result;
            return null;
        }

        Decimal? TryParseFloat(String value)
        {
            Decimal result;
            if (String.IsNullOrWhiteSpace(value)) return null;
            if (Decimal.TryParse(value, NumberStyles.Any, Culture, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Try to convert a string value to a value type.
        /// </summary>
        bool TryToConvertValue(Type fromType, String fromValue, out object toValue)
        {
            Int64? iTmp;
            UInt64? uiTmp;
            Decimal? fTmp;
            DateTime dtTmp;
            toValue = null;
            if (fromType == typeof(String))
            {
                toValue = fromValue;
            }

            #region Bool
            else if (fromType == typeof(bool))
            {
                toValue = String.Equals(fromValue, "true", StringComparison.OrdinalIgnoreCase);
            }
            else if (fromType == typeof(bool?))
            {
                if (String.Equals(fromValue, "true", StringComparison.OrdinalIgnoreCase))
                    toValue = (bool?)true;
                else if (String.Equals(fromValue, "false", StringComparison.OrdinalIgnoreCase))
                    toValue = (bool?)false;
                else
                    toValue = (bool?)null;
            }
            #endregion

            #region Int32
            else if (fromType == typeof(Int32))
            {
                toValue = Convert.ToInt32(TryParseInt(fromValue) ?? 0);
            }
            else if (fromType == typeof(Int32?))
            {
                iTmp = TryParseInt(fromValue);
                if (iTmp.HasValue)
                    toValue = (Int32?)Convert.ToInt16(iTmp.Value);
                else
                    toValue = (Int32?)null;
            }
            #endregion

            #region Int16
            else if (fromType == typeof(Int16))
            {
                toValue = Convert.ToInt16(TryParseInt(fromValue) ?? 0);
            }
            else if (fromType == typeof(Int16?))
            {
                iTmp = TryParseInt(fromValue);
                if (iTmp.HasValue)
                    toValue = (Int16?)Convert.ToInt16(iTmp.Value);
                else
                    toValue = (Int16?)null;
            }
            #endregion

            #region Int64
            else if (fromType == typeof(Int64))
            {
                toValue = TryParseInt(fromValue) ?? 0;
            }
            else if (fromType == typeof(Int64?))
            {
                toValue = TryParseInt(fromValue);
            }
            #endregion

            #region UInt32
            else if (fromType == typeof(UInt32))
            {
                toValue = Convert.ToUInt32(TryParseUInt(fromValue) ?? 0);
            }
            else if (fromType == typeof(UInt32?))
            {
                uiTmp = TryParseUInt(fromValue);
                if (uiTmp.HasValue)
                    toValue = (UInt32?)Convert.ToUInt32(uiTmp.Value);
                else
                    toValue = (UInt32?)null;
            }
            #endregion

            #region UInt16
            else if (fromType == typeof(UInt16))
            {
                toValue = Convert.ToUInt16(TryParseUInt(fromValue) ?? 0);
            }
            else if (fromType == typeof(UInt16?))
            {
                uiTmp = TryParseUInt(fromValue);
                if (uiTmp.HasValue)
                    toValue = (UInt16?)Convert.ToUInt16(uiTmp.Value);
                else
                    toValue = (UInt16?)null;
            }
            #endregion

            #region UInt64
            else if (fromType == typeof(UInt64))
            {
                toValue = TryParseUInt(fromValue) ?? 0;
            }
            else if (fromType == typeof(UInt64?))
            {
                toValue = TryParseUInt(fromValue);
            }
            #endregion

            #region double
            else if (fromType == typeof(Double))
            {
                toValue = Convert.ToDouble(TryParseFloat(fromValue) ?? 0);
            }
            else if (fromType == typeof(Double?))
            {
                fTmp = TryParseFloat(fromValue);
                if (fTmp.HasValue)
                    toValue = (Double?)Convert.ToDouble(fTmp.Value);
                else
                    toValue = (Double?)null;
            }
            #endregion

            #region Single
            else if (fromType == typeof(Single))
            {
                toValue = Convert.ToSingle(TryParseFloat(fromValue) ?? 0);
            }
            else if (fromType == typeof(Single?))
            {
                fTmp = TryParseFloat(fromValue);
                if (fTmp.HasValue)
                    toValue = (Single?)Convert.ToSingle(fTmp.Value);
                else
                    toValue = (Single?)null;
            }
            #endregion

            #region Decimal
            else if (fromType == typeof(Decimal))
            {
                toValue = TryParseFloat(fromValue) ?? 0;
            }
            else if (fromType == typeof(Decimal?))
            {
                toValue = TryParseFloat(fromValue);
            }
            #endregion

            #region DateTime
            else if (fromType == typeof(DateTime))
            {
                if (DateTime.TryParse(fromValue, Culture, DateTimeStyles.AssumeLocal, out dtTmp))
                    toValue = dtTmp;
                else
                    toValue = DateTime.MinValue;
            }
            else if (fromType == typeof(DateTime?))
            {
                if (DateTime.TryParse(fromValue, Culture, DateTimeStyles.AssumeLocal, out dtTmp))
                    toValue = (DateTime?)dtTmp;
                else
                    toValue = (DateTime?)null;
            }
            #endregion
            else
            {
                return false;
            }
            return true;
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
