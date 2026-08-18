# Windvale Language 1.0 lexical and grammar specification

## Status and ownership

This is the normative-candidate token and parsing companion to the
[Language 1.0 semantic specification](Windvale-Language-1.0.md), authorized by
[Decision 0751](../Documents/Decisions/0751-Accept-Windvale-Language-1.0-Direction.md)
and refined by
[Decision 0754](../Documents/Decisions/0754-Resolve-First-Language-1.0-Paper-Findings.md)
and
[Decision 0760](../Documents/Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md)
and
[Decision 0762](../Documents/Decisions/0762-Resolve-Language-1.0-Numeric-Graphics-Findings.md)
and
[Decision 0764](../Documents/Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md),
with complete-suite reconciliation accepted by
[Decision 0765](../Documents/Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md).
It defines candidate edition-1 spelling exactly enough for paper programs and
parser planning. Current compilers implement
[Windvale Seed](Seed-Language.md), not this grammar.

The project owner has reopened the candidate through the working
[localized-source and source-vocabulary addendum](Windvale-Language-1.0-Localized-Source.md).
This companion incorporates that addendum's replacement-candidate lexical and
source-descriptor direction. It is not a source-freeze or implementation claim.

This document owns tokenization, literal spelling, delimiter rules, productions,
and precedence. The semantic specification owns typing, ownership, effects,
evaluation, failure, and profile behavior. The
[Foundation companion](Windvale-Language-1.0-Foundation.md) owns the identities
and contracts of required standard types used below.

The [machine grammar](Windvale-Language-1.0.ebnf) is the canonical
machine-readable projection of the productions in this document. Its external
scanner tokens are resolved by this document's strict UTF-8, comment, literal,
and raw-delimiter contracts. A mismatch is a source-freeze
blocker; neither file silently overrides the other.

The complete paper corpus confirms the one structured-task and contextual
array-literal spelling recorded here. There is no alternate accepted spelling.

## Grammar notation

Productions use this notation:

- `Name ::= Form` defines `Name`.
- `A B` is concatenation.
- `A | B` is choice.
- `[A]` is optional.
- `{A}` is zero or more repetitions.
- `{A}+` is one or more repetitions.
- quoted punctuation, delimiter, numeric, and universal-descriptor text is an
  exact source token;
- other quoted keyword text names the corresponding canonical keyword token
  after source-lexicon mapping;
- `EOF` is end of source.

Lexical productions operate on Unicode scalar values after strict UTF-8
decoding. Syntactic productions operate on tokens. A compiler must place a
finite limit on tokens, nesting, list items, and diagnostic recovery.

The working source edition candidate pins the Unicode scalar-property,
normalization, and security tables used by localized lexicons and identifiers to
`windvale.unicode17.source@1`, whose exact Unicode 17.0.0 inputs and hashes are
defined by the
[source-profile artifact formats](Windvale-Language-1.0-Source-Profile-Formats.md).
Host Unicode tables are not grammar input. The replacement source freeze must
accept that exact identity after the remaining multilingual workload evidence.

## Source decoding and line structure

A source file:

- is strict UTF-8 without a byte-order mark;
- contains no surrogate value, overlong encoding, or invalid sequence;
- accepts LF U+000A or CRLF as one logical line ending;
- rejects a lone CR U+000D;
- may contain horizontal space U+0020 and tab U+0009 between tokens; and
- treats no other Unicode whitespace as syntactic whitespace.

Before tokenization, CRLF becomes one logical LF. That normalization applies
inside ordinary, multiline, and raw literal content, so a checked-in source file
and an external Windows-edited source file do not inherit different newline
semantics. Canonical formatted and repository source uses LF. Source-byte hashes
remain hashes of the original admitted bytes.

Indentation is never semantic. A tab has no specified display width.

## Comments and documentation

`//` starts an ordinary comment through the next LF or EOF. `///` starts a
documentation comment. There are no block comments and therefore no nested
comment rule.

A consecutive documentation-comment group attaches to the immediately following
module or declaration when no blank line or ordinary comment intervenes.
Documentation text excludes the leading `///` and at most one following ASCII
space. Documentation does not change executable source semantics or artifact
identity unless a package separately includes a documentation artifact.

The lexer emits one `Documentationˉtoken` for the complete attached group:

~~~text
Documentation ::= Documentationˉtoken
~~~

## Identifiers and keywords

~~~text
Asciiˉdigit ::= "0" … "9"
Identifierˉstart ::= "_" | Unicodeˉxidˉstart
Identifierˉcontinue ::= "_" | Unicodeˉxidˉcontinue
Identifierˉsegment ::= Identifierˉstart { Identifierˉcontinue }
Identifier ::= Identifierˉsegment { "ˉ" Identifierˉsegment }
Constantˉidentifier ::= Identifier
~~~

