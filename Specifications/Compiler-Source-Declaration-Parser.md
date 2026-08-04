# Windvale source declaration parser

## Status and purpose

`Compilerˉsourceˉdeclarationˉparser` is the second Windvale-written compiler slice. It performs the namespace-shaping declaration pass over strict UTF-8 source and exposes immutable byte-span views without constructing a token or declaration collection.

The current implementation is cross-host qualified at `fc87a3e` under Decision 0026. It imports the qualified `Compilerˉsourceˉlexer`, which imports `Foundationˉdecimalˉparsing`.

## Pass boundary

The pass recognizes:

- `module` name and `portable`, `hosted`, or `system` profile;
- ordered `import` declarations;
- qualified capability names;
- `text`, `[i32]`, and `bytes` data declarations and literal shape;
- optional export, name, explicit type, and bounded initializer span for `const` declarations;
- record names, fields, and non-void/non-array field types, including `i64` and `u64`;
- enum names, members, and unsuffixed nonnegative integer token shape;
- optional function export, name, parameters, return type, and balanced body span over the complete implemented primitive and nominal type surface.

Function-body tokens are balanced through the matching outer brace, including nested blocks. A constant initializer is retained as the declaration's body span from its first expression token through the token before the required semicolon; the later symbol phase owns expression grammar, typing, evaluation, and diagnostics. Function statement and expression grammar is deliberately outside this declaration pass.

## Streaming records

`Compilerˉsourceˉheader` contains parse/lex status, profile, module-name byte span, next cursor, failure position, and expected/found token kinds.

`Compilerˉsourceˉdeclaration` contains parse/lex status, declaration kind, export flag, name/declaration/body byte spans, item count, next cursor, failure position, and expected/found token kinds. `Constant = 7` is appended to the declaration-kind mapping. `Items` means qualified-name parts for capabilities, literal elements for array data, fields for records, members for enums, and parameters for functions; it is zero for constants.

`Compilerˉsourceˉmoduleˉsummary` contains the header identity, declaration-category counts, lexer token count, end cursor, and first-failure evidence. Every field is primitive or enum data; no record owns another record or runtime handle.

## Entry points

```text
Compilerˉparseˉheader(Input) -> Compilerˉsourceˉheader
Compilerˉparseˉheaderˉvalidated(Input) -> Compilerˉsourceˉheader

Compilerˉparseˉnextˉdeclaration(
    Input,
    Offset,
    Line,
    Column,
    Sawˉnonˉimport
) -> Compilerˉsourceˉdeclaration

Compilerˉparseˉnextˉdeclarationˉvalidated(...) -> Compilerˉsourceˉdeclaration
Compilerˉparseˉsource(Input) -> Compilerˉsourceˉmoduleˉsummary
Compilerˉparseˉdeclarationˉat(Input, Wanted) -> Compilerˉsourceˉdeclaration
```

Safe standalone entry points lexically validate the complete source. A compiler pass should validate once, call the validated header entry, then repeatedly call the validated next-declaration entry with the returned cursor. `Sawˉnonˉimport` enforces that imports precede every other declaration.

`Compilerˉparseˉdeclarationˉat` rescans from the header and exists for bounded tests/inspection. It is not the intended semantic-pass iteration path.

## Status and limits

`Compilerˉparseˉstatus` contains `Valid`, `Lexicalˉerror`, `Expectedˉtoken`, `Invalidˉprofile`, `Importˉafterˉdeclaration`, `Unexpectedˉdeclaration`, `Invalidˉtype`, `Invalidˉdataˉtype`, `Invalidˉliteral`, `Nestingˉlimit`, and `Itemˉlimit`.

The declaration pass enforces:

- the existing 4,194,304-byte source and 262,144-token lexer ceilings;
- at most 4,096 top-level declarations;
- at most 64 record fields or function parameters;
- at most 256 enum members;
- at most 16 qualified-name parts;
- at most 64 nested braces in a function body.

Input collections cannot exceed the lexer token ceiling. A constant initializer must contain at least one token, end at a semicolon, and remain inside the same source and token bounds. Empty/trailing-comma array and parameter forms follow the current Stage 0 syntactic behavior; semantic constant evaluation, cardinality, and uniqueness checks remain later passes.

## Hosted evidence boundary

`Source-Declaration-Parser-Tool.wv` owns only explicit argument/file capabilities and line reporting. It does not alter or duplicate parsing. With one path it reads one bounded immutable snapshot, calls `Compilerˉparseˉsource`, emits one path-free summary on success, and emits one deterministic diagnostic line on rejection.

The current candidate hosted tool parses the real lexer as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=17 tokens=6175 offset=51134
```

It parses its own declaration source as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=32 tokens=15098 offset=112327
```

## Current candidate artifacts

- `Source-Declaration-Parser.wvb`: 151,197 bytes, SHA-256 `8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb`.
- `Source-Declaration-Parser-Demo.wvb`: 154,365 bytes, SHA-256 `9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf`, result `0` under 20,000,000 instructions.
- `Source-Declaration-Parser-Tool.wvb`: 151,731 bytes, SHA-256 `ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0`.

The real lexer completes under 30,000,000 instructions. The larger self-declaration pass completes under 45,000,000. These are local deterministic constant-declaration candidates, not a successor cross-host qualification claim, and do not change assembler or linker ceilings.

The declaration pass was originally cross-host qualified at `fc87a3e`. Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055 uses the validated lexer boundary and a bounded string/comment/brace scanner to locate function-body ends after complete lexical validation; it falls back to the token scanner for rejected or ambiguous shapes. The implementation is cross-host qualified at `1a4fca7`. This remains a declaration-pass contract only; statement/expression parsing, binding, WIR construction, and WVB encoding are separate slices.
