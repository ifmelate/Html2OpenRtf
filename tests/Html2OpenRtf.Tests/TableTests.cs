using Microsoft.VisualStudio.TestTools.UnitTesting;
using Html2OpenRtf;

namespace Html2OpenRtf.Tests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void ConvertHtmlToRtf_SimpleTable_ContainsTableControls()
        {
            var html = @"
                <table>
                    <tr><td>A1</td><td>B1</td></tr>
                    <tr><td>A2</td><td>B2</td></tr>
                </table>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\trowd"));
            Assert.IsTrue(rtf.Contains("\\cell "));
            Assert.IsTrue(rtf.Contains("\\row "));
            Assert.IsTrue(rtf.Contains("A1"));
            Assert.IsTrue(rtf.Contains("B2"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_TableWithHeaders_HeadersAreBold()
        {
            var html = @"
                <table>
                    <tr><th>Name</th><th>Value</th></tr>
                    <tr><td>A</td><td>1</td></tr>
                </table>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\b "));
            Assert.IsTrue(rtf.Contains("Name"));
            Assert.IsTrue(rtf.Contains("Value"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_TableWithTbody_WorksCorrectly()
        {
            var html = @"
                <table>
                    <thead><tr><th>H</th></tr></thead>
                    <tbody><tr><td>D</td></tr></tbody>
                </table>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            Assert.IsTrue(rtf.Contains("\\trowd"));
            Assert.IsTrue(rtf.Contains("H"));
            Assert.IsTrue(rtf.Contains("D"));
        }

        [TestMethod]
        public void ConvertHtmlToRtf_TableCellWidths_AreCalculated()
        {
            var html = @"<table><tr><td>A</td><td>B</td><td>C</td></tr></table>";
            var rtf = HtmlToRtfConverter.ConvertHtmlToRtf(html);
            // 3 columns: 8640/3 = 2880 per cell
            Assert.IsTrue(rtf.Contains("\\cellx2880 "));
            Assert.IsTrue(rtf.Contains("\\cellx5760 "));
            Assert.IsTrue(rtf.Contains("\\cellx8640 "));
        }
    }
}
