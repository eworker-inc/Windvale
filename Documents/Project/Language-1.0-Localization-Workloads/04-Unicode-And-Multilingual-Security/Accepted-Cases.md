# Accepted Unicode and multilingual-security cases

These are required future conformance cases. The identifier rows were checked
against the exact Unicode 17 inputs named in the validation record.

| # | Case | Expected result |
| ---: | --- | --- |
| 1 | `Caféˉtotal` | NFC Latin segments are admitted. |
| 2 | `Расчётˉитога` | Cyrillic segments are admitted. |
| 3 | `Υπολογισμόςˉσυνόλου` | Greek segments are admitted. |
| 4 | `计算ˉ总计` | Han segments are admitted. |
| 5 | `配送ˉポリシー` | Han plus Katakana across visible segments is admitted. |
| 6 | `배송ˉ정책` | Precomposed Hangul segments are admitted. |
| 7 | `حسابˉالمجموع` | Arabic RTL segments are admitted in logical UTF-8 order. |
| 8 | `חישובˉסכום` | Hebrew RTL segments are admitted in logical UTF-8 order. |
| 9 | `HTTPˉОтвет` | Latin and Cyrillic are legal in separately validated visible segments. |
| 10 | `GPUˉتسريع` | Latin and Arabic are legal in separately validated visible segments. |
| 11 | `API応答` | UTS #39's Latin+Han Highly Restrictive combination is admitted. |
| 12 | `GPU가속` | UTS #39's Latin+Hangul Highly Restrictive combination is admitted. |
| 13 | `項目12` | One ASCII decimal-number system after a Han start is admitted. |
| 14 | `عنصر١٢` | One Arabic-Indic decimal-number system after an Arabic start is admitted. |
| 15 | `한글ˉ처리` | NFC Hangul syllables remain their exact canonical identity. |
| 16 | The multilingual paper program uses `en@1` keywords | All project identifiers compile independently of keyword localization. |
| 17 | The same exact identifier is referenced repeatedly | Exact identity resolves normally; it is not a confusable pair with itself. |
| 18 | Confusable spellings exist only in unrelated lookup scopes | Both are legal; a broader editor warning may remain. |
| 19 | A dependency collision is assigned a nonconfusable local alias | Local lookup proceeds through the explicit alias and canonical declaration identity. |
| 20 | U+061C, U+200E, or U+200F occurs once between complete body tokens | The mark is semantically ignored and retained in raw-source provenance. |
| 21 | One admitted implicit mark occurs after an RTL identifier before punctuation | Source-aware and plain-text-capable tools retain logical token order. |
| 22 | Arabic or Hebrew strong characters occur in a line comment | They remain comment text through exact LF/EOF and cannot change tokenization. |
| 23 | Arabic or Hebrew strong characters occur in a text/raw literal | They remain exact runtime text subject to the ordinary literal contract. |
| 24 | Balanced stateful bidi controls remain inside one comment/literal atom and line at depth at most 16 | Source is admitted; tools visibly annotate the controls. |
| 25 | `\u{202e}` occurs in an ordinary text literal | ASCII source is safe; the runtime text value contains U+202E. |
| 26 | A source-aware editor renders an RTL identifier | It isolates the token atom and preserves LTR syntax order and logical cursor spans. |
| 27 | Copy stored source is used on a visually reordered line | Clipboard bytes remain exact logical UTF-8, not display order. |
| 28 | Canonical reveal is used on an Arabic/Hebrew name | Exact source, canonical project/declaration identity, code points, and hashes are visible. |
| 29 | Formatter receives admitted RTL identifiers and no explicit bidi rewrite request | It preserves identifiers/content and never mirrors grammar or changes profile. |
| 30 | Scanner sees a project identifier at 256 bytes/128 scalars/32 segments within all three bounds | It may admit it if every other rule passes. |
| 31 | Skeleton maps receive many noncolliding names | Work remains linear/hashed and retains bounded per-name evidence. |
| 32 | Windows and Linux consume identical Unicode-profile bytes | They must return identical admission, code-point spans, skeletons, and diagnostics. |
