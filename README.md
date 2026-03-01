# Html2OpenRtf

Free, open-source .NET library for converting HTML to RTF (Rich Text Format).

[![Build](https://github.com/ifmelate/Html2OpenRtf/actions/workflows/ci.yml/badge.svg)](https://github.com/ifmelate/Html2OpenRtf/actions)
[![NuGet](https://img.shields.io/nuget/v/Html2OpenRtf.svg)](https://www.nuget.org/packages/Html2OpenRtf/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)



## Installation

```bash
dotnet add package Html2OpenRtf
```

## Usage

```csharp
using Html2OpenRtf;

// HTML string to RTF string
string rtf = HtmlToRtfConverter.ConvertHtmlToRtf("<p>Hello <b>World</b></p>");

// HTML to RTF bytes (ASCII)
byte[] rtfBytes = HtmlToRtfConverter.ConvertHtmlToRtfBytes(html);

// HTML to Base64-encoded RTF (useful for APIs)
string base64Rtf = HtmlToRtfConverter.ConvertHtmlToBase64Rtf(html);
```

## Supported HTML

| Category | Tags |
|----------|------|
| **Block** | `p`, `div`, `br`, `h1`-`h6`, `blockquote`, `hr`, `pre` |
| **Inline** | `b`/`strong`, `i`/`em`, `u`, `s`/`strike`/`del`, `sub`, `sup` |
| **Lists** | `ul`, `ol`, `li` (including nested) |
| **Tables** | `table`, `tr`, `td`, `th` (equal-width cells) |
| **Links** | `a` (clickable RTF hyperlinks) |
| **Images** | `img` (placeholder `[image: alt]`) |
| **Code** | `code`, `kbd`, `samp` (monospace font) |
| **Highlight** | `mark` (yellow background) |
| **Styles** | `color`, `font-size`, `background-color`, `text-align`, `font-weight`, `font-style`, `text-decoration` via `style` attribute |
| **Unicode** | Full support (Cyrillic, CJK, etc. via `\uN?` escapes) |
| **Entities** | Full HTML entity decoding |

## Target

- **.NET Standard 2.0** (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5-9)
- Single dependency: [HtmlAgilityPack](https://html-agility-pack.net/) (MIT)
- Cross-platform: Windows, Linux, macOS

## Contributing

PRs welcome. Run tests with:

```bash
dotnet test
```

## License

MIT
