# Fuse.UxParser

A new work-in-progress UX parser, heavily inspired by Roslyn and [KirillOsenkov/XmlParser](https://github.com/KirillOsenkov/XmlParser).

More or less implemented features:

- "Full fidelity" immutable syntax tree model, roundtrips back to original source
- A lazily created (mutable) facade built over syntax tree
  - Nodes of this tree has parent references
  - Not intended to be thread-safe, use from one thread at the time
- Change events, that can recreate the exact same syntax 
  - Each change can be reversed
- Visitor for syntax trees

Planned (or incomplete) features:

- Representation of trees with bad syntax
  - Not quite sure what's the best way to do this yet
- Automatic indentation and formatting
  - Make this configurable

### Documentation

No docs yet, check out `Fuse.UxParser.Tests` for some examples.

### Testing

Many of the tests will scan for .ux files to do roundtrip testing on in `../example-docs`, `../hikr` and `../fuse-samples`.

### Open questions

- How should the special literal JavaScript elements be represented?
  - Currently they're a subtype of ElementSyntaxBase, however they _can't_ be treated as normal elements.
  - Content should maybe be a child node of a special type only allowed to be part of JavaScript element?
  - In the overlay tree, should they be represented by UxElement or its own type?
- Should diff stream expect the exact same base, or should it be more lenient to trivia differences etc?
- I originally wanted to use XPath as a starting point for defining paths, however that would be very
  complex and don't support easy selection of nodes above and below root element.
- How should trivia around text nodes be represented
  - Right now all whitespace surrounding text is considered significant (ie. not trivia)
  - ..this actually means _any_ whitespace inside elements are text nodes
  - ..leaning towards changing this. Maybe whitespace is _not_ significant.
- Do text nodes interleaved with element nodes ever make sense in UX?
