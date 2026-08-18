# Rejected Unicode and multilingual-security cases

Visible spellings are paired with exact code-point descriptions where an
invisible or lookalike character would make Markdown itself misleading.

| # | Case | Required rejection |
| ---: | --- | --- |
| 1 | `Cafe` followed by U+0301 COMBINING ACUTE ACCENT | Reject non-NFC; report precomposed `Café` without rewriting source. |
| 2 | Cyrillic U+0420 followed by Latin `ay` (`Рay`) | Reject Latin+Cyrillic in one Highly Restrictive segment. |
| 3 | Greek U+03A1 followed by Latin `ay` (`Ρay`) | Reject Latin+Greek in one segment. |
| 4 | `حسابTotal` | Reject Arabic+Latin in one segment. |
| 5 | `סכוםTotal` | Reject Hebrew+Latin in one segment. |
| 6 | `变量حساب` | Reject Han+Arabic in one segment. |
| 7 | `РасчётΣ` | Reject Cyrillic+Greek in one segment. |
| 8 | `عنصر1١` | Reject ASCII plus Arabic-Indic decimal systems in one segment. |
| 9 | `١عنصر` | Reject a decimal digit as segment start. |
| 10 | `به` + U+200C + `روز` | Reject ZWNJ as Restricted/default-ignorable in an identifier. |
| 11 | Devanagari sequence containing U+200D | Reject ZWJ as Restricted/default-ignorable in an identifier. |
| 12 | `变量` followed by U+FE0F VARIATION SELECTOR-16 | Reject variation selector in an identifier. |
| 13 | `变量` followed by U+200B ZERO WIDTH SPACE | Reject invisible non-XID/default-ignorable content. |
| 14 | Arabic identifier followed by U+202E RIGHT-TO-LEFT OVERRIDE | Reject stateful bidi control in an identifier. |
| 15 | Hebrew identifier followed by U+2066 LEFT-TO-RIGHT ISOLATE | Reject isolate control in an identifier. |
| 16 | Leading U+02C9, trailing U+02C9, or adjacent U+02C9 separators | Reject an empty semantic segment. |
| 17 | U+00AF MACRON used instead of U+02C9 | Reject the lookalike as non-identifier content; do not normalize it. |
| 18 | `变量😀` | Reject emoji as non-XID/non-Allowed identifier content. |
| 19 | Identifier contains U+0378 unassigned scalar | Reject unassigned content. |
| 20 | Identifier contains U+E000 private-use scalar | Reject private-use content. |
| 21 | Identifier contains U+FDD0 noncharacter | Reject noncharacter content. |
| 22 | Fullwidth U+FF26 in `Ｆoo` | Reject `Identifier_Status=Restricted`; do not NFKC-fold it to `Foo`. |
| 23 | Same scope contains Latin `scope` and Cyrillic `ѕсоре` | Reject the later name; pinned skeletons are equal. |
| 24 | Same scope contains Latin `KAI` and Greek `ΚΑΙ` | Reject the later name; pinned skeletons are equal. |
| 25 | Same scope contains Hebrew U+05D5 `ו` and U+05DF `ן` | Reject the later RTL name; both pinned skeletons map to `l`. |
| 26 | Identifier exceeds 256 UTF-8 bytes, 128 scalars, or 32 segments | Reject at the first exact bound without unbounded normalization/skeleton work. |
| 27 | Literal U+000B occurs anywhere in source | Reject unrecognized hard line boundary, including inside a comment/raw literal. |
| 28 | Literal U+000C occurs anywhere in source | Reject unrecognized hard line boundary. |
| 29 | Literal U+0085 occurs anywhere in source | Reject NEL line-spoof input. |
| 30 | Literal U+2028 occurs anywhere in source | Reject LINE SEPARATOR line-spoof input. |
| 31 | Literal U+2029 occurs anywhere in source | Reject PARAGRAPH SEPARATOR line-spoof input. |
| 32 | U+061C/U+200E/U+200F appears inside an identifier or other complete token | Reject it at that raw span; it cannot split a token. |
| 33 | Two implicit directional marks occur at one token boundary | Reject noncanonical/deceptive redundant controls. |
| 34 | Stateful bidi embedding/override/isolate control occurs between executable tokens | Reject it; only the three implicit marks are allowed there. |
| 35 | Literal stateful bidi controls are unbalanced within one content atom | Reject and report the opener/closer span. |
| 36 | A literal stateful control effect crosses a delimiter or line | Reject cross-atom visual influence. |
| 37 | Literal stateful bidi nesting exceeds 16 | Reject before growing unbounded control state. |
| 38 | Byte-zero descriptor contains any non-ASCII or direction mark | Reject descriptor admission before the general lexer. |
| 39 | Compiler silently normalizes, transliterates, folds case, or substitutes a lookalike separator | Reject implementation as nonconforming. |
| 40 | Compiler uses host locale, host Unicode tables, font glyphs, or collation for admission | Reject nondeterministic semantic input. |
| 41 | Pack attempts to whitelist join controls or a confusable collision | Reject the pack; it cannot relax the edition profile. |
| 42 | Confusable detection scans every pair or retains unbounded candidates | Reject the implementation/performance design. |
| 43 | Diagnostic prints a hidden control without an escaped/code-point form | Reject unsafe diagnostic rendering. |
| 44 | Editor copies visual glyph order rather than logical stored bytes | Reject source corruption. |
| 45 | Display locale mirrors token/grammar order | Reject creation of a second RTL grammar. |
| 46 | A paper case is reported as compiler, editor, console, or cross-host qualification | Reject unsupported evidence status. |
