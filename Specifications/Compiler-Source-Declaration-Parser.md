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
- record names, optional bounded generic parameters, fields, and
  non-void/non-array field types, including `i64` and `u64`;
- enum names, members, and unsuffixed nonnegative integer token shape;
- variant names, optional bounded generic parameters, and zero-through-64
  uniquely shaped case-field declarations; and
- optional function export, name, optional bounded generic parameters,
  parameters, return type, and balanced body span over the complete implemented
  primitive and nominal type surface.

A generic parameter is either one type-parameter identifier or
`const Name: Integerˉtype`, where the constant type is exactly one fixed signed
or unsigned integer type. A list contains at most 32 parameters and retains its
source span; type substitution, constant evaluation, duplicate detection,
protocol requirements, and specialization remain semantic-phase work.

Function-body tokens are balanced through the matching outer brace, including nested blocks. A constant initializer is retained as the declaration's body span from its first expression token through the token before the required semicolon; the later symbol phase owns expression grammar, typing, evaluation, and diagnostics. Function statement and expression grammar is deliberately outside this declaration pass.

## Streaming records

`Compilerˉsourceˉheader` contains parse/lex status, profile, module-name byte span, next cursor, failure position, and expected/found token kinds.

`Compilerˉsourceˉdeclaration` contains parse/lex status, declaration kind, export flag, name/declaration/body byte spans, item count, next cursor, failure position, and expected/found token kinds. `Constant = 7` is appended to the declaration-kind mapping. `Items` means qualified-name parts for capabilities, literal elements for array data, fields for records, members for enums, cases for variants, and ordinary value parameters for functions; it is zero for constants. Generic parameters remain source spans in this syntax pass and do not change `Items`. The symbol phase still admits only the exact Foundation Option and Result identities until Slice 4's bounded general specialization phase is connected.

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
- at most 64 variant fields in one case and 32 generic parameters in syntax;
- at most 256 enum members;
- at most 16 qualified-name parts;
- at most 64 nested braces in a function body.

Input collections cannot exceed the lexer token ceiling. A constant initializer must contain at least one token, end at a semicolon, and remain inside the same source and token bounds. Empty/trailing-comma array and parameter forms follow the current Stage 0 syntactic behavior; semantic constant evaluation, cardinality, and uniqueness checks remain later passes.

## Hosted evidence boundary

`Source-Declaration-Parser-Tool.wv` owns only explicit argument/file capabilities and line reporting. It does not alter or duplicate parsing. With one path it reads one bounded immutable snapshot, calls `Compilerˉparseˉsource`, emits one path-free summary on success, and emits one deterministic diagnostic line on rejection.

The current candidate hosted tool parses the real lexer as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=3 enums=3 functions=19 tokens=6881 offset=56312
```

It parses its own declaration source as:

```text
source declarations status=Valid imports=1 capabilities=0 data=0 records=4 enums=4 functions=32 tokens=15142 offset=112567
```

## Current candidate artifacts

- `Source-Declaration-Parser.wvb`: 151,414 bytes, SHA-256 `b321d235e521dcae246bd20f627e3e5c0f117110a9150a934282ce834c0ffa62`.
- `Source-Declaration-Parser-Demo.wvb`: 154,582 bytes, SHA-256 `8696e7d83f7e7f8b0e0fc3d4d7cc9c79dab2b5bc1732c11bdf3633ff4e90fca9`, result `0` under 20,000,000 instructions.
- `Source-Declaration-Parser-Tool.wvb`: 151,948 bytes, SHA-256 `98e06a28dbc9f39dd23930deddc2018eb169d37bbb9b28b954bd4db6894f043a`.

Decision 0516 makes the native Project 1 front door the ordinary constructor
for all three WVBs and binds the core's exact portable type/export surface
through native inspection. The demo and capability-bearing hosted-tool runs
remain in the managed differential lane because the scalar native runner does
not complete the demo and does not bind the tool's console, diagnostic, file,
and process capabilities. This is local Windows transfer evidence, not a new
cross-host qualification claim.

The real lexer completes under 30,000,000 instructions. The larger self-declaration pass completes under 45,000,000. These are local deterministic constant-declaration candidates, not a successor cross-host qualification claim, and do not change assembler or linker ceilings.

The declaration pass was originally cross-host qualified at `fc87a3e`. Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`. Decision 0055 uses the validated lexer boundary and a bounded string/comment/brace scanner to locate function-body ends after complete lexical validation; it falls back to the token scanner for rejected or ambiguous shapes. The implementation is cross-host qualified at `1a4fca7`. This remains a declaration-pass contract only; statement/expression parsing, binding, WIR construction, and WVB encoding are separate slices.
