using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class BasicFormattingTests
    {
        [TestMethod]
        public void ConvertHtmlToRtf_NullInput_ReturnsValidRtf()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(null);
            Assert.IsTrue(rtf.StartsWith("{\\rtf1"));
            Assert.IsTrue(rtf.EndsWith("}"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_EmptyString_ReturnsValidRtf()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("");
            Assert.IsTrue(rtf.StartsWith("{\\rtf1"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_PlainText_ContainsText()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Hello World");
            Assert.IsTrue(rtf.Contains("Hello World"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Bold_ContainsBoldControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<b>Bold text</b>");
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("Bold text"));
            Assert.IsTrue(rtf.Contains("\\b0 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Strong_ContainsBoldControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<strong>Bold</strong>");
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("\\b0 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Italic_ContainsItalicControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<i>Italic text</i>");
            Assert.IsTrue(rtf.Contains("\\i "));
            Assert.IsTrue(rtf.Contains("\\i0 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Em_ContainsItalicControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<em>Emphasis</em>");
            Assert.IsTrue(rtf.Contains("\\i "));
            Assert.IsTrue(rtf.Contains("\\i0 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Underline_ContainsUnderlineControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<u>Underlined</u>");
            Assert.IsTrue(rtf.Contains("\\ul "));
            Assert.IsTrue(rtf.Contains("\\ulnone "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Strikethrough_ContainsStrikeControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<s>Deleted</s>");
            Assert.IsTrue(rtf.Contains("\\strike "));
            Assert.IsTrue(rtf.Contains("\\strike0 "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Del_ContainsStrikeControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<del>Removed</del>");
            Assert.IsTrue(rtf.Contains("\\strike "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Subscript_ContainsSubControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("H<sub>2</sub>O");
            Assert.IsTrue(rtf.Contains("\\sub "));
            Assert.IsTrue(rtf.Contains("\\nosupersub "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Superscript_ContainsSuperControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("x<sup>2</sup>");
            Assert.IsTrue(rtf.Contains("\\super "));
            Assert.IsTrue(rtf.Contains("\\nosupersub "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Paragraphs_ContainsParControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<p>First</p><p>Second</p>");
            Assert.IsTrue(rtf.Contains("First"));
            Assert.IsTrue(rtf.Contains("Second"));
            Assert.IsTrue(rtf.Contains("\\par"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_LineBreak_ContainsLineControl()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Line 1<br>Line 2");
            Assert.IsTrue(rtf.Contains("Line 1"));
            Assert.IsTrue(rtf.Contains("\\line "));
            Assert.IsTrue(rtf.Contains("Line 2"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_SelfClosingBr_ContainsLineControl()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Line 1<br/>Line 2");
            Assert.IsTrue(rtf.Contains("\\line "));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Heading1_ContainsBoldAndLargeFont()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<h1>Title</h1>");
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("\\fs48 ")); // 24pt in half-points
            Assert.IsTrue(rtf.Contains("Title"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Heading3_ContainsBoldAndMediumFont()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<h3>Subtitle</h3>");
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("\\fs28 ")); // 14pt
        }

        [TestMethod]
        public void ConvertHtmlToRtf_HorizontalRule_ContainsBorder()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<p>Before</p><hr><p>After</p>");
            Assert.IsTrue(rtf.Contains("\\brdr"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Blockquote_ContainsIndent()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<blockquote>Quoted text</blockquote>");
            Assert.IsTrue(rtf.Contains("\\li720 ")); // left indent
            Assert.IsTrue(rtf.Contains("Quoted text"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Pre_UsesMonospaceFont()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<pre>code here</pre>");
            Assert.IsTrue(rtf.Contains("\\f1 ")); // Courier New
            Assert.IsTrue(rtf.Contains("code here"));
            Assert.IsTrue(rtf.Contains("\\f0 ")); // back to Arial
        }

        [TestMethod]
        public void ConvertHtmlToRtf_NestedFormatting_ContainsBothControls()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<b><i>Bold and italic</i></b>");
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("\\i "));
            Assert.IsTrue(rtf.Contains("Bold and italic"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Anchor_ContainsTextAndUrl()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<a href=\"https://example.com\">Click here</a>");
            Assert.IsTrue(rtf.Contains("Click here"));
            Assert.IsTrue(rtf.Contains("https://example.com"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Image_ContainsPlaceholder()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<img src=\"photo.jpg\" alt=\"My photo\">");
            Assert.IsTrue(rtf.Contains("[image: My photo]"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_EscapesSpecialChars()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Price: {100} \\ done");
            Assert.IsTrue(rtf.Contains("\\{100\\}"));
            Assert.IsTrue(rtf.Contains("\\\\ done"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_HtmlEntities_AreDecoded()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("&lt;tag&gt; &amp; &quot;value&quot;");
            Assert.IsTrue(rtf.Contains("<tag>"));
            Assert.IsTrue(rtf.Contains("&"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_ScriptAndStyle_AreIgnored()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(
                "<style>body { color: red; }</style><script>alert('x')</script><p>Visible</p>");
            Assert.IsTrue(rtf.Contains("Visible"));
            Assert.IsFalse(rtf.Contains("alert"));
            Assert.IsFalse(rtf.Contains("body {"));
        }
    }
}