`Unicodeˉxidˉstart` and `Unicodeˉxidˉcontinue` are the exact edition-pinned
Unicode property classes after subtracting every scalar forbidden by the
[localized-source specification](Windvale-Language-1.0-Localized-Source.md).
U+02C9 is excluded from both classes and remains the semantic-word separator.
Every identifier must already use the specified normalization form. Exact
ordinal UTF-8 bytes determine identity; no host normalization, case folding,
collation, transliteration, or canonically-equivalent alias is admitted.

A keyword is recognized only when the complete token has the selected source
lexicon's exact primary spelling and an admitted following boundary. A keyword
followed by U+02C9 or any identifier continuation is not a keyword prefix.

Edition 1 reserves 76 body words:

~~~text
application as async authority await base bool borrow break bytes cancel_join
capability case const continue copy core data derive effects else enum export f32
f64 fail_join false fn for foreign hosted i8 i16 i32 i64 if implement import in
join let library match module move mut never optional maximum package platform
policy profile protocol record requires return rune scope service system task text
true try u8 u16 u32 u64 unit unsafe using var variant version where
~~~

The selected source profile maps the 66 words defined by the localized-source
specification to canonical token identities. The ten fixed-width numeric type
words remain exact in every profile. The universal source descriptor is metadata,
not part of this reserved-word set. Registered machine identities also retain
their exact canonical spelling. The Foundation may define types and functions
but cannot add keywords without a new source edition.

Module names, aliases, source declarations, fields, cases, and protocol names use
`Identifier`. Constants use `Constantˉidentifier` by official convention.
Capability and ABI identities use their separately specified ASCII-safe token
inside the productions that admit them.

~~~text
Qualifiedˉsourceˉname ::= Identifier { "." Identifier }
Capabilityˉidentity ::= Lowerˉsegment { "." Lowerˉsegment }
Lowerˉsegment ::= ("a" … "z") { "a" … "z" | Asciiˉdigit | "_" }
Platformˉscope ::= Lowerˉsegment { "." Lowerˉsegment }
Abiˉidentity ::= Lowerˉsegment { "." Lowerˉsegment }
Effectˉidentity ::= Lowerˉsegment { "." Lowerˉsegment }
~~~

## Numeric literals

Underscores may separate digits but cannot lead, trail, repeat, or immediately
follow a radix prefix. A sign is a unary operator, not part of a literal.

~~~text
Decimalˉdigits ::= Asciiˉdigit { [ "_" ] Asciiˉdigit }
Hexˉdigit ::= Asciiˉdigit | "a" … "f" | "A" … "F"
Hexˉdigits ::= Hexˉdigit { [ "_" ] Hexˉdigit }
Unicodeˉhexˉdigits ::= Hexˉdigit [ Hexˉdigit [ Hexˉdigit
                         [ Hexˉdigit [ Hexˉdigit [ Hexˉdigit ] ] ] ] ]
Binaryˉdigits ::= ("0" | "1") { [ "_" ] ("0" | "1") }

Integerˉbody ::= Decimalˉdigits
               | "0x" Hexˉdigits
               | "0b" Binaryˉdigits
Integerˉsuffix ::= "i8" | "i16" | "i32" | "i64"
                 | "u8" | "u16" | "u32" | "u64"
Integerˉliteral ::= Integerˉbody [ Integerˉsuffix ]
~~~

An unsuffixed integer literal requires one exact expected integer type. A
suffixed value must fit its type. Radix affects spelling only; it never selects
signedness, width, byte order, or wrapping.

Decimal floating literals contain a decimal point or exponent. Hexadecimal
floating literals require a binary exponent. The suffix may be omitted only
under one exact expected `f32` or `f64` type.

~~~text
Exponent ::= ("e" | "E") [ "+" | "-" ] Decimalˉdigits
Decimalˉfloat ::= Decimalˉdigits "." Decimalˉdigits [ Exponent ]
                | Decimalˉdigits Exponent
Binaryˉexponent ::= ("p" | "P") [ "+" | "-" ] Decimalˉdigits
Hexˉfloat ::= "0x" Hexˉdigits [ "." Hexˉdigits ] Binaryˉexponent
Floatˉsuffix ::= "f32" | "f64"
Floatˉliteral ::= (Decimalˉfloat | Hexˉfloat) [ Floatˉsuffix ]
~~~

