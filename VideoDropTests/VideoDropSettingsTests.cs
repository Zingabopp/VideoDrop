using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Util;
using static INIReader;

namespace VideoDrop.Tests
{
    [TestClass()]
    public class VideoDropSettingsTests
    {
        [TestMethod()]
        public void GetTags_NoTagsTest()
        {
            string line = "--colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300";
            // Test no tags, default bounds
            var tagList = VideoDropSettings.GetTags(line);
            Assert.IsTrue(tagList.Count == 0);
            // Test no tags, non-default bounds
            tagList = VideoDropSettings.GetTags(line, new string[] { "%", "%" });
            Assert.IsTrue(tagList.Count == 0);
        }

        [TestMethod()]
        public void GetTags_OneTagTest()
        {
            string startTag = "StartTag";
            string middleTag = "MiddleTag";
            string endTag = "EndTag";
            // One tag at end, non-default bounds
            string line = "--colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300 ^" + endTag + "^";
            Console.WriteLine(line);
            var tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Console.WriteLine(tagList.Count);
            Assert.IsTrue((tagList.Count == 1) && tagList.First() == endTag);

            // One tag at beginning, specified default bounds
            line = "<" + startTag + "> --colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue((tagList.Count == 1) && tagList.First() == startTag);

            // One tag at middle, default bounds
            line = "--colormatrix bt709 --range pc <" + middleTag + "> --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line);
            Assert.IsTrue((tagList.Count == 1) && tagList.First() == middleTag);

        }

        [TestMethod()]
        public void GetTags_HalfTagTests()
        {
            // Testing what happens if there's only one TagBound in the line
            string startTag = "StartTag";
            string middleTag = "MiddleTag";
            string endTag = "EndTag";
            string message = "";
            // One end bound at end, non-default bounds
            string line = "--colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300 " + endTag + "^";
            var tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Assert.IsTrue(tagList.Count == 0);

            // One start bound at end, non-default bounds
            line = "--colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300 ^" + endTag;
            tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Assert.IsTrue(tagList.Count == 0);

            // One start bound at beginning, specified default bounds
            line = "<" + startTag + " --colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 0);

            // One end bound at beginning, specified default bounds
            line = startTag + "> --colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 0);

            // One end bound at middle, default bounds
            line = "--colormatrix bt709 --range pc " + middleTag + "> --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line);
            Assert.IsTrue(tagList.Count == 0);

            // One start bound at middle, default bounds
            line = "--colormatrix bt709 --range pc <" + middleTag + " --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line);
            Assert.IsTrue(tagList.Count == 0);

            // Half tag at start, one tag at middle, default bounds
            line = "<" + startTag + " --colormatrix bt709 --range pc <" + middleTag + "> --input - range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line);
            Assert.IsTrue((tagList.Count == 1) && (tagList.First() == middleTag), message);

            // Half tag at start, one tag at end, non-default bounds
            line = "^" + startTag + "--colormatrix bt709 --range pc --input - range pc --seek 2500 --frames 300 ^" + endTag + "^";
            tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Assert.IsTrue((tagList.Count == 1) && (tagList.First() == endTag), message);

            // Start/Half-Middle/End, specified default bounds
            line = "<" + startTag + "> --colormatrix bt709 --range pc --input - range pc <" + middleTag + " --seek 2500 --frames 300 <" + endTag + ">";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 2 && tagList.Contains(startTag) && tagList.Contains(endTag));

            // Start/Half-Middle(Start)/Half-End(End), specified default bounds
            line = "<" + startTag + "> --colormatrix bt709 --range pc --input - range pc <" + middleTag + " --seek 2500 --frames 300 " + endTag + ">";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 1 && tagList.Contains(startTag));

        }

        [TestMethod()]
        public void GetTags_MultipleTagTests()
        {
            // Testing what happens if there's multiple tags in a line
            string startTag = "StartTag";
            string middleTag = "MiddleTag";
            string endTag = "EndTag";
            // Start/End tag, non-default bounds
            string line = "^" + startTag + "^ --colormatrix bt709 --range pc --input-range pc --seek 2500 --frames 300 ^" + endTag + "^";
            var tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Assert.IsTrue(tagList.Count == 2 && tagList.Contains(startTag) && tagList.Contains(endTag));

            // Start/Middle, non-default bounds
            line = "^" + startTag + "^ --colormatrix bt709 --range pc ^" + middleTag + "^ --input-range pc --seek 2500 --frames 300";
            tagList = VideoDropSettings.GetTags(line, new string[] { "^", "^" });
            Assert.IsTrue(tagList.Count == 2 && tagList.Contains(startTag) && tagList.Contains(middleTag));

            // Middle/End, specified default bounds
            line = "--colormatrix bt709 --range pc --input-range pc <" + middleTag + "> --seek 2500 --frames 300 <" + endTag + ">";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 2 && tagList.Contains(middleTag) && tagList.Contains(endTag));

            // Start/Middle/End, specified default bounds
            line = "<" + startTag + "> --colormatrix bt709 --range pc --input - range pc <" + middleTag + "> --seek 2500 --frames 300 <" + endTag + ">";
            tagList = VideoDropSettings.GetTags(line, new string[] { "<", ">" });
            Assert.IsTrue(tagList.Count == 3 && tagList.Contains(startTag) && tagList.Contains(middleTag) && tagList.Contains(endTag));


        }
    }
}