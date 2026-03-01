using System;
using System.Collections.Generic;
using System.Globalization;

namespace Html2OpenRtf
{
    /// <summary>
    /// Parses CSS color values into (R, G, B) tuples.
    /// Supports: #rgb, #rgba, #rrggbb, #rrggbbaa, rgb(r,g,b), rgb(r g b), percentages, and named colors.
    /// </summary>
    internal static class CssColorParser
    {
        public static bool TryParse(string value, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            if (value.StartsWith("#"))
                return TryParseHex(value, out r, out g, out b);

            if (value.StartsWith("rgb"))
                return TryParseRgb(value, out r, out g, out b);

            return TryParseNamed(value, out r, out g, out b);
        }

        private static bool TryParseHex(string hex, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            hex = hex.TrimStart('#');

            if (hex.Length == 3 || hex.Length == 4)
            {
                // #rgb or #rgba → expand first 3 digits, ignore alpha if present
                hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
            }
            else if (hex.Length == 8)
            {
                // #rrggbbaa → #rrggbb (strip alpha)
                hex = hex.Substring(0, 6);
            }

            if (hex.Length != 6) return false;

            return byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
                && byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
                && byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
        }

        private static bool TryParseRgb(string value, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            // rgb(255, 128, 0), rgba(255, 128, 0, 1.0), rgb(255 128 0), rgb(100%, 50%, 0%)
            int open = value.IndexOf('(');
            int close = value.IndexOf(')');
            if (open < 0 || close < 0 || close <= open) return false;

            var inner = value.Substring(open + 1, close - open - 1).Trim();

            // Handle slash-separated alpha: "255 128 0 / 0.5"
            int slashIdx = inner.IndexOf('/');
            if (slashIdx >= 0)
                inner = inner.Substring(0, slashIdx).Trim();

            // Try comma-separated first, then space-separated
            string[] parts;
            if (inner.Contains(","))
                parts = inner.Split(',');
            else
                parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3) return false;

            if (!TryParseColorComponent(parts[0].Trim(), out r)) return false;
            if (!TryParseColorComponent(parts[1].Trim(), out g)) return false;
            if (!TryParseColorComponent(parts[2].Trim(), out b)) return false;

            return true;
        }

        private static bool TryParseColorComponent(string val, out byte result)
        {
            result = 0;
            if (val.EndsWith("%"))
            {
                if (double.TryParse(val.Substring(0, val.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double pct))
                {
                    result = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(pct / 100.0 * 255)));
                    return true;
                }
                return false;
            }
            return byte.TryParse(val, out result);
        }

        private static bool TryParseNamed(string name, out byte r, out byte g, out byte b)
        {
            if (NamedColors.TryGetValue(name, out var color))
            {
                r = color.R;
                g = color.G;
                b = color.B;
                return true;
            }
            r = g = b = 0;
            return false;
        }

        private static readonly Dictionary<string, (byte R, byte G, byte B)> NamedColors =
            new Dictionary<string, (byte, byte, byte)>
            {
                ["black"] = (0, 0, 0),
                ["white"] = (255, 255, 255),
                ["red"] = (255, 0, 0),
                ["green"] = (0, 128, 0),
                ["blue"] = (0, 0, 255),
                ["yellow"] = (255, 255, 0),
                ["cyan"] = (0, 255, 255),
                ["magenta"] = (255, 0, 255),
                ["orange"] = (255, 165, 0),
                ["purple"] = (128, 0, 128),
                ["gray"] = (128, 128, 128),
                ["grey"] = (128, 128, 128),
                ["silver"] = (192, 192, 192),
                ["maroon"] = (128, 0, 0),
                ["navy"] = (0, 0, 128),
                ["teal"] = (0, 128, 128),
                ["olive"] = (128, 128, 0),
                ["lime"] = (0, 255, 0),
                ["aqua"] = (0, 255, 255),
                ["fuchsia"] = (255, 0, 255),
                ["darkred"] = (139, 0, 0),
                ["darkgreen"] = (0, 100, 0),
                ["darkblue"] = (0, 0, 139),
                ["lightgray"] = (211, 211, 211),
                ["lightgrey"] = (211, 211, 211),
                ["darkgray"] = (169, 169, 169),
                ["darkgrey"] = (169, 169, 169),
                ["brown"] = (165, 42, 42),
                ["pink"] = (255, 192, 203),
                ["coral"] = (255, 127, 80),
                ["tomato"] = (255, 99, 71),
                ["gold"] = (255, 215, 0),
                ["indigo"] = (75, 0, 130),
                ["violet"] = (238, 130, 238),
            };
    }
}
