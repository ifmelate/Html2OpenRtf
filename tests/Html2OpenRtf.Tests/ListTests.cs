using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class ListTests
    {
        [TestMethod]
        public void ConvertHtmlToRtf_UnorderedList_ContainsBullets()
        {
            var html = "<ul><li>Apple</li><li>Banana</li></ul>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("Apple"));
            Assert.IsTrue(rtf.Contains("Banana"));
            // Should contain bullet character
            Assert.IsTrue(rtf.Contains("\\u8226?")); // Unicode bullet
        }

        [TestMethod]
        public void ConvertHtmlToRtf_OrderedList_ContainsNumbers()
        {
            var html = "<ol><li>First</li><li>Second</li><li>Third</li></ol>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("1. First"));
            Assert.IsTrue(rtf.Contains("2. Second"));
            Assert.IsTrue(rtf.Contains("3. Third"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_NestedList_IncreasesIndent()
        {
            var html = @"
                <ul>
                    <li>Parent
                        <ul>
                            <li>Child</li>
                        </ul>
                    </li>
                </ul>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("Parent"));
            Assert.IsTrue(rtf.Contains("Child"));
            // Should have different indent levels (\li360 and \li720)
            Assert.IsTrue(rtf.Contains("\\li360 "));
            Assert.IsTrue(rtf.Contains("\\li720 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_MixedList_HandlesOrderedInsideUnordered()
        {
            var html = @"
                <ul>
                    <li>Bullet
                        <ol>
                            <li>Numbered</li>
                        </ol>
                    </li>
                </ul>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("Bullet"));
            Assert.IsTrue(rtf.Contains("1. Numbered"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_ListWithFormatting_PreservesFormatting()
        {
            var html = "<ul><li><b>Bold item</b></li><li><i>Italic item</i></li></ul>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("Bold item"));
            Assert.IsTrue(rtf.Contains("\\i "));
            Assert.IsTrue(rtf.Contains("Italic item"));
        }
    }
}
