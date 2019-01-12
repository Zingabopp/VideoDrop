using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Util;
//using static INIReader;

namespace Tests
{
    [TestClass()]
    public class UtilTests
    {
        [TestMethod()]
        public void DirectoryToString_DirectoryInfoTest()
        {
            string result = "";
            System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(@"C:\Windows");
            result = DirectoryToString(dInfo);
            Assert.AreEqual(result, @"C:\Windows\");
            dInfo = new System.IO.DirectoryInfo(@"C:\Windows\");
            result = DirectoryToString(dInfo);
            Assert.AreEqual(result, @"C:\Windows\");
        }

        [TestMethod()]
        public void DirectoryToString_StringPathTest()
        {
            string result = "";
            string dInfo = @"C:\Windows";
            result = DirectoryToString(dInfo);
            Assert.AreEqual(result, @"C:\Windows\");
            dInfo = @"C:\Windows\";
            result = DirectoryToString(dInfo);
            Assert.AreEqual(result, @"C:\Windows\");
        }
    }
}