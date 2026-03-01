using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class BugFixAndFeatureTests
    {
        // === A: Formatting State Stack ===

        [TestMethod]
        public void NestedBlockquotes_IndentIsAdditive()
        {
            var html = "<blockquote><blockquote>Deep quote</blockquote></blockquote>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            // Outer = 720, Inner = 1440
            Assert.IsTrue(rtf.Contains("\\li720 "));
            Assert.IsTrue(rtf.Contains("\\li1440 "));
            Assert.IsTrue(rtf.Contains("Deep quote"));
        }

        [TestMethod]
        public void HeadingInsideDiv_FontSizeRestored()
        {
            var html = "<p>Before</p><h1>Title</h1><p>After</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\fs48 ")); // h1 = 24pt
            // After heading, font size should be restored to 24 (12pt)
            Assert.IsTrue(rtf.Contains("\\fs24 "));
            Assert.IsTrue(rtf.Contains("Title"));
        }

        [TestMethod]
        public void NestedSpanFontSize_RestoredAfterInner()
        {
            var html = "<span style=\"font-size: 18pt\">Outer <span style=\"font-size: 10pt\">Inner</span> Still Outer</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\fs36 ")); // 18pt
            Assert.IsTrue(rtf.Contains("\\fs20 ")); // 10pt
            Assert.IsTrue(rtf.Contains("Outer "));
            Assert.IsTrue(rtf.Contains("Inner"));
            Assert.IsTrue(rtf.Contains("Still Outer"));
        }

        [TestMethod]
        public void ListItemWithNestedList_IndentRestoredCorrectly()
        {
            var html = @"
                <ul>
                    <li>Parent
                        <ul>
                            <li>Child</li>
                        </ul>
                    </li>
                    <li>Sibling</li>
                </ul>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("Parent"));
            Assert.IsTrue(rtf.Contains("Child"));
            Assert.IsTrue(rtf.Contains("Sibling"));
            Assert.IsTrue(rtf.Contains("\\li360 "));
            Assert.IsTrue(rtf.Contains("\\li720 "));
        }

        // === B: RTF Hyperlinks ===

        [TestMethod]
        public void Anchor_EmitsRtfHyperlinkField()
        {
            var html = "<a href=\"https://example.com\">Click here</a>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("HYPERLINK"));
            Assert.IsTrue(rtf.Contains("https://example.com"));
            Assert.IsTrue(rtf.Contains("\\fldrslt"));
            Assert.IsTrue(rtf.Contains("Click here"));
        }

        [TestMethod]
        public void Anchor_NoHref_RendersChildrenOnly()
        {
            var html = "<a>Just text</a>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("Just text"));
            Assert.IsFalse(rtf.Contains("HYPERLINK"));
        }

        [TestMethod]
        public void Anchor_HasBlueColorAndUnderline()
        {
            var html = "<a href=\"https://example.com\">Link</a>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\ul "));
            // Blue color should be registered
            Assert.IsTrue(rtf.Contains("\\red0\\green0\\blue255"));
        }

        // === C: CSS font-weight, font-style, text-decoration ===

        [TestMethod]
        public void SpanFontWeightBold_EmitsBold()
        {
            var html = "<span style=\"font-weight: bold\">Bold text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("\\b0 "));
            Assert.IsTrue(rtf.Contains("Bold text"));
        }

        [TestMethod]
        public void SpanFontWeight700_EmitsBold()
        {
            var html = "<span style=\"font-weight: 700\">Bold text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\b "));
        }

        [TestMethod]
        public void SpanFontStyleItalic_EmitsItalic()
        {
            var html = "<span style=\"font-style: italic\">Italic text</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\i "));
            Assert.IsTrue(rtf.Contains("\\i0 "));
        }

        [TestMethod]
        public void SpanTextDecorationUnderline_EmitsUnderline()
        {
            var html = "<span style=\"text-decoration: underline\">Underlined</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\ul "));
            Assert.IsTrue(rtf.Contains("\\ulnone "));
        }

        [TestMethod]
        public void SpanTextDecorationLineThrough_EmitsStrike()
        {
            var html = "<span style=\"text-decoration: line-through\">Struck</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\strike "));
            Assert.IsTrue(rtf.Contains("\\strike0 "));
        }

        [TestMethod]
        public void ParagraphWithColorStyle_AppliesColor()
        {
            var html = "<p style=\"color: red\">Red paragraph</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
            Assert.IsTrue(rtf.Contains("\\cf"));
            Assert.IsTrue(rtf.Contains("Red paragraph"));
        }

        [TestMethod]
        public void ParagraphWithFontSize_AppliesFontSize()
        {
            var html = "<p style=\"font-size: 18pt\">Big paragraph</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\fs36 ")); // 18pt = 36 half-points
        }

        [TestMethod]
        public void DivWithFontWeightBold_EmitsBold()
        {
            var html = "<div style=\"font-weight: bold\">Bold div</div>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("Bold div"));
        }

        [TestMethod]
        public void ParagraphWithBackgroundColor_AppliesBackground()
        {
            var html = "<p style=\"background-color: #ffff00\">Highlighted</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\chcbpat"));
        }

        // === D: New HTML Tags ===

        [TestMethod]
        public void CodeTag_UsesMonospaceFont()
        {
            var html = "Use <code>console.log()</code> to debug";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\f1 ")); // Courier New
            Assert.IsTrue(rtf.Contains("console.log()"));
            Assert.IsTrue(rtf.Contains("\\f0 ")); // back to Arial
        }

        [TestMethod]
        public void KbdTag_UsesMonospaceFont()
        {
            var html = "Press <kbd>Ctrl+C</kbd>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\f1 "));
            Assert.IsTrue(rtf.Contains("Ctrl+C"));
        }

        [TestMethod]
        public void SampTag_UsesMonospaceFont()
        {
            var html = "Output: <samp>Hello</samp>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\f1 "));
        }

        [TestMethod]
        public void MarkTag_AppliesYellowBackground()
        {
            var html = "This is <mark>highlighted</mark> text";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\chcbpat"));
            Assert.IsTrue(rtf.Contains("\\red255\\green255\\blue0")); // yellow
            Assert.IsTrue(rtf.Contains("highlighted"));
        }

        // === E: RTF Spec Fixes ===

        [TestMethod]
        public void RtfHeader_ContainsUc1()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("test");
            Assert.IsTrue(rtf.Contains("\\uc1"));
        }

        [TestMethod]
        public void BackgroundColor_UsesChcbpat()
        {
            var html = "<span style=\"background-color: red\">Red bg</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\chcbpat"));
            Assert.IsFalse(rtf.Contains("\\highlight"));
        }

        [TestMethod]
        public void TableCells_HavePlainReset()
        {
            var html = "<table><tr><td>Cell</td></tr></table>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\plain\\f0\\fs24 "));
        }

        // === F: CssColorParser Improvements ===

        [TestMethod]
        public void CssColor_FourDigitHex_Parsed()
        {
            var html = "<span style=\"color: #f00a\">Red</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
        }

        [TestMethod]
        public void CssColor_EightDigitHex_Parsed()
        {
            var html = "<span style=\"color: #ff0000ff\">Red</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
        }

        [TestMethod]
        public void CssColor_SpaceSeparatedRgb_Parsed()
        {
            var html = "<span style=\"color: rgb(0 128 0)\">Green</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red0\\green128\\blue0"));
        }

        [TestMethod]
        public void CssColor_RgbWithSlashAlpha_Parsed()
        {
            var html = "<span style=\"color: rgb(0 128 0 / 0.5)\">Green</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red0\\green128\\blue0"));
        }

        [TestMethod]
        public void CssColor_PercentageRgb_Parsed()
        {
            var html = "<span style=\"color: rgb(100%, 0%, 0%)\">Red</span>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\red255\\green0\\blue0"));
        }
    }
}
