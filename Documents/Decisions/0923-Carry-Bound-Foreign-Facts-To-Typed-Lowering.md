# Decision 0923: carry bound Foreign facts to typed lowering

## Status

Proposed implementation checkpoint on 2026-09-02. A candidate WVFB 1.0
constructor and independent validator have been added to the private Foreign
binder and verified locally. Production consumption, paired-host evidence, and
acceptance remain pending.

## Context

The compiler already authenticates the exact retained source, target, and
Foreign catalog, binds each registered declaration to a Foreign callable, and
validates the exact System/FFI no-retain/no-unwind facts. It then discards those
normalized facts and stops at `Foreignˉloweringˉpending`. Passing WVTD and WVFC
directly into the main Analyzer would reintroduce the foreign-only semantic
closure that previously left just 616 bytes under the compiler's immutable
value limit.

The next lowering phase needs a compact versioned handoff, but that handoff
must not become a transferable authentication token or silently reuse an
ordinary function-call identity.

## Decision

1. Add private `WVFB 1.0` as specified by
   [Compiler source Foreign lowering carrier](../../Specifications/Compiler-Source-Foreign-Lowering-Carrier.md).
2. Construct one record for every completely authenticated-and-bound Foreign
   declaration. Each record retains its WVSS module and declaration, WVSD
   directory entry, WVFC record, exact registered semantic identities and
   types, and mandatory unsafe/no-retain/no-unwind facts.
3. Bind WVFB to the one currently accepted Linux x86-64 SysV AMD64 C v1 target.
   Reject malformed values independently before a consumer may inspect a
   record.
4. Keep WVFB non-authoritative. Binder success is still meaningful only inside
   the production coordinator's retained-snapshot control flow. An arbitrary
   valid WVFB grants no capability and authorizes no execution.
5. Return no partial carrier on source, catalog, target, symbol, body-binding,
   callable-fact, or carrier-validation failure.
6. Do not yet publish WVFB from `wvbind`, pass it into the Analyzer, change
   WVLB/WVIR/WVCA, assign a Foreign operation, add a WVB import, resolve a native
   symbol, or remove `Foreignˉloweringˉpending`.

## Verification

Extend the existing `language-1-authenticated-foreign-binding` owner rather
than adding another compiler reconstruction. One combined test application
carries the existing core and portable fixture modules through one cached
profile-7 package. Its canonical cases validate the exact 136-byte one-record
carrier, fixed target and semantic fields, deterministic construction, and
empty-on-failure rule. Its portable multi-module case validates the exact
216-byte two-record ordering. Malformed carrier cases cover length, magic,
version, header geometry, count, target, normalized facts, and reserved bytes
through the independent validator.

The focused owner is the implementation gate. Documentation verification and
changed-file routing verification protect the new format owner and causal test
selection. Production ingress is not rerun for this checkpoint because neither
the `wvbind` command contract nor coordinator behavior changes.

## Consequences

The authenticated-call path now has a bounded typed fact carrier that can cross
the capacity boundary into later lowering without duplicating catalog parsing
inside the Analyzer. The next chunk must make `wvbind` publish WVFB to a
coordinator-owned destination, bind that file to retained inputs, and introduce
the paired WVLB/WVIR identity and typed Foreign-call operation. Runtime and
native containment remain later gates.

## Nonclaims

This checkpoint does not lower a source invocation, publish compiler analysis
products, change WVB, execute a Foreign call, expose a host pointer, load a
library, call a provider, complete Slice 8, accept Decisions 0893 or 0895, or
qualify Language 1.0.
