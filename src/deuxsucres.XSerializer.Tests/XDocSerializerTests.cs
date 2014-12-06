using deuxsucres.XSerializer.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace deuxsucres.XSerializer.Tests
{
    public class XDocSerializerTests
    {

        [Fact]
        public void TestCreate()
        {
            var serializer = new XDocSerializer();
            Assert.Same(System.Globalization.CultureInfo.InvariantCulture, serializer.Culture);
        }

        [Fact]
        public void TestGetSetCulture()
        {
            var serializer = new XDocSerializer();

            // Default culture is invariant
            Assert.Same(System.Globalization.CultureInfo.InvariantCulture, serializer.Culture);

            // Test new culture
            var culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            serializer.Culture = culture;
            Assert.Same(culture, serializer.Culture);

            // Test default culture
            serializer.Culture = null;
            Assert.Same(System.Globalization.CultureInfo.InvariantCulture, serializer.Culture);

        }

        #region Serialization

        [Fact]
        public void TestSerializeValues()
        {
            var serializer = new XDocSerializer();

            // Value types are not supported
            Assert.Throws<ArgumentException>(() => serializer.Serialize(123));
            Assert.Throws<ArgumentException>(() => serializer.Serialize(123.456));
            Assert.Throws<ArgumentException>(() => serializer.Serialize(DateTime.Now));
            Assert.Throws<ArgumentException>(() => serializer.Serialize(true));
            Assert.Throws<ArgumentException>(() => serializer.Serialize("Text"));

            // Invalid arguments
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null, (XElement)null));
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(new TestClassSimple(), (XElement)null));
        }

        [Fact]
        public void TestSerializeClass()
        {
            var serializer = new XDocSerializer();
            DateTime dt = new DateTime(2014, 6, 8, 11, 44, 56);
            var node1 = serializer.Serialize(new TestClassSimple() {
                Value1 = null,
                Value2 = 23,
                Value3 = 67.89,
                Value4 = dt
            });
            Assert.Equal("<TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></TestClassSimple>", node1.ToString(SaveOptions.DisableFormatting));
            Assert.Throws<ArgumentException>(() => serializer.Serialize(123));
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null));
            Assert.Throws<ArgumentException>(() => serializer.Serialize(new TestClassSimple(), String.Empty));
            
            var node2 = new XElement("root");
            serializer.Serialize(new TestClassSimple() {
                Value1 = null,
                Value2 = 23,
                Value3 = 67.89,
                Value4 = dt
            }, node2);
            Assert.Equal("<root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root>", node2.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestSerializeEnumerable()
        {
            var serializer = new XDocSerializer();

            DateTime dt = new DateTime(2014, 6, 8, 11, 44, 56);
            var array = new TestClassSimple[]{
                new TestClassSimple() {
                    Value1 = null,
                    Value2 = 23,
                    Value3 = 67.89,
                    Value4 = dt
                },new TestClassSimple() {
                    Value1 = "Texte",
                    Value2 = 98,
                    Value3 = -12.67,
                    Value4 = dt
                }
            };
            var list = new List<TestClassSimple>(array);

            var node1 = serializer.Serialize(array);
            Assert.Equal("<TestClassSimples><TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></TestClassSimple><TestClassSimple><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></TestClassSimple></TestClassSimples>", node1.ToString(SaveOptions.DisableFormatting));
            node1 = serializer.Serialize(list);
            Assert.Equal("<TestClassSimples><TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></TestClassSimple><TestClassSimple><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></TestClassSimple></TestClassSimples>", node1.ToString(SaveOptions.DisableFormatting));

            node1 = serializer.Serialize(new String[] { "Un", "Deux", "Trois" });
            Assert.Equal("<Strings><String>Un</String><String>Deux</String><String>Trois</String></Strings>", node1.ToString(SaveOptions.DisableFormatting));

            var node2 = new XElement("root");
            serializer.Serialize(array, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root></root>", node2.ToString(SaveOptions.DisableFormatting));
            node2 = new XElement("root");
            serializer.Serialize(list, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root></root>", node2.ToString(SaveOptions.DisableFormatting));

            node2 = new XElement("roots");
            serializer.Serialize(new String[] { "Un", "Deux", "Trois" }, node2);
            Assert.Equal("<roots><root>Un</root><root>Deux</root><root>Trois</root></roots>", node2.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestSerializeNonTypedEnumerable()
        {
            var serializer = new XDocSerializer();

            DateTime dt = new DateTime(2014, 6, 8, 11, 44, 56);
            var array = new object[]{
                new TestClassSimple() {
                    Value1 = null,
                    Value2 = 23,
                    Value3 = 67.89,
                    Value4 = dt
                },new TestClassSimple() {
                    Value1 = "Texte",
                    Value2 = 98,
                    Value3 = -12.67,
                    Value4 = dt
                }
            };
            System.Collections.IEnumerable list = new List<Object>(array);

            var node1 = serializer.Serialize(array);
            Assert.Equal("<Objects><Object><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></Object><Object><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></Object></Objects>", node1.ToString(SaveOptions.DisableFormatting));
            node1 = serializer.Serialize(list);
            Assert.Equal("<Items><Item><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></Item><Item><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></Item></Items>", node1.ToString(SaveOptions.DisableFormatting));

            var node2 = new XElement("root");
            serializer.Serialize(array, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root></root>", node2.ToString(SaveOptions.DisableFormatting));
            node2 = new XElement("root");
            serializer.Serialize(list, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4><Value5>1</Value5></root></root>", node2.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestSerializeDictionary()
        {
            var target = new XDocSerializer();

            var dic1 = new Dictionary<String, object>();
            dic1["val1"] = "ABC";
            dic1["value1"] = "Text";
            dic1["VALUE2"] = 987;
            dic1["value3"] = 123.456;
            dic1["value4"] = new DateTime(2014, 6, 8, 11, 44, 56);
            dic1["value5"] = new TestClassSimple() {
                Value1 = "Un",
                Value2 = 2,
                Value3 = 2.4,
                Value4 = new DateTime(2014, 6, 8, 11, 44, 56)
            };
            var dic2 = new Dictionary<String, object>();
            dic2["val1"] = "Un";
            dic2["val2"] = 4;
            dic2["val3"] = 78.890;
            dic2["val4"] = new DateTime(2014, 6, 8, 11, 44, 56);
            dic1["value6"] = dic2;

            var ser = target.Serialize(dic1, "root").ToString(SaveOptions.DisableFormatting);

            Assert.Equal(
                "<root>" +
                    "<val1>ABC</val1>" +
                    "<value1>Text</value1>" +
                    "<VALUE2>987</VALUE2>" +
                    "<value3>123.456</value3>" +
                    "<value4>2014-06-08 11:44:56Z</value4>" +
                    "<value5>" +
                        "<Value3>2.4</Value3>" +
                        "<Value1>Un</Value1>" +
                        "<Value2>2</Value2>" +
                        "<Value4>2014-06-08 11:44:56Z</Value4>" +
                        "<Value5>1</Value5>" +
                    "</value5>" +
                    "<value6>" +
                        "<val1>Un</val1>" +
                        "<val2>4</val2>" +
                        "<val3>78.89</val3>" +
                        "<val4>2014-06-08 11:44:56Z</val4>" +
                    "</value6>" +
                "</root>",
                ser
                );

        }

        [Fact]
        public void TestAnonymousObject()
        {
            var target = new XDocSerializer();

            var obj1 = new {
                val1 = "ABC",
                value1 = "Text",
                VALUE2 = 987,
                value3 = 123.456,
                value4 = new DateTime(2014, 6, 8, 11, 44, 56),
                value5 = new TestClassSimple() {
                    Value1 = "Un",
                    Value2 = 2,
                    Value3 = 2.4,
                    Value4 = new DateTime(2014, 6, 8, 11, 44, 56)
                },
                value6 = new {
                    val1 = "Un",
                    val2 = 4,
                    val3 = 78.89,
                    val4 = new DateTime(2014, 6, 8, 11, 44, 56)
                }
            };

            var ser = target.Serialize(obj1).ToString(SaveOptions.DisableFormatting);

            Assert.Equal(
                "<f__AnonymousType17>" +
                    "<val1>ABC</val1>" +
                    "<value1>Text</value1>" +
                    "<VALUE2>987</VALUE2>" +
                    "<value3>123.456</value3>" +
                    "<value4>2014-06-08 11:44:56Z</value4>" +
                    "<value5>" +
                        "<Value3>2.4</Value3>" +
                        "<Value1>Un</Value1>" +
                        "<Value2>2</Value2>" +
                        "<Value4>2014-06-08 11:44:56Z</Value4>" +
                        "<Value5>1</Value5>" +
                    "</value5>" +
                    "<value6>" +
                        "<val1>Un</val1>" +
                        "<val2>4</val2>" +
                        "<val3>78.89</val3>" +
                        "<val4>2014-06-08 11:44:56Z</val4>" +
                    "</value6>" +
                "</f__AnonymousType17>",
                ser
                );

            ser = target.Serialize(obj1, "root").ToString(SaveOptions.DisableFormatting);

            Assert.Equal(
                "<root>" +
                    "<val1>ABC</val1>" +
                    "<value1>Text</value1>" +
                    "<VALUE2>987</VALUE2>" +
                    "<value3>123.456</value3>" +
                    "<value4>2014-06-08 11:44:56Z</value4>" +
                    "<value5>" +
                        "<Value3>2.4</Value3>" +
                        "<Value1>Un</Value1>" +
                        "<Value2>2</Value2>" +
                        "<Value4>2014-06-08 11:44:56Z</Value4>" +
                        "<Value5>1</Value5>" +
                    "</value5>" +
                    "<value6>" +
                        "<val1>Un</val1>" +
                        "<val2>4</val2>" +
                        "<val3>78.89</val3>" +
                        "<val4>2014-06-08 11:44:56Z</val4>" +
                    "</value6>" +
                "</root>",
                ser
                );

        }

        #endregion

        #region Populate

        [Fact]
        public void TestPopulate()
        {
            var serializer = new XDocSerializer();

            var val = new TestClassSimple();
            serializer.Populate(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root, val);
            Assert.Equal("Text", val.Value1);
            Assert.Equal(987, val.Value2);
            Assert.Equal(123.456, val.Value3);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), val.Value4);
            Assert.Equal(1, val.Value5);

            val = new TestClassSimple();
            serializer.Populate("<root value1=\"Text\" VALUE2=\"987\" value3=\"123.456\" value4=\"2014-06-08 11:44:56\" value5=\"123\" />", val);
            Assert.Equal("Text", val.Value1);
            Assert.Equal(987, val.Value2);
            Assert.Equal(123.456, val.Value3);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), val.Value4);
            Assert.Equal(1, val.Value5);

        }

        [Fact]
        public void TestPopulateList()
        {
            var serializer = new XDocSerializer();

            List<String> list1 = new List<string>();
            serializer.Populate(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root, list1);
            Assert.Equal(5, list1.Count);
            Assert.Equal("Text", list1[0]);
            Assert.Equal("987", list1[1]);
            Assert.Equal("123.456", list1[2]);
            Assert.Equal("2014-06-08 11:44:56", list1[3]);
            Assert.Equal("123", list1[4]);

        }

        [Fact]
        public void TestPopulateArray()
        {
            var serializer = new XDocSerializer();

            String[] arr1 = new String[3];
            serializer.Populate(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root, arr1);
            Assert.Equal("Text", arr1[0]);
            Assert.Equal("987", arr1[1]);
            Assert.Equal("123.456", arr1[2]);

            arr1 = new String[8];
            serializer.Populate("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>", arr1);
            Assert.Equal("Text", arr1[0]);
            Assert.Equal("987", arr1[1]);
            Assert.Equal("123.456", arr1[2]);
            Assert.Equal("2014-06-08 11:44:56", arr1[3]);
            Assert.Equal("123", arr1[4]);
            Assert.Equal(null, arr1[5]);
            Assert.Equal(null, arr1[6]);
            Assert.Equal(null, arr1[7]);

            // Null arguments
            Assert.Throws<ArgumentNullException>(() => serializer.Populate((XElement)null, ""));
            Assert.Throws<ArgumentNullException>(() => serializer.Populate(XDocument.Parse("<root><value1>Text</value1></root>").Root, null));
            // With string
            Assert.Throws<ArgumentException>(() => serializer.Populate(XDocument.Parse("<root><value1>Text</value1></root>").Root, ""));
            // With value type
            Assert.Throws<ArgumentException>(() => serializer.Populate(XDocument.Parse("<root><value1>Text</value1></root>").Root, 1234));
            // With Enum
            Assert.Throws<ArgumentException>(() => serializer.Populate(XDocument.Parse("<root><value1>Text</value1></root>").Root, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestPopulateDictionary()
        {
            var serializer = new XDocSerializer();

            var val = new Dictionary<String, object>(StringComparer.OrdinalIgnoreCase);
            serializer.Populate(XDocument.Parse(
                @"<root val1=""ABC"">
<value1>Text</value1>
<VALUE2>987</VALUE2>
<value3>123.456</value3>
<value4>2014-06-08 11:44:56</value4>
<value5>
    <val1>Un</val1>
    <val2>2</val2>
    <val3>2.4</val3>
    <val4>2014-06-08 11:44:56</val4>
</value5>
</root>").Root,
            val);
            Assert.Equal(
                new string[] { "val1", "value1", "VALUE2", "value3", "value4", "value5" },
                val.Keys.ToArray()
                );
            Assert.Equal("ABC", val["val1"]);
            Assert.Equal("Text", val["value1"]);
            Assert.Equal((Int64)987, val["value2"]);
            Assert.Equal(123.456m, val["value3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), val["value4"]);
            Assert.IsType<Dictionary<String, Object>>(val["value5"]);

            var dic2 = (Dictionary<String, Object>)val["value5"];
            Assert.Equal("Un", dic2["val1"]);
            Assert.Equal((Int64)2, dic2["val2"]);
            Assert.Equal(2.4m, dic2["val3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), dic2["val4"]);
        }

        #endregion

        #region Deserialization : simple values

        [Fact]
        public void TestDeserializeError()
        {
            var serializer = new XDocSerializer();
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize<String>((XElement)null));
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize((XElement)null));
        }

        [Fact]
        public void TestDeserializeString()
        {
            var serializer = new XDocSerializer();
            Assert.Equal("Test", serializer.Deserialize<String>(XDocument.Parse("<root>Test</root>").Root));
            Assert.Equal("Test", serializer.Deserialize(XDocument.Parse("<root>Test</root>").Root));
        }

        [Fact]
        public void TestDeserializeBool()
        {
            var serializer = new XDocSerializer();
            Assert.Equal(false, serializer.Deserialize<bool>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal(true, serializer.Deserialize<bool>(XDocument.Parse("<root>TRUE</root>").Root));
            Assert.Equal(false, serializer.Deserialize<bool>(XDocument.Parse("<root>false</root>").Root));
            Assert.Equal(null, serializer.Deserialize<bool?>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal(true, serializer.Deserialize<bool?>(XDocument.Parse("<root>true</root>").Root));
            Assert.Equal(false, serializer.Deserialize<bool?>(XDocument.Parse("<root>FALSE</root>").Root));
            Assert.Equal(true, serializer.Deserialize(XDocument.Parse("<root>true</root>").Root));
            Assert.Equal(false, serializer.Deserialize("<root>false</root>"));
        }

        [Fact]
        public void TestDeserializeInt()
        {
            var serializer = new XDocSerializer();
            // Int16
            Assert.Equal((Int16)(-16), serializer.Deserialize<Int16>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal((Int16)(-16), serializer.Deserialize(XDocument.Parse("<root>-16</root>").Root, typeof(Int16)));
            Assert.Equal(0, serializer.Deserialize<Int16>(XDocument.Parse("<root>test</root>").Root));

            // Int16?
            Assert.Equal((Int16?)-16, serializer.Deserialize<Int16?>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal((Int16?)-16, serializer.Deserialize("<root>-16</root>", typeof(Int16?)));
            Assert.Equal(null, serializer.Deserialize<Int16?>(XDocument.Parse("<root>test</root>").Root));

            // Int32
            Assert.Equal(32, serializer.Deserialize<Int32>(XDocument.Parse("<root>32</root>").Root));
            Assert.Equal(0, serializer.Deserialize<Int32>(XDocument.Parse("<root></root>").Root));

            // Int32?
            Assert.Equal(32, serializer.Deserialize<Int32?>(XDocument.Parse("<root>32</root>").Root));
            Assert.Equal(null, serializer.Deserialize<Int32?>(XDocument.Parse("<root>test</root>").Root));

            // Int64
            Assert.Equal(64, serializer.Deserialize<Int64>(XDocument.Parse("<root>64</root>").Root));
            Assert.Equal(0, serializer.Deserialize<Int64>(XDocument.Parse("<root>test</root>").Root));

            // Int64?
            Assert.Equal(64, serializer.Deserialize<Int64?>(XDocument.Parse("<root>64</root>").Root));
            Assert.Equal(null, serializer.Deserialize<Int64?>(XDocument.Parse("<root>test</root>").Root));

            // Non typed
            Assert.Equal((Int64)(-16), serializer.Deserialize(XDocument.Parse("<root>-16</root>").Root));
        }

        [Fact]
        public void TestDeserializeUInt()
        {
            var serializer = new XDocSerializer();
            // UInt16
            Assert.Equal(16, serializer.Deserialize<UInt16>(XDocument.Parse("<root>16</root>").Root));
            Assert.Equal(0, serializer.Deserialize<UInt16>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal(0, serializer.Deserialize<UInt16>(XDocument.Parse("<root>test</root>").Root));

            // UInt16?
            Assert.Equal((UInt16?)16, serializer.Deserialize<UInt16?>(XDocument.Parse("<root>16</root>").Root));
            Assert.Equal(null, serializer.Deserialize<UInt16?>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal(null, serializer.Deserialize<UInt16?>(XDocument.Parse("<root>test</root>").Root));

            // UInt32
            Assert.Equal((UInt32)32, serializer.Deserialize<UInt32>(XDocument.Parse("<root>32</root>").Root));
            Assert.Equal((UInt32)0, serializer.Deserialize<UInt32>(XDocument.Parse("<root></root>").Root));

            // UInt32?
            Assert.Equal((UInt32?)32, serializer.Deserialize<UInt32?>(XDocument.Parse("<root>32</root>").Root));
            Assert.Equal(null, serializer.Deserialize<UInt32?>(XDocument.Parse("<root>test</root>").Root));

            // UInt64
            Assert.Equal((UInt64)64, serializer.Deserialize<UInt64>(XDocument.Parse("<root>64</root>").Root));
            Assert.Equal((UInt64)0, serializer.Deserialize<UInt64>(XDocument.Parse("<root>test</root>").Root));

            // UInt64?
            Assert.Equal((UInt64?)64, serializer.Deserialize<UInt64?>(XDocument.Parse("<root>64</root>").Root));
            Assert.Equal(null, serializer.Deserialize<UInt64?>(XDocument.Parse("<root>test</root>").Root));
        }

        [Fact]
        public void TestDeserializeFloat()
        {
            var serializer = new XDocSerializer();
            // Single
            Assert.Equal((Single)(-16), serializer.Deserialize<Single>(XDocument.Parse("<root>-16</root>").Root));
            Assert.Equal(0, serializer.Deserialize<Single>(XDocument.Parse("<root>test</root>").Root));

            // Single?
            Assert.Equal((Single)123.45, serializer.Deserialize<Single?>(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal((Single)12345, serializer.Deserialize<Single?>(XDocument.Parse("<root>123,45</root>").Root));
            Assert.Equal(null, serializer.Deserialize<Single?>(XDocument.Parse("<root>test</root>").Root));

            // Double
            Assert.Equal(123.45, serializer.Deserialize<Double>(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal(0, serializer.Deserialize<Double>(XDocument.Parse("<root></root>").Root));

            // Double?
            Assert.Equal(123.45, serializer.Deserialize<Double?>(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal(null, serializer.Deserialize<Double?>(XDocument.Parse("<root>test</root>").Root));

            // Devimal
            Assert.Equal(123.45m, serializer.Deserialize<Decimal>(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal(0, serializer.Deserialize<Decimal>(XDocument.Parse("<root>test</root>").Root));

            // Decimal?
            Assert.Equal(123.45m, serializer.Deserialize<Decimal?>(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal(null, serializer.Deserialize<Decimal?>(XDocument.Parse("<root>test</root>").Root));

            // Non typed
            Assert.Equal(123.45m, serializer.Deserialize(XDocument.Parse("<root>123.45</root>").Root));
            Assert.Equal(12345L, serializer.Deserialize(XDocument.Parse("<root>123,45</root>").Root));

            // Not invariant culture
            serializer.Culture = CultureInfo.GetCultureInfo("fr-fr");
            Assert.Equal((Single)123.45, serializer.Deserialize<Single?>(XDocument.Parse("<root>123,45</root>").Root));
            Assert.Equal(123.45m, serializer.Deserialize(XDocument.Parse("<root>123,45</root>").Root));

        }

        [Fact]
        public void TestDeserializeDateTime()
        {
            var serializer = new XDocSerializer();
            // DateTime
            Assert.Equal(new DateTime(2014, 8, 6), serializer.Deserialize<DateTime>(XDocument.Parse("<root>2014/08/06</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize<DateTime>(XDocument.Parse("<root>2014/08/06 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8), serializer.Deserialize<DateTime>(XDocument.Parse("<root>2014-06-08</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize<DateTime>(XDocument.Parse("<root>2014-06-08 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize<DateTime>(XDocument.Parse("<root>08/06/2014 11:44:56</root>").Root));
            Assert.Equal(DateTime.MinValue, serializer.Deserialize<DateTime>(XDocument.Parse("<root>test</root>").Root));

            // DateTime?
            Assert.Equal(new DateTime(2014, 8, 6), serializer.Deserialize<DateTime?>(XDocument.Parse("<root>2014/08/06</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize<DateTime?>(XDocument.Parse("<root>2014/08/06 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8), serializer.Deserialize<DateTime?>(XDocument.Parse("<root>2014-06-08</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize<DateTime?>(XDocument.Parse("<root>2014-06-08 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize<DateTime?>(XDocument.Parse("<root>08/06/2014 11:44:56</root>").Root));
            Assert.Equal(null, serializer.Deserialize<DateTime?>(XDocument.Parse("<root>test</root>").Root));

            // Non typed
            Assert.Equal(new DateTime(2014, 8, 6), serializer.Deserialize(XDocument.Parse("<root>2014/08/06</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize(XDocument.Parse("<root>2014/08/06 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8), serializer.Deserialize(XDocument.Parse("<root>2014-06-08</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize(XDocument.Parse("<root>2014-06-08 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 8, 6, 11, 44, 56), serializer.Deserialize(XDocument.Parse("<root>08/06/2014 11:44:56</root>").Root));

            // Not invariant culture
            serializer.Culture = CultureInfo.GetCultureInfo("fr-fr");
            Assert.Equal(new DateTime(2014, 8, 6), serializer.Deserialize<DateTime>(XDocument.Parse("<root>2014/08/06</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize(XDocument.Parse("<root>2014-06-08 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize<DateTime>(XDocument.Parse("<root>08/06/2014 11:44:56</root>").Root));
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), serializer.Deserialize(XDocument.Parse("<root>08/06/2014 11:44:56</root>").Root));
        }

        #endregion

        #region Deserialization : object

        [Fact]
        public void TestDeserializeClass()
        {
            var serializer = new XDocSerializer();

            var val = serializer.Deserialize<TestClassSimple>(XDocument.Parse("<root>2014/08/06</root>").Root);
            Assert.Equal(null, val.Value1);
            Assert.Equal(0, val.Value2);
            Assert.Equal(0.0, val.Value3);
            Assert.Equal(DateTime.MinValue, val.Value4);

            val = serializer.Deserialize<TestClassSimple>(XDocument.Parse(@"
<root>
    <value1>Text</value1>
    <VALUE2>987</VALUE2>
    <value3>123.456</value3>
    <value4>2014-06-08 11:44:56</value4>
    <value5>123</value5>
    <value7>
        <value1>DEUX</value1>
        <value2>3</value2>
        <value3>4.5</value3>
        <value4>2014-12-03 19:43:56</value4>
    </value7>
</root>
").Root);
            Assert.Equal("Text", val.Value1);
            Assert.Equal(987, val.Value2);
            Assert.Equal(123.456, val.Value3);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), val.Value4);
            Assert.Equal(1, val.Value5);
            Assert.NotNull(val.Value7);
            Assert.Equal("DEUX", val.Value7.Value1);
            Assert.Equal(3, val.Value7.Value2);
            Assert.Equal(4.5, val.Value7.Value3);
            Assert.Equal(new DateTime(2014, 12, 3, 19, 43, 56), val.Value7.Value4);
            Assert.Equal(1, val.Value7.Value5);

            val = serializer.Deserialize<TestClassSimple>("<root value1=\"Text\" VALUE2=\"987\" value3=\"123.456\" value4=\"2014-06-08 11:44:56\" value5=\"123\" />");
            Assert.Equal("Text", val.Value1);
            Assert.Equal(987, val.Value2);
            Assert.Equal(123.456, val.Value3);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), val.Value4);
            Assert.Equal(1, val.Value5);

            // Null arguments
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize((XElement)null, typeof(TestClassSimple)));
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize(XDocument.Parse("<root><value1>Text</value1></root>").Root, null));

            // Attribute to object
            Exception ex = Assert.Throws<ArgumentException>(() => serializer.Deserialize<TestClassSimple>(XDocument.Parse("<root value7=\"123\"><value1>Text</value1></root>").Root));
            Assert.Equal("Type 'deuxsucres.XSerializer.Tests.TestClasses.TestClassSimple' can't be deserialized from an attribute value.", ex.Message);
            ex = Assert.Throws<ArgumentException>(() => serializer.Deserialize<TestClassSimple>(XDocument.Parse("<root value8=\"123\"><value1>Text</value1></root>").Root));
            Assert.Equal("Type 'deuxsucres.XSerializer.Tests.TestClasses.TestClassSimple' can't be deserialized from an attribute value.", ex.Message);
        }

        [Fact]
        public void TestDeserializeList()
        {
            var serializer = new XDocSerializer();

            IList<String> list1 = serializer.Deserialize<IList<String>>(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root);
            Assert.IsType<List<String>>(list1);
            Assert.Equal(5, list1.Count);
            Assert.Equal("Text", list1[0]);
            Assert.Equal("987", list1[1]);
            Assert.Equal("123.456", list1[2]);
            Assert.Equal("2014-06-08 11:44:56", list1[3]);
            Assert.Equal("123", list1[4]);

            List<String> list2 = serializer.Deserialize<List<String>>(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root);
            Assert.Equal(5, list2.Count);
            Assert.Equal("Text", list2[0]);
            Assert.Equal("987", list2[1]);
            Assert.Equal("123.456", list2[2]);
            Assert.Equal("2014-06-08 11:44:56", list2[3]);
            Assert.Equal("123", list2[4]);

            Collection<String> list3 = serializer.Deserialize<Collection<String>>(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root);
            Assert.Equal(5, list3.Count);
            Assert.Equal("Text", list3[0]);
            Assert.Equal("987", list3[1]);
            Assert.Equal("123.456", list3[2]);
            Assert.Equal("2014-06-08 11:44:56", list3[3]);
            Assert.Equal("123", list3[4]);
        }

        [Fact]
        public void TestDeserializeArray()
        {
            var serializer = new XDocSerializer();

            String[] arr1 = serializer.Deserialize<String[]>(XDocument.Parse("<root><value1>Text</value1><VALUE2>987</VALUE2><value3>123.456</value3><value4>2014-06-08 11:44:56</value4><value5>123</value5></root>").Root);
            Assert.Equal(5, arr1.Length);
            Assert.Equal("Text", arr1[0]);
            Assert.Equal("987", arr1[1]);
            Assert.Equal("123.456", arr1[2]);
            Assert.Equal("2014-06-08 11:44:56", arr1[3]);
            Assert.Equal("123", arr1[4]);

        }

        [Fact]
        public void TestDeserializeDictionary()
        {
            var serializer = new XDocSerializer();

            var dic1 = serializer.Deserialize<Dictionary<String, object>>(XDocument.Parse(
                @"<root val1=""ABC"">
<value1>Text</value1>
<VALUE2>987</VALUE2>
<value3>123.456</value3>
<value4>2014-06-08 11:44:56</value4>
<value5>
    <val1>Un</val1>
    <val2>2</val2>
    <val3>2.4</val3>
    <val4>2014-06-08 11:44:56</val4>
</value5>
</root>").Root);
            Assert.Equal(
                new string[] { "val1", "value1", "VALUE2", "value3", "value4", "value5" },
                dic1.Keys.ToArray()
                );
            Assert.Equal("ABC", dic1["val1"]);
            Assert.Equal("Text", dic1["value1"]);
            Assert.Equal((Int64)987, dic1["VALUE2"]);
            Assert.Equal(123.456m, dic1["value3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), dic1["value4"]);
            Assert.IsType<Dictionary<String, Object>>(dic1["value5"]);

            var sdic1 = (Dictionary<String, Object>)dic1["value5"];
            Assert.Equal("Un", sdic1["val1"]);
            Assert.Equal((Int64)2, sdic1["val2"]);
            Assert.Equal(2.4m, sdic1["val3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), sdic1["val4"]);

            var dic2 = serializer.Deserialize<IDictionary<String, object>>(
                @"<root val1=""ABC"">
<value1>Text</value1>
<VALUE2>987</VALUE2>
<value3>123.456</value3>
<value4>2014-06-08 11:44:56</value4>
<value5>
    <val1 type=""string"">Un</val1>
    <val2 type=""int"">2</val2>
    <val3 type=""float"">2.4</val3>
    <val4 type=""date"">2014-06-08 11:44:56</val4>
</value5>
</root>");
            Assert.Equal(
                new string[] { "val1", "value1", "VALUE2", "value3", "value4", "value5" },
                dic1.Keys.ToArray()
                );
            Assert.Equal("ABC", dic2["val1"]);
            Assert.Equal("Text", dic2["value1"]);
            Assert.Equal((Int64)987, dic2["value2"]);
            Assert.Equal(123.456m, dic2["value3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), dic2["value4"]);
            Assert.IsType<Dictionary<String, Object>>(dic2["value5"]);

            sdic1 = (Dictionary<String, Object>)dic2["value5"];
            Assert.Equal("Un", sdic1["val1"]);
            Assert.Equal((Int64)2, sdic1["val2"]);
            Assert.Equal(2.4m, sdic1["val3"]);
            Assert.Equal(new DateTime(2014, 6, 8, 11, 44, 56), sdic1["val4"]);
        }

        #endregion

    }
}
