# Unicode workload 4 validation record

## Exact inputs

All 11 files named by Workload 1's `Unicode-17-Source.wvup` were downloaded from
their recorded Unicode Consortium URLs on 2026-08-18. Every exact byte length and
SHA-256 matched the committed profile. The files total 9,326,660 bytes.

The checked source identity is:

~~~text
windvale.unicode17.source@1
Unicode 17.0.0
UAX #15 revision 57
UAX #31 revision 43
UTS #39 revision 32
~~~

Source display/control decisions additionally name
[UAX #9 revision 51](https://www.unicode.org/reports/tr9/tr9-51.html) and
[UTS #55 version 2, revision 5](https://www.unicode.org/reports/tr55/tr55-5.html).
They do not replace or silently update the hashed source-profile tables.

## Mechanical identifier checks

The 15 concrete accepted spellings in `Accepted-Cases.md` were decoded to exact
scalars and checked for:

- NFC;
- `XID_Start`/`XID_Continue` membership;
- `Identifier_Status=Allowed`;
- absence of `Default_Ignorable_Code_Point`;
- script/Script_Extensions coverage and the required restriction level; and
- one decimal-number system per U+02C9-delimited segment.

All 15 passed. The 24 concrete malformed identifiers in rejected cases 1 through
22 plus the two first confusable pairs produced their expected failure or equal
skeleton result. The Hebrew U+05D5/U+05DF pair was independently found as an
admitted same-script skeleton collision.

The exact `NormalizationTest.txt` records confirm:

- U+00E9 is NFC and decomposes canonically to U+0065 U+0301;
- U+AC00 is NFC and decomposes to U+1100 U+1161; and
- U+D55C is NFC and decomposes to U+1112 U+1161 U+11AB.

The exact `confusables.txt` records include the mappings used by the selected
collision cases, including Cyrillic U+0455 to Latin `s`, Greek U+039A to Latin
`K`, Greek U+03A1 to Latin `P`, and both Hebrew U+05D5/U+05DF to Latin `l`.

An additional exact intersection check found zero scalars that are simultaneously
`XID_Continue`, `Identifier_Status=Allowed`, and `Bidi_Mirrored=Yes`. Therefore
identifier-segment `bidiSkeleton` needs the pinned Bidi_Class, normalization,
default-ignorable, and confusable inputs but no unrecorded mirroring glyph for an
admitted edition-1 scalar. Full source/editor rendering still needs separately
versioned UAX #9 display data; Workload 5 owns its shipment/cache binding because
display bytes are not compiler semantic input.

## Paper source

`Source/Multilingual-Identifiers.wv` uses canonical `en@1` keywords with eight
project-owned function names covering Latin, Cyrillic, Greek, Han, Japanese,
Korean, Arabic, and Hebrew. Its exact hash is recorded beside the source.

The file is a Language 1.0 paper fixture. Current Seed compilers do not implement
the Language 1.0 source descriptor or Unicode boundary, so no AST/WIR/WVB/native
result is claimed.

## Evidence boundary

This validation establishes exact candidate data membership and internally
consistent paper expectations. Future conformance must independently implement
the full pinned NFC, restriction-level, mixed-number, `bidiSkeleton`, UAX #9
control-stack, source-span, diagnostic, and source-aware-display algorithms on
Windows and Linux. It may not use this first-author checker as the compiler.
