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
- record names, fields, and non-void/non-array field types;
- enum names, members, and unsuffixed nonnegative integer token shape;
- optional function export, name, parameters, return type, and balanced body span.

Function-body tokens are balanced through the matching outer brace, including nested blocks. Statement and expression grammar is deliberately outside this declaration pass.

## Streaming records

`Compilerˉsourceˉheader` contains parse/lex status, profile, module-name byte span, next cursor, failure position, and expected/found token kinds.

`Compilerˉsourceˉdeclaration` contains parse/lex status, declaration kind, export flag, name/declaration/body byte spans, item count, next cursor, failure position, and expected/found token kinds. `Items` means qualified-name parts for capabilities, literal elements for array data, fields for records, members for enums, and parameters for functions.

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

Input collections cannot exceed the lexer token ceiling. Empty/trailing-comma array and parameter forms follow the current Stage 0 syntactic behavior; semantic cardinality and uniqueness checks remain a later pass.

## Hosted evidence boundary

`Source-Declaration-Parser-Tool.wv` owns only explicit argument/file capabilities and line reporting. It does not alter or duplicate parsing. With one path it reads one bounded immutable snapshot, calls `Compilerˉparseˉsource`, emits one path-free summary on success, and emits one deterministic diagnostic line on rejection.

The candidate parses the real lexer as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=2 enums=3 functions=17 tokens=5384 offset=45588
```

It parses its own declaration source as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=25 tokens=9561 offset=70591
```

## Current candidate artifacts

- `Source-Declaration-Parser.wvb`: 105,321 bytes, SHA-256 `4bbaaaa6293ab1fb5a4eb92c3e8a52c078943ba88652b27f69fdc3c5ab76fda7`.
- `Source-Declaration-Parser-Demo.wvb`: 109,443 bytes, SHA-256 `ab28936fe0961261a0f243009d5c9b93af52069326618e03e428d1cc024fea11`, result `0` under 20,000,000 instructions.
- `Source-Declaration-Parser-Tool.wvb`: 107,228 bytes, SHA-256 `94134e28bef9544b0fbb4b4ae6dfd3deb3aa52598475023d37b01a5de8686d45`.

The real lexer completes under 30,000,000 instructions. The larger self-declaration pass completes under 45,000,000. These are compiler-front-end qualification ceilings and do not change assembler or linker ceilings.

The declaration pass was originally cross-host qualified at `fc87a3e`. Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055 uses the validated lexer boundary and a bounded string/comment/brace scanner to locate function-body ends after complete lexical validation; it falls back to the token scanner for rejected or ambiguous shapes. The implementation is cross-host qualified at `1a4fca7`. This remains a declaration-pass contract only; statement/expression parsing, binding, WIR construction, and WVB encoding are separate slices.
