# Windvale Language 1.0 lexical and grammar specification

## Status and ownership

This is the normative-candidate token and parsing companion to the
[Language 1.0 semantic specification](Windvale-Language-1.0.md), authorized by
[Decision 0751](../Documents/Decisions/0751-Accept-Windvale-Language-1.0-Direction.md).
It defines candidate edition-1 spelling exactly enough for paper programs and
parser planning. Current compilers implement
[Windvale Seed](Seed-Language.md), not this grammar.

This document owns tokenization, literal spelling, delimiter rules, productions,
and precedence. The semantic specification owns typing, ownership, effects,
evaluation, failure, and profile behavior. The
[Foundation companion](Windvale-Language-1.0-Foundation.md) owns the identities
and contracts of required standard types used below.

Structured-task spelling is the highest-risk grammar in this candidate and
remains subject to the paper corpus before source freeze. It has one exact
candidate form here; there is no alternate accepted spelling.

## Grammar notation

Productions use this notation:

- `Name ::= Form` defines `Name`.
- `A B` is concatenation.
- `A | B` is choice.
- `[A]` is optional.
- `{A}` is zero or more repetitions.
- `{A}+` is one or more repetitions.
- quoted text is a token.
- `EOF` is end of source.

Lexical productions operate on Unicode scalar values after strict UTF-8
decoding. Syntactic productions operate on tokens. A compiler must place a
finite limit on tokens, nesting, list items, and diagnostic recovery.

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
Asciiˉletter ::= "A" … "Z" | "a" … "z" | "_"
Asciiˉdigit ::= "0" … "9"
Segment ::= Asciiˉletter { Asciiˉletter | Asciiˉdigit }
Identifier ::= Segment { "ˉ" Segment }
Constantˉidentifier ::= ("A" … "Z" | "_")
                        { "A" … "Z" | Asciiˉdigit | "_" }
~~~

U+02C9 is the only non-ASCII scalar admitted inside an identifier. A keyword is
recognized only when the complete token has the exact lowercase ASCII spelling.
A keyword followed by U+02C9 is not a keyword prefix.

Edition 1 reserves:

~~~text
application as async authority await base bool borrow break bytes cancel_join
capability case const continue copy core data derive edition effects else enum
export f32 f64 fail_join false fn for foreign hosted i8 i16 i32 i64 if
implement import in join let library match module move mut never optional
platform policy profile protocol record requires return rune scope service
system task text true try u8 u16 u32 u64 unit unsafe using var variant version
where
~~~

The Foundation may define capitalized type and function names but cannot add
keywords without a new source edition.

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
Unicodeˉescape ::= "\u{" Hexˉdigits "}"
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
Rawˉdelimiter ::= { "#" }   // zero through eight
~~~

The closing delimiter must contain exactly the opening number of hashes. Raw
content performs no escape, normalization, or indentation processing and may
contain LF. Raw byte content is ASCII plus LF only.

Interpolated text begins with `$` followed by an ordinary or multiline text
literal. `{ Expression }` inserts one value through the Foundation formatting
protocol. `{{` and `}}` produce literal braces. Interpolation is not admitted in
raw or byte literals. Every interpolation has an explicit or statically derived
maximum output bound under the Foundation contract.

~~~text
Interpolatedˉtext ::= "$" (Interpolatedˉordinary | Interpolatedˉmultiline)
Interpolatedˉordinary ::= '"' { Interpolationˉliteralˉitem
                                | Interpolationˉfield } '"'
Interpolatedˉmultiline ::= '"""' { Interpolationˉmultilineˉitem
                                    | Interpolationˉfield } '"""'
Interpolationˉfield ::= "{" Expression "}"
~~~

The interpolated scanner emits literal segments and interpolation delimiters.
Inside a literal segment, `{{` and `}}` are escaped braces, a single `{` begins
an expression, and an unmatched single `}` is rejected. An interpolation
expression uses ordinary tokenization and balanced delimiters; braces within its
nested literals do not close the field.

## Module header and imports

~~~text
Source ::= Edition Module Profile Platform Authority
           { Capabilityˉrequirement }
           { Import }
           { Declaration } EOF

Edition ::= "edition" "1" ";"
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

Platform scopes and capability requirements are unique and canonical. Required
and optional identities cannot overlap. Imports precede all other declarations.

## Declarations

