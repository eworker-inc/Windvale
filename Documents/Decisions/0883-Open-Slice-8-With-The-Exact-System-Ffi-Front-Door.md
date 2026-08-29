# Decision 0883: open Slice 8 with the exact System/FFI front door

## Status

Accepted implementation checkpoint on 2026-08-29. Slice 8 remains in progress.

## Context

The frozen Language 1.0 grammar already defines an explicit `unsafe foreign`
declaration with a registered ABI-contract identity and exact external symbol.
The accepted System/FFI workload selects one Linux x86-64 SysV AMD64 C boundary:
`windvale.paper.buffer_source.sysv_amd64_c_v1` and
`wv_paper_buffer_source_read_v1`. Before this checkpoint, those rules existed in
the specification and paper corpus but the real compiler did not recognize the
`foreign` keyword or preserve declaration evidence.

Slice 8 must not begin by admitting arbitrary C declarations, inheriting a host
ABI, or granting ambient native authority. The first compiler seam needs a
small exact oracle before target selection, typed WIR, pointer and region
semantics, linking, containment, or execution can safely expand it.

## Decision

1. Append `Foreign = 115` to the source token identities and recognize the
   canonical English `foreign` spelling. Edition 1 admits the token; Seed does
   not.
2. Parse the frozen paper declaration only when it is explicitly `unsafe`, uses
   the exact registered ABI identity and external symbol, and matches the exact
   three-parameter, `i64`, `effects(ffi.call)` signature. Admit the existing
   compact and paper-layout spellings, including the grammar's optional trailing
   parameter comma. Reject escaped, unknown, or mismatched identities.
3. Admit foreign declarations only in the System source profile. `export`
   remains explicit and does not change the authority requirement.
4. Preserve bounded source spans for the ABI and symbol as compiler evidence.
   Do not lower a call, bind a symbol, choose a host ABI, or publish a native
   artifact at this checkpoint.
5. Add one deterministic native owner with 12 isolated cases covering keyword
   identity, edition gating, valid declaration forms, exact evidence, missing
   `unsafe`, ABI and symbol rejection, pointer and effect mismatch, terminator
   rejection, profile gating, and exported System admission.

## Consequences

- Slice 8 has a real compiler front door rather than a paper-only syntax claim.
- The owner registry advances to 115 owners and 5,630 cases in 19,198 LF-only
  bytes at SHA-256
  `32e21b116ce5691198c611d47829ec43c092d9dcb3ae99fcf5ac18a167be8231`.
- The focused owner passes all 12 cases through two byte-identical WVB builds
  and 12 isolated runner executions. The editor contract passes, and the
  changed-file planner passes 31 general and 206 native routing cases.
- Exact target enforcement, general grammar-driven foreign signatures, typed
  WIR and pointer evidence, ABI registry binding, linker resolution, SysV call
  lowering, hostile native execution, containment, and the first real migrated
  boundary remain subsequent Slice 8 checkpoints.
- No portable or Hosted module gains foreign-call authority, and no runtime or
  linker behavior changes in this checkpoint.

## Reconsideration triggers

Generalize the parser only after the semantic model can retain and validate the
same exact ABI, target, type, effect, pointer, lifetime, and symbol evidence.
Change token identity only before any published edition-1 serialized lexical
contract depends on 115. Expand beyond the paper ABI only through another named,
registered, tested contract rather than a host-default shortcut.
