using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Util;
//using static INIReader;

namespace UtilTests
{
    [TestClass()]
    public class FileSystemOpsTests
    {
        [TestMethod()]
        public void DirectoryToString_DirectoryInfoTest()
        {
            string result = "";
            System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(@"C:\Windows");
            result = DirectoryToString(dInfo);
            Assert.AreEqual(@"C:\Windows\", result);
            dInfo = new System.IO.DirectoryInfo(@"C:\Windows\");
            result = DirectoryToString(dInfo);
            Assert.AreEqual(@"C:\Windows\", result);
        }

        [TestMethod()]
        public void DirectoryToString_StringPathTest()
        {
            string result = "";
            string dInfo = @"C:\Windows";
            result = DirectoryToString(dInfo);
            Assert.AreEqual(@"C:\Windows\", result);
            dInfo = @"C:\Windows\";
            result = DirectoryToString(dInfo);
            Assert.AreEqual(@"C:\Windows\", result);
        }

        [TestMethod()]
        public void QuotePathTest()
        {
            string path = "";
            string result = "";

            path = @"C:\Windows\test.txt";
            result = QuotePath(path);
            Assert.AreEqual("\"C:\\Windows\\test.txt\"", result);

            path = @"\test.txt";
            result = QuotePath(path);
            Assert.AreEqual("\"\\test.txt\"", result);

            path = "\"C:\\Windows\\test.txt\"";
            result = QuotePath(path);
            Assert.AreEqual("\"C:\\Windows\\test.txt\"", result);
        }

        [TestMethod()]
        public void JoinPathsTest()
        {
            string path1, path2, expectedResult, result = "";

            path1 = @"C:\Windows";
            path2 = @"test\test.txt";
            expectedResult = path1 + "\\" + path2;
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path1 = @"C:\Windows\";
            path2 = @"test\test.txt";
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path1 = @"C:\Windows\";
            path2 = @"\test\test.txt";
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path1 = @"C:\Windows";
            path2 = @"test\test.txt";
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path1 = @"C:\Windows";
            path2 = "";
            expectedResult = path1;
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path2 = @"C:\Windows";
            path1 = "";
            expectedResult = path2;
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);

            path1 = "";
            path2 = "";
            expectedResult = "";
            result = JoinPaths(path1, path2);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod()]
        public void DirectoryExistsTest()
        {
            Assert.IsTrue(DirectoryExists(new System.IO.DirectoryInfo(@"C:\Windows")));
            Assert.IsFalse(DirectoryExists(new System.IO.DirectoryInfo(@"C:\Windows123456")));
            string nullStr = null;
            Assert.IsFalse(DirectoryExists(nullStr));

        }
    }

    [TestClass()]
    public class ParsingTests
    {
        [TestMethod()]
        public void ParseBool_Test()
        {
            string test;

            test = "true";
            Assert.IsTrue(ParseBool(test));
            test = "1 ";
            Assert.IsTrue(ParseBool(test));
            test = "Yes";
            Assert.IsTrue(ParseBool(test));

            test = "false";
            Assert.IsFalse(ParseBool(test));
            test = " 0 ";
            Assert.IsFalse(ParseBool(test));
            test = "nO";
            Assert.IsFalse(ParseBool(test));

            test = "random";
            Assert.IsFalse(ParseBool(test));
            test = null;
            Assert.IsFalse(ParseBool(test));
        }
    }
}