Literal conversion uses the exact strict floating profile. A decimal literal is
converted as if from its exact decimal rational value; a hexadecimal literal is
converted from its exact binary rational value. Both use roundTiesToEven.

## Rune, text, and byte literals

An ordinary rune literal uses single quotes and contains one Unicode scalar or
one admitted escape:

~~~text
Runeˉliteral ::= "'" (Runeˉscalar | Textˉescape | Unicodeˉescape) "'"
Unicodeˉescape ::= "\u{" Unicodeˉhexˉdigits "}"
Textˉescape ::= "\" ( "\" | "'" | '"' | "n" | "r" | "t" | "0"
                      | "{" | "}" )
Byteˉescape ::= Textˉescape | "\x" Hexˉdigit Hexˉdigit
~~~

`\u{...}` contains one through six hex digits and must name a Unicode scalar.
`\n` is LF, `\r` is carriage return, `\t` is tab, and `\0` is U+0000. An
ordinary text literal uses double quotes, admits text and Unicode escapes, and
cannot contain a literal LF.

~~~text
Textˉliteral ::= '"' { Textˉscalar | Textˉescape | Unicodeˉescape } '"'
Byteˉliteral ::= "b" '"' { Asciiˉbyte | Byteˉescape } '"'
~~~

A byte literal contains only ASCII source scalars plus escapes. `\xNN` appends
one exact byte. A Unicode escape is not admitted in a byte literal.

The literal scanners use these exact character classes:

- `Runeˉscalar` is one Unicode scalar other than apostrophe, backslash, LF, or
  CR;
- `Textˉscalar` is one Unicode scalar other than double quote, backslash, LF, or
  CR;
- `Asciiˉbyte` is U+0020 through U+007E except double quote and backslash;
- multiline text items are text scalars, LF, escapes, or quote characters that
  do not begin the closing triple quote;
- multiline byte items are ASCII bytes, LF, escapes, or quote characters that
  do not begin the closing triple quote; and
- raw content is the longest scalar sequence before the first exact closing
  quote-plus-hash delimiter; raw byte content additionally permits only ASCII and
  LF.

A multiline text literal uses three double quotes. Its content begins
immediately after the opening delimiter and ends immediately before the next
unescaped closing delimiter. Literal LF values are retained. No leading or
trailing newline is inserted or removed, no indentation is stripped, and no host
newline conversion occurs.

~~~text
Multilineˉtext ::= '"""' { Multilineˉtextˉitem } '"""'
Multilineˉbytes ::= "b" '"""' { Multilineˉbyteˉitem } '"""'
~~~

Escapes have the same meaning as in ordinary literals. A closing quote may be
escaped. Multiline bytes retain literal ASCII and LF only; other bytes use
`\xNN`.

A raw literal uses zero through eight `#` delimiters:

~~~text
Rawˉtext ::= "r" Rawˉdelimiter '"' Rawˉcontent
             '"' Rawˉdelimiter
Rawˉbytes ::= "br" Rawˉdelimiter '"' Rawˉbyteˉcontent
              '"' Rawˉdelimiter
Rawˉdelimiter ::= [ "#" [ "#" [ "#" [ "#"
                    [ "#" [ "#" [ "#" [ "#" ] ] ] ] ] ] ] ]
~~~

The closing delimiter must contain exactly the opening number of hashes. Raw
content performs no escape, normalization, or indentation processing and may
contain LF. Raw byte content is ASCII plus LF only.

Edition 1 has no interpolated-text literal. The complete paper corpus uses
explicit bounded text builders and the Foundation formatting protocol, and it
does not establish an allocation-budget or destination-owner contract for a
standalone interpolation expression. A later edition may add interpolation only
with those ownership and failure inputs visible. `$"..."`, `$"""..."""`, and
interpolation in raw or byte literals are therefore lexical errors in edition 1.

## Module header and imports

~~~text
Source ::= Sourceˉdescriptor Module Profile Platform Authority
           { Capabilityˉrequirement }
           { Import }
           { Declaration } EOF

Sourceˉdescriptor ::= "#!wv/1" " " Sourceˉprofile "@"
                      Sourceˉprofileˉversion Sourceˉdescriptorˉend
Sourceˉprofile ::= Sourceˉprofileˉcomponent
                   { "." Sourceˉprofileˉcomponent }
Sourceˉprofileˉcomponent ::= Sourceˉprofileˉatom
                             { "-" Sourceˉprofileˉatom }
Sourceˉprofileˉatom ::= ("A" … "Z" | "a" … "z")
                        { "A" … "Z" | "a" … "z" | Asciiˉdigit }
