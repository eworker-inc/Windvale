# Decision 0858: catalog deterministic source closure targets

## Status

Accepted on 2026-08-25.

## Context

Decision 0857 made a verified plain-capture environment executable but left
source closure-body lowering open. A source closure has no top-level WVSD
symbol, while WVIR and WVB physical functions previously corresponded only to
ordinary symbols and generic specializations. Treating a closure as a fake
source symbol would weaken directory validation and diagnostics. Compiling a
nested target recursively inside its parent would also make function ordering
and generic fixed-point resolution depend on mutable construction state.

The compiler needs a bounded, reproducible identity for every closure before it
can publish synthetic function entries and binding ranges.

## Decision

1. Compiler-private WVCL 1.0 evidence catalogs source closure sites. One site
   is exactly `(module, parent physical function, source offset, source length)`
   and carries one exact function-type instance and explicit capture count.
2. First deterministic traversal assigns the closure ordinal. Exact repeated
   admission reuses that ordinal. A repeated site with different type or
   capture evidence rejects; it cannot produce an alternate target.
3. A synthetic physical target is the checked sum of the ordinary symbol-entry
   count, final generic-function instance count, and closure ordinal. Closure
   targets therefore follow existing physical functions without pretending to
   be source declarations.
4. One catalog admits at most 256 closure sites, 64 captures per site, 8,192
   retained bytes, and 16,777,216 aggregate source-span bytes. Admission and
   target arithmetic fail closed. Full duplicate checking is bounded to 32,640
   pair comparisons.
5. WVCL validates magic, version, exact retained and aggregate accounting,
   spans, counts, and unique sites before use. It is compiler evidence, not a
   source, package, ABI, runtime, or authority-bearing format.
6. This checkpoint establishes planning identity only. Connecting WVCL to the
   compiler's fixed-point evidence, emitting `Closureˉcreate`, compiling
   synthetic bodies, and publishing synthetic binding ranges remain the next
   Slice 6 implementation step.
7. The accepted Language 1.0 source identity remains the Decision 0857
   amendment. No frozen grammar or semantic input changes in this checkpoint.

## Consequences

Closure lowering can use one final, append-only physical function order rather
than recursive insertion or forged source declarations. Diagnostics retain the
real parent and source span, and a generic-resolution rerun can reconstruct the
same closure ordinals before emitting final targets.

The catalog deliberately does not contain copied source text or binding
payloads. Later passes reparse the validated source span and reconstruct the
closure-local binding phase from capture analysis, preventing two retained
models from drifting.

## Evidence

The compiler component is a deterministic 14,524-byte WVB at SHA-256
`5cf39d57dd9f69cc0e3e90ae20742c907527481cd3d43c77af6ba1c4f672b13d`.
Its 23,078-byte self-test at SHA-256
`adbb332cd832d69c06660688508d8d45ea981f00f9563497de574790c47d977d`
executes with result `42`.

The focused Windows owner passes 50 cases across nine evidence modules:

```text
native language 1 callable semantics status=Passed cases=50 result=42 modules=9 wvb-bytes=4192390 evidence-sha256=27f585258368122eb2a78e520117d5f1a659614375e527e4e3275185eed8e5ca
```

Independent Linux reproduction, WVIR/WVB integration, repository-wide
Qualification, and promoted artifact repinning remain separate claims.

## Reconsideration triggers

Reconsider this decision if deterministic traversal cannot reproduce closure
ordinals across generic fixed-point iterations, if source spans cannot remain
stable diagnostics, or if a later backend requires a different physical target
order. Any replacement must retain checked target arithmetic, exact type and
capture identity, bounded evidence, malformed-input rejection, and no fake
source symbols.
