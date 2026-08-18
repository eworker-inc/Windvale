# Bidirectional source and display

## One logical grammar direction

Windvale source grammar has one logical left-to-right token order. Selecting an
Arabic, Hebrew, or Chinese source profile changes admitted token spellings, not
the order of declarations, operands, delimiters, calls, or named arguments.
There is no mirrored RTL grammar and no file-direction field in the source
descriptor.

UTF-8 byte/scalar order is source identity. Visual order is presentation. A
compiler never infers token order from a rendered line.

## Exact line boundary

Windvale admits LF and CRLF as already specified. It rejects these literal raw-
source scalars everywhere, including inside comments and text/raw literals:

| Scalar | Name |
| --- | --- |
| U+000B | LINE TABULATION / vertical tab |
| U+000C | FORM FEED |
| U+0085 | NEXT LINE |
| U+2028 | LINE SEPARATOR |
| U+2029 | PARAGRAPH SEPARATOR |

An editor might display any of them as a hard line break while the Windvale
comment scanner would otherwise continue to LF. Global rejection removes that
line-spoofing ambiguity. A program needing the scalar as runtime text uses an
ordinary `\u{...}` escape; raw source cannot contain it literally.

## Directional marks between tokens

After the ASCII-only byte-zero descriptor, exactly these implicit directional
marks may occur at a complete token or logical-line boundary:

- U+061C ARABIC LETTER MARK (`ALM`);
- U+200E LEFT-TO-RIGHT MARK (`LRM`); and
- U+200F RIGHT-TO-LEFT MARK (`RLM`).

At most one mark is admitted at one boundary. It is ignored for token/semantic
construction but retained in raw-source hashing, spans, diffs, and provenance.
It is not whitespace, cannot split an identifier or other token, and cannot
appear in the universal descriptor. Any other default-ignorable outside comment
or text/rune/raw-literal content is invalid source.

Inside comment or text/rune/raw-literal content, these marks are content rather
than ignored token separators.

## Stateful directional controls in content

Literal U+202A..U+202E embedding/override controls and U+2066..U+2069 isolate/
pop controls are never admitted in identifiers, keywords, public labels, or
between executable tokens. They may occur as deliberate Unicode data inside one
comment-content or text/rune/raw-literal-content atom only when:

- UAX #9 revision 51 stack processing balances them completely within that one
  atom and logical line;
- no effect crosses a delimiter, token, or line boundary; and
- nesting never exceeds 16.

Unbalanced runtime data remains expressible with `\u{...}` escapes, whose ASCII
source spelling has no bidi effect. Tools always expose literal stateful controls
in a show-invisibles view and may require an explicit security-policy approval
for their use even when the source is valid.

Join controls and variation selectors inside comment/text content are preserved
as data and shown by the same tooling. They never become valid identifier
characters.

## Source-aware rendering

Editors, semantic diffs, diagnostic renderers, and source viewers follow the
Basic Ordering for Source Code in Unicode Technical Standard #55 version 2,
revision 5:

- each source line has LTR structural order;
- token boundaries are atom boundaries;
- an identifier/keyword atom uses the direction of its content while remaining
  in its logical token position;
- comment and literal delimiters are separate syntax atoms from their content;
- numeric atoms remain LTR;
- comment/literal content can use first-strong presentation inside its isolated
  atom; and
- cursor, selection, diagnostics, breakpoints, and copy preserve logical raw
  source spans rather than visual glyph order.

Rendering uses higher-level isolation; an editor does not silently insert bidi
control bytes merely to display a file. A separate explicit plain-text-bidi
operation may add/remove the three admitted implicit marks and must be
idempotent, semantics-preserving, visibly reported, and covered by Workload 3's
transactional conversion rules.

## Invisibles and diagnostics

Editors provide independent controls to show ASCII whitespace, default-
ignorables/joiners/variation selectors, and non-NFC text. A visible marker must
not suppress the character's real shaping/directional effect. Diagnostics show:

- logical raw source with the affected scalar escaped or annotated;
- exact UTF-8 byte and scalar span;
- Unicode code point and stable reason category;
- source-profile/Unicode-table identity; and
- bounded related scope/control information.

Color alone is not sufficient to distinguish comments, literals, identifiers,
and syntax. Source-aware console review eventually needs UTF-8 decoding, glyph
and shaping support, bidi layout, lexical atom isolation, and fallback fonts;
those are console/editor implementation requirements rather than different
language semantics.

## Plain-text and Git behavior

Git stores exact logical bytes. Raw diffs are always available. A source-aware
diff additionally shows token atoms, explicit code-point annotations for hidden
controls, and canonical semantic identities. Copy stored source preserves raw
logical order; displayed glyph order is never copied as if it were source.

Tools that cannot render source structurally show escaped control annotations
and may choose a logical-order fallback. They must not reorder stored bytes or
claim that visual column order is the compiler token order.