Sourceˉprofileˉversion ::= ("1" … "9") { Asciiˉdigit }
Module ::= [ Documentation ] "module" Identifier ";"
Profile ::= "profile" ("core" | "hosted" | "system") ";"
Platform ::= "platform" Platformˉscope
             { "," Platformˉscope } [ "," ] ";"
Authority ::= "authority" ("library" | "application" | "service" | "system")
              ";"

Capabilityˉrequirement ::= ("requires" | "optional") "capability"
                           Capabilityˉidentity "version"
                           Decimalˉdigits ";"
Import ::= [ Documentation ] "import" Identifier "as" Identifier ";"
~~~

`Sourceˉdescriptorˉend` is the externally scanned first logical line ending.
The descriptor begins at byte zero, is ASCII-only, has no byte-order mark,
comment, or whitespace before it, and occupies at most 128 bytes excluding that
line ending. Its profile identity occupies 2 through 96 bytes. Its version is a
positive decimal integer no greater than 4,294,967,295, with no leading zero,
sign, separator, or suffix. Profile identities and versions are case-sensitive.

The fixed descriptor reader resolves the one explicit immutable source profile,
which binds the source edition, exact keyword lexicon, public source-vocabulary
profile, Unicode data, and their content identities. `en@1` is the canonical
English profile but is never an ambient default. After admission, quoted keyword
terminals in the remaining grammar refer to canonical token identities rather
than English source bytes. The localized-source specification owns the semantic
descriptor/profile contract; the source-profile artifact format companion owns
its exact candidate bytes and admission order.

Platform scopes and capability requirements are unique and canonical. Required
and optional identities cannot overlap. Imports precede all other declarations.
Each `Platformˉscope` token is an opaque key in the semantic target-scope
registry. The comma-separated items are alternative predicates over one
structured build target, not grammar for combining environment, architecture,
ABI, extension, or capability dimensions. Periods inside a key imply no prefix
relationship.

## Declarations

~~~text
Declaration ::= Recordˉdeclaration | Enumˉdeclaration | Variantˉdeclaration
              | Protocolˉdeclaration | Implementˉdeclaration
              | Deriveˉdeclaration | Functionˉdeclaration
              | Foreignˉdeclaration | Constantˉdeclaration
              | Dataˉdeclaration | Packageˉdataˉdeclaration

Visibility ::= [ "export" ]
Genericˉparameters ::= "<" Genericˉparameter
                       { "," Genericˉparameter } [ "," ] ">"
Genericˉparameter ::= Identifier
                    | "const" Constantˉidentifier ":" Integerˉtype
Whereˉclause ::= "where" Requirement { "," Requirement } [ "," ]
Requirement ::= Type ":" Protocolˉinstance
Protocolˉinstance ::= Qualifiedˉsourceˉname [ Typeˉarguments ]
Typeˉarguments ::= "<" Typeˉargument { "," Typeˉargument } [ "," ] ">"
Typeˉargument ::= Type | Constantˉexpression
~~~

### Records

~~~text
Recordˉdeclaration ::= [ Documentation ] Visibility "record" Identifier
                       [ Genericˉparameters ] [ Whereˉclause ]
                       "{" Recordˉfield { Recordˉfield } "}"
Recordˉfield ::= [ Documentation ] Identifier ":" Type ";"
~~~

A record has at least one field. Empty structure uses `unit`.

### Enums

~~~text
Enumˉdeclaration ::= [ Documentation ] Visibility "enum" Identifier
                     ":" Integerˉtype
                     "{" Enumˉmember { Enumˉmember } "}"
Enumˉmember ::= [ Documentation ] Identifier "=" [ "-" ] Integerˉliteral ";"
~~~

An enum has at least one member. Its tags must be exact literals of its declared
integer type after contextual typing.

### Variants

~~~text
Variantˉdeclaration ::= [ Documentation ] Visibility "variant" Identifier
                        [ Genericˉparameters ] [ Whereˉclause ]
                        "{" Variantˉcase { Variantˉcase } "}"
Variantˉcase ::= [ Documentation ] Identifier
                 [ "(" Variantˉfield { "," Variantˉfield } [ "," ] ")" ] ";"
Variantˉfield ::= Identifier ":" Type
~~~

A variant has at least one case. An empty parameter list is invalid; omit it for
a no-data case.

### Protocols and implementations

~~~text
Protocolˉdeclaration ::= [ Documentation ] Visibility "protocol" Identifier
                         [ Genericˉparameters ] [ Whereˉclause ]
                         "{" Protocolˉitem { Protocolˉitem } "}"
