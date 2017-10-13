# Fuse.UxParser

A new work-in-progress UX parser, heavily inspired by Roslyn and [KirillOsenkov/XmlParser](https://github.com/KirillOsenkov/XmlParser).

Features, some of them incomplete:
- "Full fidelity" syntax tree, roundtrip back to original source
- A lazily created (mutable) facade built over a syntax tree

No docs yet, check out `Fuse.UxParser.Tests` for some examples.

Will scan for .ux files to do roundtrip testing on in `../example-docs` and `../fuse-samples`
