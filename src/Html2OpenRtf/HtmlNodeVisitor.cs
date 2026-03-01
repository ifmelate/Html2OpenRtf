using System;
using System.Collections.Generic;
using System.Globalization;
using HtmlAgilityPack;

namespace Html2OpenRtf
{
    /// <summary>
    /// Walks an HtmlAgilityPack DOM tree and emits RTF via <see cref="RtfDocumentBuilder"/>.
    /// </summary>
    internal sealed class HtmlNodeVisitor
    {
        private readonly RtfDocumentBuilder _rtf;
        private bool _needsPar;
        private int _listDepth;
        private readonly Stack<ListContext> _listStack = new Stack<ListContext>();
        private readonly Stack<FormattingState> _stateStack = new Stack<FormattingState>();
        private FormattingState _currentState;

        // Heading font sizes in half-points (RTF convention)
        private static readonly Dictionary<string, int> HeadingSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["h1"] = 48, // 24pt
            ["h2"] = 36, // 18pt
            ["h3"] = 28, // 14pt
            ["h4"] = 24, // 12pt
            ["h5"] = 20, // 10pt
            ["h6"] = 18, // 9pt
        };

        public HtmlNodeVisitor(RtfDocumentBuilder rtf)
        {
            _rtf = rtf;
            _currentState = new FormattingState(fontSize: 24, leftIndent: 0, firstLineIndent: 0);
        }

        private void PushState()
        {
            _stateStack.Push(_currentState);
        }

        private void PopState()
        {
            if (_stateStack.Count == 0) return;
            _currentState = _stateStack.Pop();
            _rtf.FontSize(_currentState.FontSize);
            _rtf.LeftIndent(_currentState.LeftIndent);
            _rtf.FirstLineIndent(_currentState.FirstLineIndent);
        }

