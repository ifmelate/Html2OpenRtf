using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class InlineStyleTests
    {
        [TestMethod]
        public void ConvertHtmlToRtf_SpanColor_RegistersColorAndApplies()
        {
            var html = "<span style=\"color: #ff0000\">Red text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
            Assert.IsTrue(rtf.Contains("\\cf"));
            Assert.IsTrue(rtf.Contains("Red text"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SpanColorNamed_RegistersColor()
        {
            var html = "<span style=\"color: blue\">Blue text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red0\\green0\\blue255"));
            Assert.IsTrue(rtf.Contains("Blue text"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SpanColorRgb_RegistersColor()
        {
            var html = "<span style=\"color: rgb(0, 128, 0)\">Green text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red0\\green128\\blue0"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SpanBackgroundColor_AppliesHighlight()
        {
            var html = "<span style=\"background-color: yellow\">Highlighted</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\chcbpat"));
            Assert.IsTrue(rtf.Contains("Highlighted"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SpanFontSizePt_AppliesFontSize()
        {
            var html = "<span style=\"font-size: 18pt\">Large</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\fs36 ")); // 18pt = 36 half-points
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SpanFontSizePx_AppliesFontSize()
        {
            var html = "<span style=\"font-size: 16px\">Medium</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            // 16px * 1.5 = 24 half-points = 12pt
            Assert.IsTrue(rtf.Contains("\\fs24 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_MultipleStyles_AllApplied()
        {
            var html = "<span style=\"color: red; font-size: 24pt\">Big red</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
            Assert.IsTrue(rtf.Contains("\\fs48 ")); // 24pt
            Assert.IsTrue(rtf.Contains("Big red"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_ShortHexColor_ParsesCorrectly()
        {
            var html = "<span style=\"color: #f00\">Red</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_TextAlign_AppliesAlignment()
        {
            var html = "<p style=\"text-align: center\">Centered</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\qc "));
        }
    }
}
