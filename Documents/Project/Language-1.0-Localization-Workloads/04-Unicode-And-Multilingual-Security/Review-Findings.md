# Localization workload 4 findings

## Status

The project owner accepts these Unicode and multilingual-security decisions for
the replacement Language 1.0 candidate. Implementation, editor/console, and
cross-host evidence remain open.

## Finding 1: one pinned Unicode profile covers all source languages

Language packs do not bring private normalization, character, script, or
confusable tables. The exact Unicode 17 profile admits the required writing
systems and keeps Windows/Linux semantics equal.

## Finding 2: Unicode project identifiers work under `en@1`

Keyword localization and project-owned naming are independent. The paper source
uses eight scripts while retaining canonical English keywords, proving that a
new profile is not required merely to name a project declaration in its author's
language.

## Finding 3: security applies per visible semantic segment

U+02C9 makes cross-script concepts explicit. This permits natural technical
names such as `HTTPˉОтвет` while preventing hidden Latin/Cyrillic mixing inside
one word. UTS #39's intentionally allowed CJK/Korean combinations remain legal.

## Finding 4: confusable collisions are semantic-scope errors

Each spelling may be individually valid. It becomes dangerous when two distinct
names compete for the same lookup. Hash maps over both pinned directional
skeletons provide strict, bounded, low-false-positive rejection.

## Finding 5: join controls stay outside edition 1 identifiers

Their linguistic value is real, particularly for Persian and some Indic
orthographies. Their invisibility, shaping dependence, Restricted status, and
contextual validation cost make exclusion the correct first-edition choice.
Windvale records the limitation instead of pretending every orthography is fully
covered.

## Finding 6: code grammar remains structurally left-to-right

Arabic/Hebrew source changes token content, not grammar. One logical order keeps
parsing, diffs, conversion, diagnostics, source maps, and collaboration exact.
Source-aware rendering isolates RTL atoms for humans.

## Finding 7: line-spoof characters must fail globally

Windvale recognizes only LF/CRLF. Rejecting the five other literal hard-line
characters even in comments/raw text prevents an editor from showing a line
boundary that the compiler does not recognize. Escapes preserve runtime access
to those scalar values.

## Finding 8: limited implicit direction marks improve plain-text usability

One ALM/LRM/RLM at a token boundary can stabilize display without changing
semantics. Exact admission, raw provenance, show-invisibles behavior, and a
single-mark rule keep that allowance inspectable.

## Finding 9: stateful controls belong inside bounded content atoms only

Paired controls can be legitimate text data. Keeping their literal effects
inside one line/atom with depth 16 prevents them from visually capturing syntax.
Escapes express deliberately unbalanced runtime data safely.

## Finding 10: identifiers need explicit product bounds

The 256-byte, 128-scalar, 32-segment limits preserve long descriptive names while
bounding normalization, skeleton calculation, mangling, diagnostics, and
compiler-service retention. Source labels keep their existing stricter bounds.

## Finding 11: UTS #55 behavior is an editor/reviewer requirement

Correct parsing does not guarantee readable RTL source. Editors, semantic diffs,
and source-aware diagnostic viewers must render lexical atoms rather than one
undifferentiated bidi paragraph and must expose hidden controls accessibly.

## Finding 12: no native-language certification is claimed

These examples test script mechanics and security, not translated keywords or
terminology quality. They need exact data review and future implementation tests,
but they do not require the project owner to certify fluency in nine scripts.

## Disposition

Carry the accepted rules into the grammar/localized-source specification and
advance to Workload 5: installation, deduplication, offline resolution, update/
rollback, cache generations, Windows/Linux equality, and measured time/memory
protocols. Workload 5 subsequently accepted those paper contracts; executable
and measured qualification remains open.
