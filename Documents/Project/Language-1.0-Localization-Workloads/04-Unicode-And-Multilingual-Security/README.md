# Language 1.0 localization workload 4: Unicode and multilingual security

## Status

Complete owner-accepted first-author paper bundle for the Language 1.0 Unicode
identifier, line, bidirectional-source, confusable, and editor-display boundary.
The exact Unicode 17.0.0 inputs and selected cases have been mechanically
checked. This is not compiler/editor implementation or cross-host qualification.

## Result first

Windvale Language 1.0 can safely admit meaningful identifiers in the required
writing systems without creating one compiler per human language:

- Latin, Cyrillic, Greek, Han, Hiragana, Katakana, Hangul, Arabic, and Hebrew
  examples pass the same pinned NFC/XID/UTS #39 pipeline;
- U+02C9 remains a visible semantic-word boundary, so names such as
  `HTTPˉОтвет` and `GPUˉتسريع` are legal without hiding a mixed-script word;
- the exact UTS #39 Highly Restrictive Japanese, Chinese, and Korean script
  combinations remain legal inside one segment;
- two visually confusable names cannot coexist in one semantic lookup scope,
  even when each name is independently valid;
- one segment cannot mix decimal-number systems;
- join controls, variation selectors, bidi controls, and other default-
  ignorables remain outside identifiers in edition 1;
- Windvale source keeps one logical left-to-right grammar order while editors
  isolate and render RTL token content according to source structure;
- deceptive non-Windvale line separators are rejected everywhere in raw source;
  and
- project identifiers receive exact length/segment bounds so admission,
  skeleton calculation, diagnostics, and retained compiler state remain bounded.

## Bundle contents

| Item | Purpose |
| --- | --- |
| [Unicode identifier policy](Unicode-Identifier-Policy.md) | Exact admission order, scripts, digits, normalization, confusables, bounds, and join-control decision. |
| [Bidirectional source and display](Bidirectional-Source-And-Display.md) | Logical order, line-spoof prevention, source controls, editor atoms, invisibles, and plain-text behavior. |
| [Accepted cases](Accepted-Cases.md) | Required multilingual, control, display, and scope success cases. |
| [Rejected cases](Rejected-Cases.md) | Required normalization, script, number, invisible, bidi, line, limit, and confusable failures. |
| [Validation record](Validation-Record.md) | Exact Unicode inputs, mechanical checks, reviewed standards, and paper-evidence limits. |
| [Review findings](Review-Findings.md) | Owner-accepted decisions and remaining implementation evidence. |
| [`Source/`](Source/) | One `en@1` paper program proving Unicode project identifiers are independent of keyword localization. |

## Important limitation

Excluding U+200C ZERO WIDTH NON-JOINER prevents the preferred spelling of some
Persian and related-language words. That is a deliberate Language 1.0 safety
limit, not a claim that the character is linguistically unnecessary. The
visible Windvale separator can express multi-concept program names, but it is not
a replacement for every natural-language orthographic distinction.

Release 1 currently targets official `en@1` and `zh-Hans@1` profiles, so this
limitation does not block those packs. A later edition may admit joining controls
only through exact UTS #39 contextual rules, native-language workloads, new
Unicode-profile bytes/hashes, and a named language decision.

## Workload disposition

The Unicode/security design and cases are accepted for the replacement Language
1.0 candidate. Implementation remains open until the real scanner, resolver,
formatter, editor/reviewer display, diagnostics, cache, and Windows/Linux tests
execute this packet with the exact pinned data.