~~~text
Declaration ::= Recordˉdeclaration | Enumˉdeclaration | Variantˉdeclaration
              | Protocolˉdeclaration | Implementˉdeclaration
              | Deriveˉdeclaration | Functionˉdeclaration
              | Foreignˉdeclaration | Constantˉdeclaration
              | Dataˉdeclaration

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

### Constants and data

~~~text
Constantˉdeclaration ::= [ Documentation ] Visibility "const"
                         Constantˉidentifier ":" Type
                         "=" Constantˉexpression ";"
Dataˉdeclaration ::= [ Documentation ] Visibility "data" Identifier ":" Type
                     "=" Constantˉexpression ";"
~~~

`const` is storage-free. `data` creates one immutable module value whose type and
initializer are admitted by the semantic and Foundation contracts.

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

The first literal is the canonical ABI identity and the second is the exact
external symbol. Both must be ordinary non-interpolated single-line text.

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
signature's one borrowed parameter. Borrow types are not permitted in user
record or variant fields, module data, constants, owned stored collections,
tasks, serializable formats, or unrestricted escaping aggregates.

## Statements and blocks

~~~text
Block ::= "{" { Statement } "}"
Valueˉblock ::= "{" { Statement } Expression "}"

Statement ::= Letˉstatement | Varˉstatement | Destructureˉstatement
            | Assignmentˉstatement | Expressionˉstatement
            | Ifˉstatement | Matchˉstatement | Whileˉstatement
            | Forˉstatement | Usingˉstatement | Taskˉscopeˉstatement
            | Unsafeˉstatement
            | Returnˉstatement | Breakˉstatement | Continueˉstatement

Letˉstatement ::= "let" Identifier [ ":" Type ] "=" Expression ";"
Varˉstatement ::= "var" Identifier [ ":" Type ] "=" Expression ";"
Destructureˉstatement ::= ("let" | "var") Pattern "=" Expression ";"
Assignmentˉstatement ::= Place Assignmentˉoperator Expression ";"
Assignmentˉoperator ::= "=" | "+=" | "-=" | "*=" | "/=" | "%="
Expressionˉstatement ::= Expression ";"
Returnˉstatement ::= "return" [ Expression ] ";"
Breakˉstatement ::= "break" ";"
Continueˉstatement ::= "continue" ";"
~~~

`return;` is valid only for `unit`. A `never` function has no reachable return.

~~~text
Ifˉstatement ::= "if" Expression Block
                 { "else" "if" Expression Block }
                 [ "else" Block ]
Whileˉstatement ::= "while" Expression Block
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
rather than adding a second spawn expression. The paper corpus must prove that
this spelling expresses join, cancellation, provider restart, GUI, and service
cases without hidden work before source freeze.

## Patterns

~~~text
Pattern ::= "_"
          | Identifier
          | Qualifiedˉsourceˉname
          | Qualifiedˉsourceˉname
            "{" [ Fieldˉpattern { "," Fieldˉpattern } [ "," ] ] "}"
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

~~~text
Primaryˉexpression ::= Literal | "true" | "false" | "()"
                     | Identifier
                     | "(" Expression ")"
                     | Nominalˉconstruction
                     | Recordˉupdate
                     | Closure
                     | Unsafeˉexpression
                     | Interpolatedˉtext

Literal ::= Integerˉliteral | Floatˉliteral | Runeˉliteral
          | Textˉliteral | Byteˉliteral
          | Multilineˉtext | Multilineˉbytes
          | Rawˉtext | Rawˉbytes

Nominalˉconstruction ::= Qualifiedˉsourceˉname
                         "{" Fieldˉvalue { "," Fieldˉvalue } [ "," ] "}"
Fieldˉvalue ::= Identifier ":" Expression
Recordˉupdate ::= Qualifiedˉsourceˉname "base" Expression
                  "{" Fieldˉvalue { "," Fieldˉvalue } [ "," ] "}"
~~~

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
Capabilities and resources follow the same rule. A noncapturing closure uses
`[]`.

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

Before source freeze, the grammar must have:

- one machine-readable grammar generated from or checked against this document;
- lexical tests for every UTF-8, identifier, comment, literal, delimiter, and
  lookalike boundary;
- accepted and rejected precedence cases;
- complete examples for every declaration, type, statement, expression, pattern,
  closure, resource, task, and unsafe form;
- bounded recovery tests for truncated and malicious source; and
- editor grammar and formatter agreement over the paper corpus.
