# Fuse.UxParser

A new work-in-progress UX parser, heavily inspired by Roslyn and [KirillOsenkov/XmlParser](https://github.com/KirillOsenkov/XmlParser).

Features, some of them incomplete:
- "Full fidelity" syntax tree, roundtrip back to original source
- A lazily created (mutable) facade built over a syntax tree

No docs yet, check out `Fuse.UxParser.Tests` for some examples.

Will scan for .ux files to do roundtrip testing on in `../example-docs` and `../fuse-samples`

### Open design questions

- Should diff stream expect the exact same base, or should it be more lenient to trivia differences etc?
- I originally wanted to use XPath as a starting point for defining paths, however that would be very
  complex and don't support easy selection of nodes above and below root element.
