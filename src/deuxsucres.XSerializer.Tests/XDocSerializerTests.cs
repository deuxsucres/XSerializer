using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    }
}
