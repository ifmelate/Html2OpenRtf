using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;
using RtfPipe;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class RoundTripTests
    {
        static RoundTripTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [TestMethod]
        public void RoundTrip_PlainText_ContentPreserved()
        {
            var original = "Simple text content";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(original);
            var html = RtfToHtml(rtf);
            Assert.IsTrue(NormalizeText(html).Contains("Simple text content"));
        }

        [TestMethod]
        public void RoundTrip_BoldText_ContentPreserved()
        {
            var original = "<p>Normal and <b>bold</b> text</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(original);
            var html = RtfToHtml(rtf);
            var normalized = NormalizeText(html);
            Assert.IsTrue(normalized.Contains("Normal and"));
            Assert.IsTrue(normalized.Contains("bold"));
        }

        [TestMethod]
        public void RoundTrip_RussianText_ContentPreserved()
        {
            var original = "<p>Пациент жалуется на головную боль</p>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(original);
            var html = RtfToHtml(rtf);
            Assert.IsTrue(html.Contains("Пациент") || html.Contains("&#1055;") || NormalizeText(html).Contains("Пациент"));
        }

        [TestMethod]
        public void RoundTrip_CKEditorTypicalOutput_ContentPreserved()
        {
            var original = @"
                <p><strong>Patient Notes</strong></p>
                <p>The patient reports <em>mild discomfort</em> in the lower back region.</p>
                <ul>
                    <li>Pain level: 4/10</li>
                    <li>Duration: 3 days</li>
                </ul>
                <p>Prescribed: <u>Arnica montana 30C</u></p>";

            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(original);

            // Verify it's valid RTF that RtfPipe can parse
            var html = RtfToHtml(rtf);
            Assert.IsNotNull(html);

            var normalized = NormalizeText(html);
            Assert.IsTrue(normalized.Contains("Patient Notes"));
            Assert.IsTrue(normalized.Contains("mild discomfort"));
            Assert.IsTrue(normalized.Contains("Pain level"));
            Assert.IsTrue(normalized.Contains("Arnica montana"));
        }

        [TestMethod]
        public void RoundTrip_GeneratedRtfIsValidForRtfPipe()
        {
            var inputs = new[]
            {
                "<p>Simple paragraph</p>",
                "<b>Bold</b> and <i>italic</i>",
                "<h1>Heading</h1><p>Content</p>",
                "<ul><li>Item 1</li><li>Item 2</li></ul>",
                "<table><tr><td>Cell</td></tr></table>",
            };

            foreach (var input in inputs)
            {
                var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(input);

                // Should not throw — proves generated RTF is parseable
                var html = RtfToHtml(rtf);
                Assert.IsNotNull(html, $"RtfPipe returned null for input: {input}");
            }
        }

        private static string RtfToHtml(string rtf)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
            return Rtf.ToHtml(stream);
        }

        private static string NormalizeText(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            // Strip HTML tags
            var text = Regex.Replace(html, "<[^>]+>", " ");
            // Collapse whitespace
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }
    }
}
