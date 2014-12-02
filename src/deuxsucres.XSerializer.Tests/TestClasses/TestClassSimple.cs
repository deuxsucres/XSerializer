using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace deuxsucres.XSerializer.Tests.TestClasses
{
    public class TestClassSimple
    {
        Int64 _Value5 = 1;
        public String Value1 { get; set; }
        public Int32 Value2 { get; set; }
        public Double Value3 = 0.0;
        public DateTime Value4 { get; set; }
        public Int64 Value5 { get { return _Value5; } }
        public String Value6 = null;
    }
}
