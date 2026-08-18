# Accepted conversion and source-tooling cases

These are required future conformance cases. “Accept” means the described paper
outcome is the intended Language 1.0 behavior; it does not claim an
implementation has run.

| # | Case | Expected result |
| ---: | --- | --- |
| 1 | Convert the fixture from `zh-Hans@1` to `en@1` | Bytes equal `Expected-En.wv`; canonical projection is unchanged. |
| 2 | Convert the fixture from `en@1` to `zh-Hans@1` | Bytes equal `Expected-Zh-Hans.wv`; canonical projection is unchanged. |
| 3 | Convert `zh-Hans@1` to `test-Unicode@1` | Bytes equal `Expected-Test-Unicode.wv`; no translation-quality claim is made. |
| 4 | Convert A to B and directly back to the same exact A inputs | The original raw bytes and SHA-256 return exactly. |
| 5 | Target spellings use different UTF-8 lengths | Later spans remain correct because replacements use admitted spans, not text search. |
| 6 | Chinese project module/function/parameter/alias names are present | Their exact UTF-8 bytes survive every profile conversion. |
| 7 | Comments, strings, documentation, whitespace, blank lines, and LF/CRLF occur | Every byte outside mapped spans survives conversion. |
| 8 | Fixed-width numeric words, platform IDs, capability/effect identities, and foreign names occur | Registered machine identities remain unchanged. |
| 9 | A named argument resolves through a complete target catalog | Only its imported-public label changes; the canonical parameter identity remains equal. |
| 10 | Converter writes a new absent destination | It validates fully, emits a bounded report/map, then publishes the complete file. |
| 11 | Explicit in-place conversion sees the original hash unchanged | Atomic same-filesystem replacement publishes the validated file. |
| 12 | Ordinary Copy is used in a localized file | Clipboard plain text equals the exact stored selection. |
| 13 | Copy canonical source is requested for an admitted file | Deterministic `en@1` source is produced with explicit provenance. |
| 14 | Copy displayed view is requested | Human-view text is clearly marked as presentation, not silently treated as compilable source. |
| 15 | Provenanced same-profile source is pasted | Validated stored source inserts as one undo transaction. |
| 16 | Provenanced different-profile source is pasted and conversion is accepted | Target primary spellings insert as one undo transaction. |
| 17 | Plain unprovenanced Unicode text is pasted | Exact text inserts; normal syntax/security checks apply without language guessing. |
| 18 | Formatter runs on a `zh-Hans@1` file | Descriptor/profile remain Chinese; primary Chinese keyword/public labels are retained. |
| 19 | Formatter runs twice without other changes | Second output is byte-identical. |
| 20 | Project-owned declaration is semantically renamed | Only its declaration/references change, subject to identifier/security admission. |
| 21 | Imported localized label is selected for rename in a consumer | Tool reveals the canonical library owner/catalog; consumer cannot rename the dependency. |
| 22 | A catalog publisher changes one localized public label | New catalog bytes/hashes and review are required; consumer source changes only by explicit conversion. |
| 23 | Canonical reveal is opened | Stored spelling, canonical identity, pack/catalog hash, and raw span are visible without a disk edit. |
| 24 | Search selects stored, canonical, display, or semantic mode | Results follow only the explicitly selected identity space. |
| 25 | Git semantic review compares a profile-only conversion | Raw patch and both profile/catalog identities remain available beside the equal semantic projection. |
| 26 | Diagnostic locale changes | Human prose may change; diagnostic identity, fields, spans, result, and artifacts do not. |
| 27 | A converted breakpoint or diagnostic span is remapped | The validated conversion map and raw hashes select the output span. |
| 28 | Two compiler-service requests use different raw/localized source | Request-owned maps/diagnostics remain separate even if canonical semantic evidence is safely shared. |
| 29 | IME composition is incomplete | No hidden source conversion occurs before commit. |
| 30 | Screen reader inspects a displayed localized token | It can obtain label, semantic role, and canonical identity as one token. |