        public void Visit(HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Document:
                    VisitChildren(node);
                    break;

                case HtmlNodeType.Text:
                    VisitText((HtmlTextNode)node);
                    break;

                case HtmlNodeType.Element:
                    VisitElement(node);
                    break;
            }
        }

        private void VisitChildren(HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
                Visit(child);
        }

        private void VisitText(HtmlTextNode textNode)
        {
            var text = HtmlEntity.DeEntitize(textNode.Text);

            // Collapse whitespace (HTML behavior) unless inside <pre>
            if (!IsInsidePre(textNode))
            {
                text = CollapseWhitespace(text);
                if (string.IsNullOrEmpty(text))
                    return;
            }

            _rtf.AppendText(text);
        }

        private void VisitElement(HtmlNode node)
        {
            var tag = node.Name.ToLowerInvariant();

            switch (tag)
            {
                case "br":
                    _rtf.LineBreak();
                    return;

                case "hr":
                    EmitParagraphBreak();
                    _rtf.HorizontalRule();
                    return;

                case "img":
                    var alt = node.GetAttributeValue("alt", "image");
                    _rtf.AppendText($"[image: {alt}]");
                    return;

                case "p":
                case "div":
                    EmitParagraphBreak();
                    VisitStyledBlock(node);
                    return;

                case "h1": case "h2": case "h3":
                case "h4": case "h5": case "h6":
                    VisitHeading(node, tag);
                    return;

                case "blockquote":
                    VisitBlockquote(node);
                    return;

                case "pre":
                    VisitPre(node);
                    return;

                case "b":
                case "strong":
                    VisitInlineFormat(node, () => _rtf.Bold(true), () => _rtf.Bold(false));
                    return;

                case "i":
                case "em":
                    VisitInlineFormat(node, () => _rtf.Italic(true), () => _rtf.Italic(false));
                    return;

                case "u":
                    VisitInlineFormat(node, () => _rtf.Underline(true), () => _rtf.Underline(false));
                    return;

                case "s":
                case "strike":
                case "del":
                    VisitInlineFormat(node, () => _rtf.Strikethrough(true), () => _rtf.Strikethrough(false));
                    return;

                case "sub":
                    VisitInlineFormat(node, () => _rtf.Subscript(true), () => _rtf.Subscript(false));
                    return;

                case "sup":
                    VisitInlineFormat(node, () => _rtf.Superscript(true), () => _rtf.Superscript(false));
                    return;

                case "a":
                    VisitAnchor(node);
                    return;

                case "span":
                    VisitSpan(node);
                    return;

                case "code":
                case "kbd":
                case "samp":
                    VisitInlineFormat(node, () => _rtf.Font(1), () => _rtf.Font(0));
                    return;

                case "mark":
                    VisitMark(node);
                    return;

                case "ul":
                    VisitList(node, ordered: false);
                    return;

                case "ol":
                    VisitList(node, ordered: true);
                    return;

                case "li":
                    VisitListItem(node);
                    return;

                case "table":
                    VisitTable(node);
                    return;

                // Structural tags to pass through
                case "html": case "body": case "head":
                case "header": case "footer": case "main":
                case "section": case "article": case "nav":
                case "aside": case "figure": case "figcaption":
                case "form": case "fieldset": case "label":
                case "tbody": case "thead": case "tfoot":
                    VisitChildren(node);
                    return;

                // Ignored tags
                case "style": case "script": case "link":
                case "meta": case "title": case "noscript":
                    return;

                default:
                    // Unknown tags — just process children
                    VisitChildren(node);
                    return;
            }
        }

        private void VisitHeading(HtmlNode node, string tag)
        {
            EmitParagraphBreak();
            PushState();
            _rtf.Bold(true);
            if (HeadingSizes.TryGetValue(tag, out int size))
            {
                _currentState = new FormattingState(size, _currentState.LeftIndent, _currentState.FirstLineIndent);
                _rtf.FontSize(size);
            }

            VisitChildren(node);

            _rtf.Bold(false);
            PopState();
        }

        private void VisitBlockquote(HtmlNode node)
        {
            EmitParagraphBreak();
            PushState();
            int newIndent = _currentState.LeftIndent + 720; // 0.5 inch additive
            _currentState = new FormattingState(_currentState.FontSize, newIndent, _currentState.FirstLineIndent);
            _rtf.LeftIndent(newIndent);
            VisitChildren(node);
            PopState();
        }

        private void VisitPre(HtmlNode node)
        {
            EmitParagraphBreak();
            _rtf.Font(1); // Courier New
            VisitChildren(node);
            _rtf.Font(0); // back to Arial
        }

        private void VisitInlineFormat(HtmlNode node, Action open, Action close)
        {
            open();
            VisitChildren(node);
            close();
        }

        private void VisitMark(HtmlNode node)
        {
            // <mark> = yellow background highlight
            int idx = _rtf.RegisterColor(255, 255, 0);
            _rtf.BackgroundColor(idx);
            VisitChildren(node);
            _rtf.BackgroundColor(0);
        }

        private void VisitAnchor(HtmlNode node)
        {
            var href = node.GetAttributeValue("href", null);
            if (!string.IsNullOrEmpty(href))
            {
                _rtf.BeginHyperlink(href);
                _rtf.ForeColor(_rtf.RegisterColor(0, 0, 255)); // blue
                _rtf.Underline(true);
                VisitChildren(node);
                _rtf.Underline(false);
                _rtf.ForeColor(0);
                _rtf.EndHyperlink();
            }
            else
            {
                VisitChildren(node);
            }
        }

        private void VisitSpan(HtmlNode node)
        {
            var style = node.GetAttributeValue("style", null);

            PushState();
            var resets = ApplyInlineStyles(style);
            VisitChildren(node);
            ResetInlineStyles(resets);
            PopState();
        }

        private void VisitList(HtmlNode node, bool ordered)
        {
            _listDepth++;
            _listStack.Push(new ListContext(ordered, 0));
            VisitChildren(node);
            _listStack.Pop();
            _listDepth--;
        }

        private void VisitListItem(HtmlNode node)
        {
            EmitParagraphBreak();
            PushState();

            int indent = _listDepth * 360; // 0.25 inch per level
            _currentState = new FormattingState(_currentState.FontSize, indent, -180);
            _rtf.LeftIndent(indent);
            _rtf.FirstLineIndent(-180); // hanging indent for bullet/number

            if (_listStack.Count > 0)
            {
                var ctx = _listStack.Pop();
                ctx = new ListContext(ctx.Ordered, ctx.Counter + 1);
                _listStack.Push(ctx);

                if (ctx.Ordered)
                {
                    _rtf.AppendText($"{ctx.Counter}. ");
                }
                else
                {
                    string bullet = _listDepth <= 1 ? "\u2022" : "-";
                    _rtf.AppendText($"{bullet} ");
                }
            }

            VisitChildren(node);

            PopState();
        }

        private void VisitTable(HtmlNode node)
        {
            EmitParagraphBreak();

            // Collect rows
            var rows = new List<List<HtmlNode>>();
            CollectTableRows(node, rows);

            if (rows.Count == 0)
            {
                VisitChildren(node);
                return;
            }

            // Determine max columns
            int maxCols = 0;
            foreach (var row in rows)
                if (row.Count > maxCols) maxCols = row.Count;

            if (maxCols == 0) return;

            // Cell width in twips (assume 6 inch page = 8640 twips)
            int cellWidth = 8640 / maxCols;

            foreach (var row in rows)
            {
                // Row definition
                _rtf.AppendRaw("\\trowd\\trgaph108 ");
                for (int c = 0; c < maxCols; c++)
                {
                    _rtf.AppendRaw($"\\cellx{cellWidth * (c + 1)} ");
                }

                for (int c = 0; c < maxCols; c++)
                {
                    _rtf.AppendRaw("\\pard\\intbl\\plain\\f0\\fs24 ");
                    if (c < row.Count)
                    {
                        var cell = row[c];
                        bool isHeader = cell.Name.Equals("th", StringComparison.OrdinalIgnoreCase);
                        if (isHeader) _rtf.Bold(true);
                        VisitChildren(cell);
                        if (isHeader) _rtf.Bold(false);
                    }
                    _rtf.AppendRaw("\\cell ");
                }
                _rtf.AppendRaw("\\row ");
            }

            _needsPar = false;
        }

        private void CollectTableRows(HtmlNode node, List<List<HtmlNode>> rows)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType != HtmlNodeType.Element) continue;

                var tag = child.Name.ToLowerInvariant();
                if (tag == "tr")
                {
                    var cells = new List<HtmlNode>();
                    foreach (var cell in child.ChildNodes)
                    {
                        if (cell.NodeType != HtmlNodeType.Element) continue;
                        var cellTag = cell.Name.ToLowerInvariant();
                        if (cellTag == "td" || cellTag == "th")
                            cells.Add(cell);
                    }
                    if (cells.Count > 0) rows.Add(cells);
                }
                else if (tag == "thead" || tag == "tbody" || tag == "tfoot")
                {
                    CollectTableRows(child, rows);
                }
            }
        }

        private void VisitStyledBlock(HtmlNode node)
        {
            var style = node.GetAttributeValue("style", null);
            if (string.IsNullOrEmpty(style))
            {
                VisitChildren(node);
                return;
            }

            PushState();

            // text-align is block-only, handle before shared logic
            var styles = ParseInlineStyle(style);
            if (styles.TryGetValue("text-align", out var align))
            {
                switch (align.ToLowerInvariant())
                {
                    case "center": _rtf.AppendRaw("\\qc "); break;
                    case "right": _rtf.AppendRaw("\\qr "); break;
                    case "justify": _rtf.AppendRaw("\\qj "); break;
                }
            }

            var resets = ApplyInlineStyles(style);
            VisitChildren(node);
            ResetInlineStyles(resets);
            PopState();
        }

        /// <summary>
        /// Applies shared inline CSS properties (color, background-color, font-size,
        /// font-weight, font-style, text-decoration) and returns flags for resetting.
        /// </summary>
        private StyleResets ApplyInlineStyles(string style)
        {
            var resets = new StyleResets();
            if (string.IsNullOrEmpty(style)) return resets;

            var styles = ParseInlineStyle(style);

            if (styles.TryGetValue("color", out var colorVal) && CssColorParser.TryParse(colorVal, out var cr, out var cg, out var cb))
            {
                _rtf.ForeColor(_rtf.RegisterColor(cr, cg, cb));
                resets.Color = true;
            }

            if (styles.TryGetValue("background-color", out var bgVal) && CssColorParser.TryParse(bgVal, out var br, out var bg, out var bb))
            {
                _rtf.BackgroundColor(_rtf.RegisterColor(br, bg, bb));
                resets.BgColor = true;
            }

            if (styles.TryGetValue("font-size", out var fsVal))
            {
                int halfPts = ParseFontSize(fsVal);
                if (halfPts > 0)
                {
                    _currentState = new FormattingState(halfPts, _currentState.LeftIndent, _currentState.FirstLineIndent);
                    _rtf.FontSize(halfPts);
                }
            }

            if (styles.TryGetValue("font-weight", out var fwVal))
            {
                var fw = fwVal.Trim().ToLowerInvariant();
                if (fw == "bold" || fw == "700" || fw == "800" || fw == "900")
                {
                    _rtf.Bold(true);
                    resets.Bold = true;
                }
            }

            if (styles.TryGetValue("font-style", out var fiVal))
            {
                if (fiVal.Trim().Equals("italic", StringComparison.OrdinalIgnoreCase))
                {
                    _rtf.Italic(true);
                    resets.Italic = true;
                }
            }

            if (styles.TryGetValue("text-decoration", out var tdVal))
            {
                var td = tdVal.Trim().ToLowerInvariant();
                if (td.Contains("underline")) { _rtf.Underline(true); resets.Underline = true; }
                if (td.Contains("line-through")) { _rtf.Strikethrough(true); resets.Strike = true; }
            }

            return resets;
        }

        private void ResetInlineStyles(StyleResets resets)
        {
            if (resets.Color) _rtf.ForeColor(0);
            if (resets.BgColor) _rtf.BackgroundColor(0);
            if (resets.Bold) _rtf.Bold(false);
            if (resets.Italic) _rtf.Italic(false);
            if (resets.Underline) _rtf.Underline(false);
            if (resets.Strike) _rtf.Strikethrough(false);
        }

        private void EmitParagraphBreak()
        {
            if (_needsPar)
                _rtf.Paragraph();
            _needsPar = true;
        }

        private static Dictionary<string, string> ParseInlineStyle(string style)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parts = style.Split(';');
            foreach (var part in parts)
            {
                int colon = part.IndexOf(':');
                if (colon <= 0) continue;
                var key = part.Substring(0, colon).Trim();
                var val = part.Substring(colon + 1).Trim();
                if (key.Length > 0 && val.Length > 0)
                    result[key] = val;
            }
            return result;
        }

        /// <summary>
        /// Parse CSS font-size to RTF half-points.
        /// Supports: "12pt", "16px", "1.5em", "120%", "small", etc.
        /// </summary>
        private static int ParseFontSize(string value)
        {
            value = value.Trim().ToLowerInvariant();

            // Named sizes
            switch (value)
            {
                case "xx-small": return 14;  // 7pt
                case "x-small": return 16;   // 8pt
                case "small": return 20;     // 10pt
                case "medium": return 24;    // 12pt
                case "large": return 28;     // 14pt
                case "x-large": return 36;   // 18pt
                case "xx-large": return 48;  // 24pt
            }

            if (value.EndsWith("pt"))
            {
                if (double.TryParse(value.Substring(0, value.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double pt))
                    return (int)(pt * 2);
            }

            if (value.EndsWith("px"))
            {
                // 1px ≈ 0.75pt
                if (double.TryParse(value.Substring(0, value.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double px))
                    return (int)(px * 1.5);
            }

            if (value.EndsWith("em"))
            {
                // 1em = 12pt base
                if (double.TryParse(value.Substring(0, value.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double em))
                    return (int)(em * 24);
            }

            if (value.EndsWith("%"))
            {
                if (double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double pct))
                    return (int)(pct / 100.0 * 24);
            }

            return 0;
        }

        private static bool IsInsidePre(HtmlNode node)
        {
            var parent = node.ParentNode;
            while (parent != null)
            {
                if (parent.Name.Equals("pre", StringComparison.OrdinalIgnoreCase))
                    return true;
                parent = parent.ParentNode;
            }
            return false;
        }

        private static string CollapseWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new System.Text.StringBuilder(text.Length);
            bool lastWasSpace = false;
            foreach (char c in text)
            {
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    lastWasSpace = false;
                }
            }
            return sb.ToString();
        }

        private struct StyleResets
        {
            public bool Color, BgColor, Bold, Italic, Underline, Strike;
        }

        private struct ListContext
        {
            public readonly bool Ordered;
            public readonly int Counter;
            public ListContext(bool ordered, int counter) { Ordered = ordered; Counter = counter; }
        }

        private struct FormattingState
        {
            public readonly int FontSize;
            public readonly int LeftIndent;
            public readonly int FirstLineIndent;
            public FormattingState(int fontSize, int leftIndent, int firstLineIndent)
            {
                FontSize = fontSize;
                LeftIndent = leftIndent;
                FirstLineIndent = firstLineIndent;
            }
        }
    }
}