Protocolˉitem ::= [ Documentation ] Functionˉsignature ";"

Implementˉdeclaration ::= [ Documentation ] "implement" Protocolˉinstance
                          "for" Type [ Whereˉclause ]
                          "{" Functionˉimplementation
                              { Functionˉimplementation } "}"
Functionˉimplementation ::= [ Documentation ] Functionˉhead Block

Deriveˉdeclaration ::= [ Documentation ] "derive" Protocolˉinstance
                       { "," Protocolˉinstance } [ "," ] "for" Type ";"
~~~

An implementation body contains exactly the protocol functions and no data.
`derive` is admitted only for protocols whose Foundation contract names a
bounded compiler derivation.

### Constants, data, and package data

~~~text
Constantˉdeclaration ::= [ Documentation ] Visibility "const"
                         Constantˉidentifier ":" Type
                         "=" Constantˉexpression ";"
Dataˉdeclaration ::= [ Documentation ] Visibility "data" Identifier ":" Type
                      "=" Constantˉexpression ";"
Packageˉdataˉdeclaration ::= [ Documentation ] Visibility "package" "data"
                             Identifier ":" Packageˉdataˉtype "maximum"
                             Integerˉliteral ";"
Packageˉdataˉtype ::= "bytes" | "text"
~~~

`const` is storage-free. `data` creates one immutable module value whose type and
initializer are admitted by the semantic and Foundation contracts.
`package data` creates one package-bound shared immutable module value. Its
maximum literal must have the exact `u64` suffix. There is no optional, inferred,
path-bearing, or runtime-loaded package-data form in edition 1.

### Functions

~~~text
Functionˉdeclaration ::= [ Documentation ] Visibility Functionˉhead Block
Functionˉhead ::= [ "unsafe" ] [ "async" ] "fn" Identifier
                  [ Genericˉparameters ] "(" [ Parameters ] ")"
                  "->" Type [ Effectˉclause ] [ Whereˉclause ]
Functionˉsignature ::= [ "unsafe" ] [ "async" ] "fn" Identifier
                       [ Genericˉparameters ] "(" [ Parameters ] ")"
                       "->" Type [ Effectˉclause ] [ Whereˉclause ]
Parameters ::= Parameter { "," Parameter } [ "," ]
Parameter ::= Identifier ":" Parameterˉtype
Parameterˉtype ::= "borrow" [ "mut" ] Type | Type
Effectˉclause ::= "effects" "(" [ Effectˉidentity
                                  { "," Effectˉidentity } [ "," ] ] ")"
~~~

A by-value owned parameter takes ownership. Copy and shared immutable types use
their ordinary transfer. Borrow modes are explicit.

### Foreign declarations

~~~text
Foreignˉdeclaration ::= [ Documentation ] Visibility "unsafe" "foreign"
                        Textˉliteral Functionˉsignature
                        "as" Textˉliteral ";"
~~~

The first literal is the canonical registered ABI-contract identity and the
second is the exact external symbol. Both must be ordinary non-interpolated
single-line text. Calling convention, target predicate, scalar/pointer layout,
retention, and unwind policy come from that exact registered contract; the
literal is not a host-default calling-convention nickname.

## Types

~~~text
Type ::= Primitiveˉtype
       | Qualifiedˉsourceˉname [ Typeˉarguments ]
       | Functionˉtype
       | Borrowˉtype

Primitiveˉtype ::= "unit" | "never" | "bool"
                 | Integerˉtype | "f32" | "f64"
                 | "rune" | "text" | "bytes"
Integerˉtype ::= "i8" | "i16" | "i32" | "i64"
               | "u8" | "u16" | "u32" | "u64"
Borrowˉtype ::= "borrow" [ "mut" ] Type
Functionˉtype ::= [ "unsafe" ] [ "async" ] "fn"
                  "(" [ Type { "," Type } [ "," ] ] ")"
                  "->" Type [ Effectˉclause ]
~~~

Borrow types may appear only where the semantic lifetime can be represented.
Edition 1 has no named lifetime grammar. A public borrowed result is tied to its
signature's one borrowed parameter. An ephemeral `Slice<T>` or
`Mutableˉslice<T>` parameter is one borrowed parameter whose lifetime comes from
its underlying owner, so a checked element result may inherit that sole
provenance. Borrow types are not permitted in user
record or variant fields, module data, constants, owned stored collections,
tasks, serializable formats, or unrestricted escaping aggregates.

## Statements and blocks

~~~text
Block ::= "{" { Statement } "}"
Valueˉblock ::= "{" { Statement } Expression "}"

