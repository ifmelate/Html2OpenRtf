using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class UnicodeTests
    {
        [TestMethod]
        public void ConvertHtmlToRtf_RussianText_ContainsUnicodeEscapes()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Привет мир");
            // Each Cyrillic character should be \uN? where N is the Unicode code point
            // П = U+041F = 1055
            Assert.IsTrue(rtf.Contains("\\u1055?"));
            // р = U+0440 = 1088
            Assert.IsTrue(rtf.Contains("\\u1088?"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_CjkText_ContainsUnicodeEscapes()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("你好");
            // 你 = U+4F60 = 20320
            Assert.IsTrue(rtf.Contains("\\u20320?"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_MixedAsciiUnicode_BothPresent()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Hello Мир");
            Assert.IsTrue(rtf.Contains("Hello "));
            Assert.IsTrue(rtf.Contains("\\u"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_Emoji_ContainsUnicodeEscape()
        {
            // Emoji are surrogate pairs; each char of the pair gets escaped
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Test \u2764"); // ❤
            Assert.IsTrue(rtf.Contains("Test "));
            Assert.IsTrue(rtf.Contains("\\u"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_GermanUmlauts_ContainsUnicodeEscapes()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("Über Straße");
            // Ü = U+00DC = 220
            Assert.IsTrue(rtf.Contains("\\u220?"));
            // ß = U+00DF = 223
            Assert.IsTrue(rtf.Contains("\\u223?"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_UnicodeInHtmlEntities_DecodedAndEscaped()
        {
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf("&#8212;"); // em dash
            // — = U+2014 = 8212
            Assert.IsTrue(rtf.Contains("\\u8212?"));
        }

        [TestMethod]
        public void ConvertHtmlToBase64Rtf_ReturnsValidBase64()
        {
            var base64 = HtmlToRtfConverter.ConvertHtmlToBase64Rtf("<p>Test</p>");
            Assert.IsFalse(string.IsNullOrEmpty(base64));

            // Should be valid Base64
            var bytes = System.Convert.FromBase64String(base64);
            Assert.IsTrue(bytes.Length > 0);

            // Decoded should start with {\rtf1
            var decoded = System.Text.Encoding.UTF8.GetString(bytes);
            Assert.IsTrue(decoded.StartsWith("{\\rtf1"));
        }
    }
}
