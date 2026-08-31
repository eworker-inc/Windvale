# Decision 0897: lower lexical unsafe invocation contexts

## Status

Accepted and implemented as a focused local Windows checkpoint on 2026-08-31.
This decision does not complete Slice 8 or claim paired-host qualification.
Authenticated Foreign lowering, WVB representation, verifier/runtime/native
containment, and one migrated runtime or OS boundary remain pending.

## Context

The frozen Language 1.0 System profile already parses unsafe function
declarations, Foreign declarations, and statement or value `unsafe` blocks.
Authenticated source ingress and private Foreign binding establish which
declarations exist and which registered ABI facts they carry, but the typed
WVIR producer previously did not preserve the source-level invocation boundary.
An unsafe direct call could therefore follow the ordinary callable path, while
an unsafe named-function value could not be represented at all because
function-reference admission rejected every nonzero declaration flag.

Unsafe is a review and checking boundary, not authority. A System source
profile does not make every operation unsafe, marking a function unsafe does
not grant its body ambient permission, and taking a function reference does not
perform an invocation. Conversely, an indirect call must not erase the unsafe
classification merely because source lookup has already produced a callable
value.

Adding an `unsafe` WVIR operation would give a lexical compile-time fact an
unnecessary runtime representation and create work for every downstream
consumer. The smaller contract is to check the source scope while constructing
typed WVIR and retain only the callable flag needed when invocation is delayed.

## Decision

1. Track one compiler-private unsigned unsafe depth in typed-WVIR construction.
   The admitted range is `0..64`; deeper source nesting fails as an invalid
   value block before publication.
2. Enter and leave that depth around both statement and value `unsafe` blocks.
   Restore the enclosing depth on successful and failed lowering paths.
3. Reset the depth to zero whenever WVIR construction starts an ordinary or
   synthetic function. Lexical unsafe context never crosses a function
   boundary.
4. Treat an unsafe function declaration as a restriction on invocation, not as
   an implicit unsafe context for its body. A wrapper function must use an
   explicit unsafe block around the operation it audits.
5. Reject a direct call of an unsafe function or Foreign declaration outside a
   lexical unsafe context with appended exact status
   `Unsafeˉcontextˉrequired = 47`. Preserve all earlier status values.
6. Admit a nongeneric named-function reference with an explicit empty effect
   clause and the already supported by-value parameter/result profile when its
   only declaration flags are `async` and `unsafe`. Retain those exact bits in
   its private WVFT instance. Reference construction itself is safe.
7. At indirect invocation, read the authoritative WVFT flags and require the
   same lexical unsafe context when bit `1` (`unsafe`) is present. Do not infer
   safety from the local variable, structural signature, or call spelling.
8. Emit no WVIR operation, block, temporary, operand, serialized unsafe flag,
   or new WVIR version for an unsafe wrapper. The wrapper disappears after the
   invocation checks have succeeded.
9. Let binding discovery enter unsafe statement blocks so lexical locals,
   references, and calls inside them receive the same binding evidence as an
   ordinary nested block.
10. Keep this boundary separate from capability grants, pointer safety,
    authenticated Foreign operation construction, ABI lowering, memory
    containment, and terminal-failure isolation.

## Evidence

A current split Analyzer rebuilt successfully from the modified compiler. Its
source-analysis phase consumed 2,163,890 source bytes and published 3,887,400
WVIR bytes; optimized emission produced a 1,579,735-byte Analyzer WVB at
SHA-256
`95624b2109a8d12e3b022f95633317f640ed0c498a369b5764394322e804e890`.
The packaged Windows Analyzer is 51,749,888 bytes at SHA-256
`7fc0640a9737460dd001ae9710c72f58c02e973d6a92e63f1483cbb387be85c3`.

`Tools/Native/Test-Language-1.0-Unsafe-Wir.mjs` injects that Analyzer into 13
bounded cases. Ten valid paths cover statement, value, nested,
explicit-wrapper, local-value, safe direct, safe baseline, unsafe
named-reference, and unsafe indirect invocation behavior. Three cases reject direct invocation,
declaration-body ambient permission, and indirect invocation outside the
context with exact status `47`. Two comparisons prove safe and wrapped forms
have identical function, block, operation, temporary, and operand counts. The
focused run reports:

```text
native language 1 unsafe WIR status=Passed cases=13 valid=10 rejected=3 transparency=2
```

This is local implementation evidence. It is not a replacement for the final
paired-host Slice 8 gate.

## Consequences

- Unsafe review boundaries become enforceable before Foreign instructions or
  native ABI calls are added.
- Ordinary safe code and downstream WVIR consumers pay no serialized-format or
  runtime cost for the lexical wrapper.
- Unsafe callable values preserve their restriction across local storage and
  indirect invocation.
- Diagnostics point at the invocation that requires review rather than at the
  declaration or the surrounding System profile.
- Later Foreign lowering can reuse this exact check while adding its own typed
  pointer, ABI, verifier, runtime, and containment evidence.

## Reconsideration triggers

Reconsider the private depth representation if a future language edition makes
unsafe authority first-class or transferable, if callable values require a
public serialized effect/flag identity beyond WVFT, or if a verified native
operation needs explicit unsafe provenance after source checking. Such a change
requires a new versioned contract and must not silently reinterpret this
compile-time lexical boundary.
