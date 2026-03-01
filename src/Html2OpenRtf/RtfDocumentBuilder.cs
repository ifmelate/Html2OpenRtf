using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Html2OpenRtf
{
    /// <summary>
    /// Builds an RTF document string incrementally.
    /// Manages font table, color table, and emits RTF control words.
    /// </summary>
    internal sealed class RtfDocumentBuilder
    {
        private readonly StringBuilder _body = new StringBuilder();
        private readonly List<RtfColor> _colors = new List<RtfColor>();
        private readonly List<string> _fonts = new List<string>();
        private bool _firstParagraph = true;

        public RtfDocumentBuilder()
        {
            // Default fonts
            _fonts.Add("Arial");        // \f0 - default proportional
            _fonts.Add("Courier New");  // \f1 - monospace (for <pre>/<code>)

            // Index 0 = auto color (placeholder; rendered as empty entry ";" in color table)
            _colors.Add(new RtfColor(0, 0, 0));
        }

        /// <summary>
        /// Register a color and return its 1-based index for use in \cfN / \chcbpatN.
        /// Index 0 is reserved for auto/default color and is never returned.
        /// </summary>
        public int RegisterColor(byte r, byte g, byte b)
        {
            // Skip index 0 (auto color) — explicit colors always get their own entry
            for (int i = 1; i < _colors.Count; i++)
            {
                if (_colors[i].R == r && _colors[i].G == g && _colors[i].B == b)
                    return i;
            }
            _colors.Add(new RtfColor(r, g, b));
            return _colors.Count - 1;
        }

        public void AppendText(string text)
        {
            foreach (char c in text)
            {
                if (c == '\\')
                    _body.Append("\\\\");
                else if (c == '{')
                    _body.Append("\\{");
                else if (c == '}')
                    _body.Append("\\}");
                else if (c == '\n')
                    _body.Append("\\line ");
                else if (c == '\r')
                    continue; // skip CR
                else if (c == '\t')
                    _body.Append("\\tab ");
                else if (c > 127)
                    AppendUnicode(c);
                else
                    _body.Append(c);
            }
        }

        private void AppendUnicode(char c)
        {
            // RTF Unicode escape: \uN? where N is signed 16-bit and ? is fallback char
            short signed = unchecked((short)c);
            _body.Append("\\u");
            _body.Append(signed.ToString(CultureInfo.InvariantCulture));
            _body.Append('?');
        }

        public void AppendRaw(string rtf) => _body.Append(rtf);

        public void Paragraph()
        {
            if (_firstParagraph)
            {
                _firstParagraph = false;
                _body.Append("\\pard ");
            }
            else
            {
                _body.Append("\\par\\pard ");
            }
        }

        public void LineBreak() => _body.Append("\\line ");

        public void Bold(bool on) => _body.Append(on ? "\\b " : "\\b0 ");
        public void Italic(bool on) => _body.Append(on ? "\\i " : "\\i0 ");
        public void Underline(bool on) => _body.Append(on ? "\\ul " : "\\ulnone ");
        public void Strikethrough(bool on) => _body.Append(on ? "\\strike " : "\\strike0 ");
        public void Subscript(bool on) => _body.Append(on ? "\\sub " : "\\nosupersub ");
        public void Superscript(bool on) => _body.Append(on ? "\\super " : "\\nosupersub ");

        /// <summary>Font size in half-points (RTF convention). E.g., 24 = 12pt.</summary>
        public void FontSize(int halfPoints) => _body.Append($"\\fs{halfPoints} ");

        /// <summary>Set font by index in font table.</summary>
        public void Font(int index) => _body.Append($"\\f{index} ");

        /// <summary>Set foreground color by color table index.</summary>
        public void ForeColor(int colorIndex) => _body.Append($"\\cf{colorIndex} ");

        /// <summary>Set background color by color table index (uses \chcbpat for arbitrary colors).</summary>
        public void BackgroundColor(int colorIndex) => _body.Append($"\\chcbpat{colorIndex} ");

        /// <summary>Reset all character formatting to defaults.</summary>
        public void ResetCharFormat() => _body.Append("\\plain\\f0\\fs24 ");

        public void OpenGroup() => _body.Append('{');
        public void CloseGroup() => _body.Append('}');

        /// <summary>Begin an RTF hyperlink field. Call EndHyperlink() after writing link content.</summary>
        public void BeginHyperlink(string url)
        {
            // Escape backslashes and quotes in URL for RTF field instruction
            var escaped = url.Replace("\\", "\\\\").Replace("\"", "\\\"");
            _body.Append("{\\field{\\*\\fldinst{HYPERLINK \"");
            _body.Append(escaped);
            _body.Append("\"}}{\\fldrslt{");
        }

        /// <summary>End an RTF hyperlink field.</summary>
        public void EndHyperlink()
        {
            _body.Append("}}}");
        }

        /// <summary>Horizontal rule: thin line across the page.</summary>
        public void HorizontalRule()
        {
            _body.Append("\\pard\\brdrb\\brdrs\\brdrw10\\brsp20 \\par\\pard ");
        }

        /// <summary>Left indent in twips (1 inch = 1440 twips).</summary>
        public void LeftIndent(int twips) => _body.Append($"\\li{twips} ");

        /// <summary>First-line indent in twips.</summary>
        public void FirstLineIndent(int twips) => _body.Append($"\\fi{twips} ");

        /// <summary>Builds the complete RTF document string.</summary>
        public string Build()
        {
            var sb = new StringBuilder();

            // RTF header
            sb.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\uc1");

            // Font table
            sb.Append("{\\fonttbl");
            for (int i = 0; i < _fonts.Count; i++)
            {
                string charset = "\\fcharset0";
                string family = i == 1 ? "\\fmodern" : "\\fswiss";
                sb.Append($"{{\\f{i}{family}{charset} {_fonts[i]};}}");
            }
            sb.Append('}');

            // Color table
            sb.Append("{\\colortbl ;");
            for (int i = 1; i < _colors.Count; i++)
            {
                var c = _colors[i];
                sb.Append($"\\red{c.R}\\green{c.G}\\blue{c.B};");
            }
            sb.Append('}');

            // Default formatting
            sb.Append("\\f0\\fs24 ");

            // Body
            sb.Append(_body);

            // Close RTF
            sb.Append('}');

            return sb.ToString();
        }

        private readonly struct RtfColor
        {
            public readonly byte R, G, B;
            public RtfColor(byte r, byte g, byte b) { R = r; G = g; B = b; }
        }
    }
}
