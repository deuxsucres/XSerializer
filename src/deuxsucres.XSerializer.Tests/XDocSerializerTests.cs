using deuxsucres.XSerializer.Tests.TestClasses;
using System;
using System.Collections.Generic;
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
            Assert.Equal("<TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></TestClassSimple>", node1.ToString(SaveOptions.DisableFormatting));
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
            Assert.Equal("<root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></root>", node2.ToString(SaveOptions.DisableFormatting));
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null, (XElement)null));
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(new TestClassSimple(), (XElement)null));
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
            Assert.Equal("<TestClassSimples><TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></TestClassSimple><TestClassSimple><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></TestClassSimple></TestClassSimples>", node1.ToString(SaveOptions.DisableFormatting));
            node1 = serializer.Serialize(list);
            Assert.Equal("<TestClassSimples><TestClassSimple><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></TestClassSimple><TestClassSimple><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></TestClassSimple></TestClassSimples>", node1.ToString(SaveOptions.DisableFormatting));

            var node2 = new XElement("root");
            serializer.Serialize(array, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></root></root>", node2.ToString(SaveOptions.DisableFormatting));
            node2 = new XElement("root");
            serializer.Serialize(list, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></root></root>", node2.ToString(SaveOptions.DisableFormatting));
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
            Assert.Equal("<Objects><Object><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></Object><Object><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></Object></Objects>", node1.ToString(SaveOptions.DisableFormatting));
            node1 = serializer.Serialize(list);
            Assert.Equal("<Items><Item><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></Item><Item><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></Item></Items>", node1.ToString(SaveOptions.DisableFormatting));

            var node2 = new XElement("root");
            serializer.Serialize(array, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></root></root>", node2.ToString(SaveOptions.DisableFormatting));
            node2 = new XElement("root");
            serializer.Serialize(list, node2);
            Assert.Equal("<root><root><Value3>67.89</Value3><Value2>23</Value2><Value4>2014-06-08 11:44:56Z</Value4></root><root><Value3>-12.67</Value3><Value1>Texte</Value1><Value2>98</Value2><Value4>2014-06-08 11:44:56Z</Value4></root></root>", node2.ToString(SaveOptions.DisableFormatting));
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

    }
}