Statement ::= Bindingˉstatement
            | Assignmentˉstatement | Expressionˉstatement
            | Ifˉstatement | Matchˉstatement | Whileˉstatement
            | Forˉstatement | Usingˉstatement | Taskˉscopeˉstatement
            | Unsafeˉstatement
            | Returnˉstatement | Breakˉstatement | Continueˉstatement

Bindingˉstatement ::= ("let" | "var") Pattern [ ":" Type ]
                      "=" Expression ";"
Assignmentˉstatement ::= Place Assignmentˉoperator Expression ";"
Assignmentˉoperator ::= "=" | "+=" | "-=" | "*=" | "/=" | "%="
Expressionˉstatement ::= Expression ";"
Returnˉstatement ::= "return" [ Expression ] ";"
Breakˉstatement ::= "break" ";"
Continueˉstatement ::= "continue" ";"
~~~

The optional binding type annotates the complete right-hand value before the
pattern is applied. It is valid for a simple identifier, discard, or structured
pattern and does not annotate one selected field. `let` makes every introduced
binding immutable; `var` makes every introduced binding mutable, subject to the
ordinary ownership and borrow rules.

`return;` is valid only for `unit`. A `never` function has no reachable return.

~~~text
Ifˉstatement ::= "if" Expression Block
                 { "else" "if" Expression Block }
                 [ "else" Block ]
Whileˉstatement ::= "while" Expression Block
                  | "while" "let" Pattern "=" Expression Block
Forˉstatement ::= "for" Pattern "in" Expression Block
Matchˉstatement ::= "match" Expression
                    "{" Matchˉstatementˉarm { Matchˉstatementˉarm } "}"
Matchˉstatementˉarm ::= "case" Pattern [ "if" Expression ] Block
~~~

### Resource scope

~~~text
Usingˉstatement ::= "using" Identifier "=" Expression Block
~~~

The expression must produce one owned resource, commonly through `try`. The
block owns that binding. `using` performs only the Foundation local-release
contract; fallible completion remains explicit source inside the block.

### Structured task scope

~~~text
Taskˉscopeˉstatement ::= "task" "scope" Identifier "=" Expression
                         "policy" Taskˉpolicy Block
Taskˉpolicy ::= "join" | "cancel_join" | "fail_join"
~~~

The expression constructs one bounded Foundation `Taskˉscope`. Inside the block,
an asynchronous closure is spawned by calling the scope's named `Spawn`
operation. `async` marks that closure or function as suspendable; `await` waits
for a typed task handle.

The candidate deliberately keeps scheduling and scope creation in Foundation
rather than adding a second spawn expression. The complete paper corpus proves
that this spelling expresses join, cancellation, provider restart, GUI, service,
and accelerator cases without hidden work.

The concurrent hosted-service workload confirms the spelling. Explicit
cancellation is the ordinary Foundation call
`Task.Requestˉcancel(Scope: borrow mut Scope)`; context derivation is
`Task.Operationˉcontext(Scope: borrow Scope)`. Neither needs another statement,
`spawn` expression, `select`, detached-task marker, thread keyword, or exception
form. Provider operations that may suspend are ordinary async calls and therefore
require the existing `await` unary expression.

## Patterns

~~~text
Pattern ::= "_"
          | Qualifiedˉsourceˉname
            [ "{" [ Fieldˉpattern { "," Fieldˉpattern } [ "," ] ] "}" ]
Fieldˉpattern ::= Identifier ":" Pattern
~~~

A bare identifier in a binding position introduces a binding. A qualified name
selects an enum member or no-data variant case. A field pattern selects a record
or variant case with named payload fields. Pattern meaning is resolved
semantically; token shape alone does not infer copying or moving.

## Expressions and precedence

Assignment is not an expression. From highest to lowest binding strength:

| Level | Forms | Association |
| ---: | --- | --- |
| 1 | primary, field, index, call | left |
| 2 | `try`, `await`, `borrow`, `borrow mut`, `!`, `~`, unary `-` | right |
| 3 | `*`, `/`, `%` | left |
| 4 | `+`, `-` | left |
| 5 | `<<`, `>>` | left |
| 6 | `<`, `<=`, `>`, `>=` | non-associative |
| 7 | `==`, `!=` | non-associative |
| 8 | `&` | left |
| 9 | `^` | left |
| 10 | `|` | left |
| 11 | `&&` | left, short-circuit |
| 12 | `||` | left, short-circuit |
| 13 | value `if` and `match` | right |

Relational and equality operators cannot chain. Parentheses are required for a
different grouping.

~~~text
Expression ::= Ifˉexpression | Matchˉexpression | Orˉexpression
Ifˉexpression ::= "if" Expression Valueˉblock
                 { "else" "if" Expression Valueˉblock }
                 "else" Valueˉblock
Matchˉexpression ::= "match" Expression
                     "{" Matchˉvalueˉarm { Matchˉvalueˉarm } "}"
Matchˉvalueˉarm ::= "case" Pattern [ "if" Expression ] Valueˉblock

Orˉexpression ::= Andˉexpression { "||" Andˉexpression }
Andˉexpression ::= Bitˉorˉexpression { "&&" Bitˉorˉexpression }
Bitˉorˉexpression ::= Bitˉxorˉexpression { "|" Bitˉxorˉexpression }
Bitˉxorˉexpression ::= Bitˉandˉexpression { "^" Bitˉandˉexpression }
Bitˉandˉexpression ::= Equalityˉexpression { "&" Equalityˉexpression }
Equalityˉexpression ::= Relationalˉexpression
                        [ ("==" | "!=") Relationalˉexpression ]
Relationalˉexpression ::= Shiftˉexpression
                          [ ("<" | "<=" | ">" | ">=") Shiftˉexpression ]
Shiftˉexpression ::= Additiveˉexpression
                     { ("<<" | ">>") Additiveˉexpression }
Additiveˉexpression ::= Multiplicativeˉexpression
                        { ("+" | "-") Multiplicativeˉexpression }
Multiplicativeˉexpression ::= Unaryˉexpression
                              { ("*" | "/" | "%") Unaryˉexpression }
~~~

A value block's final expression has no semicolon. This is an explicit grammar
position, not automatic semicolon insertion.

~~~text
Unaryˉexpression ::= ("try" | "await" | "!" | "~" | "-") Unaryˉexpression
                   | "borrow" [ "mut" ] Unaryˉexpression
                   | Postfixˉexpression
Postfixˉexpression ::= Primaryˉexpression { Postfixˉsuffix }
Postfixˉsuffix ::= "." Identifier
                 | "[" Expression "]"
                 | "(" [ Callˉarguments ] ")"
Callˉarguments ::= Positionalˉarguments | Namedˉarguments
Positionalˉarguments ::= Expression { "," Expression } [ "," ]
Namedˉarguments ::= Namedˉargument { "," Namedˉargument } [ "," ]
Namedˉargument ::= Identifier ":" Expression
~~~

Named and positional arguments cannot mix in one call.

An ordinary generic call uses the semantic specification's argument-derived
structural resolution. Edition 1 also admits this separate primary form, which
is required when a named generic function has no typed argument evidence:

~~~text
Explicitˉgenericˉcall ::= Qualifiedˉsourceˉname "::"
                          Typeˉarguments
                          "(" [ Callˉarguments ] ")"
~~~

The list supplies every generic parameter in declaration order. `::` is
mandatory, so `Name<T>(...)` remains invalid and `<` in an ordinary expression
remains relational. The form applies only to a resolved qualified named function,
not an arbitrary callable expression.

~~~text
Primaryˉexpression ::= Literal | "true" | "false" | "()"
                     | Identifier
                     | "(" Expression ")"
                     | Arrayˉliteral
                     | Nominalˉconstruction
                     | Recordˉupdate
                     | Explicitˉgenericˉcall
                     | Closure
                     | Unsafeˉexpression

Literal ::= Integerˉliteral | Floatˉliteral | Runeˉliteral
          | Textˉliteral | Byteˉliteral
          | Multilineˉtext | Multilineˉbytes
          | Rawˉtext | Rawˉbytes

Arrayˉliteral ::= "[" [ Expression { "," Expression } [ "," ] ] "]"

Nominalˉconstruction ::= Qualifiedˉsourceˉname
                         "{" Fieldˉvalue { "," Fieldˉvalue } [ "," ] "}"
Fieldˉvalue ::= Identifier ":" Expression
Recordˉupdate ::= Qualifiedˉsourceˉname "base" Expression
                  "{" Fieldˉvalue { "," Fieldˉvalue } [ "," ] "}"
~~~

An array literal requires one exact expected `Array<T, N>` type. It contains
exactly `N` expressions of exact type `T`, including zero expressions when
`N = 0`, and evaluates them from left to right once. It does not infer a common
element type, perform conversion, allocate dynamic backing, or provide a
repetition shorthand. Brackets remain unambiguous with postfix indexing and a
closure capture list because those occupy different grammar positions.

Name resolution distinguishes record construction from variant-case construction
and distinguishes an enum member, no-data variant case, function, module alias,
and value. A qualified name without braces is parsed through identifier plus
postfix field segments. Grammar shape does not provide overloading.

### Closures

~~~text
Closure ::= [ "async" ] "fn" Captureˉlist
            "(" [ Parameters ] ")" "->" Type
            [ Effectˉclause ] Block
Captureˉlist ::= "[" [ Capture { "," Capture } [ "," ] ] "]"
Capture ::= ("copy" | "move" | "borrow" | "borrow" "mut") Identifier
~~~

Every referenced outer local must appear exactly once in the capture list.
Capability references and resources stored in lexical locals follow the same
rule. A required module-bound singleton capability root is not a lexical local
and therefore is not spelled in the capture list; a qualified call through it
must still appear in the closure's exact effect clause and the module's required
capability closure. A noncapturing closure uses `[]`.

### Unsafe expression

~~~text
Unsafeˉstatement ::= "unsafe" Block
Unsafeˉexpression ::= "unsafe" Valueˉblock
~~~

An unsafe statement block produces `unit`; an unsafe value block produces its
tail expression. Either may appear only in a System module and does not suppress
type, ownership, range, effect, or capability checks unrelated to its named
unsafe operations.

## Places and mutation

~~~text
Place ::= Identifier { "." Identifier | "[" Expression "]" }
~~~

A place is assignable only when semantic analysis proves a mutable local, owned
mutable field, or exclusive mutable borrow. Parsing a place does not grant
mutation or aliasing.

## Constant expressions

Constant expressions are the expression subset admitted by the semantic
compile-time contract:

~~~text
Constantˉexpression ::= Expression
~~~

- typed literals and `()`;
- earlier constants;
- enum members;
- record and variant construction from constant fields;
- contextual fixed-array literals from constant elements;
- checked pure operators;
- value-producing `if` and `match`;
- admitted bounded Foundation constant functions; and
- no allocation, capability, resource, task, unsafe, FFI, environment, or
  unbounded generic work.

The compiler evaluates a constant under ordinary checked semantics. A would-trap
constant is a compile-time diagnostic.

## Delimiters and trailing commas

Every declaration and statement terminator shown as `;` is required. Braces,
parentheses, brackets, and angle brackets must balance before semantic analysis.

A trailing comma is admitted only where a production explicitly shows it. The
formatter should retain a trailing comma for a multiline list and omit it for a
single-line list, but formatting does not change parsing.

`<` begins generic arguments only where name resolution and the syntactic
generic position require it. Relational expressions never guess a generic parse.
Inside a type-argument production, adjacent `>` characters close nested generic
lists one at a time without required whitespace. In an expression operator
position, `>>` is the one right-shift token.

## Rejected ambiguities

Edition 1 deliberately rejects:

- mixing named and positional call arguments;
- implicit record fields or positional record construction;
- empty variant payload parentheses;
- an array literal without one exact expected `Array<T, N>` type;
- array repetition syntax or inferred common element type;
- chained comparisons;
- expression assignment;
- omitted `else` in a value-producing `if`;
- a final semicolon on the yielded expression of a value block;
- inferred closure capture;
- an unqualified imported member;
- keyword aliases or alternative macron spellings;
- host newline acceptance inside canonical source;
- indentation stripping from multiline literals; and
- preprocessor or macro token substitution.

## Parser freeze evidence

This is a source-design gate, not a claim that the current Seed parser accepts
edition 1. An accepted or rejected case is complete here when its exact source,
token/parse expectation, diagnostic category, recovery bound, and ownership in
the future conformance suite are recorded. Migration turns those cases into
executable parser, editor, and formatter tests before implementation
conformance is claimed.

Before source freeze, the machine grammar supplies the complete production
projection while the review must confirm:

- exact agreement between that machine grammar and this document;
- lexical tests for every UTF-8, identifier, comment, literal, delimiter, and
  lookalike boundary;
- exact source-descriptor, source-profile, source-lexicon, source-vocabulary,
  Unicode-table, keyword-boundary, and localized public-name resolution cases
  required by the localized-source companion;
- accepted and rejected precedence cases;
- full-arity explicit-generic call cases proving `::` disambiguation from
  relational `<`/`>` and rejection of bare, partial, or arbitrary-callable
  suffixes;
- contextual fixed-array literal cases proving exact count/type, empty arrays,
  trailing commas, left-to-right evaluation, and disambiguation from indexing
  and closure capture lists;
- complete examples for every declaration, type, statement, expression, pattern,
  closure, resource, task, and unsafe form;
- bounded recovery tests for truncated and malicious source; and
- editor grammar and formatter agreement over the paper corpus.
