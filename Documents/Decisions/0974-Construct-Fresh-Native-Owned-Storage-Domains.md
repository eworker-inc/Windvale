# Decision 0974: Construct fresh native owned-storage domains

## Status

Proposed implementation contract, 3 October 2026. The existing ownership-to-
storage priority supplies the need for this candidate constructor; acceptance,
launcher integration and installed qualification remain separate gates.

## Problem and result

ABI 24 Main requires an initialized physical allocator, canonical budget state,
binding adapter and context 10. Current focused tests construct those states
themselves. Ordinary console startup still supplies context 7, so adding more
collection operations cannot make that startup execute owned-storage programs.

A shared runtime constructor now provides the next integration component. It
checks a fresh caller-supplied domain and initializes the existing leaves before
publishing context 10. Invalid requests preserve the caller's bytes. Existing
source semantics, ABI 24, context 10 and historical container bytes are unchanged.

## Candidate contract

The runtime-private 112-byte version-one request names the context, physical
state, canonical accounting state, binding adapter and arena, along with their
finite limits and a fresh epoch. Complete mappings and exclusive ownership are
caller preconditions. Checked extent arithmetic, alignment, fifteen pairwise
comparisons, a bounded stack exclusion and a complete fresh-metadata scan
precede domain writes. Reinitializing a live or retired domain is refused.

The root budget uses the existing identity-one/generation-one token. Its maximum
must fit the supplied physical arena, with at most 64 children. The constructor
does not acquire host memory or create a payload lease. It invokes the existing
physical and budgeted leaves and publishes their exact linked provider entry.
Unexpected initialization refusal restores fixed metadata and the old arena
header. The [owned-domain specification](../../Specifications/Windvale-Native-Owned-Domain.md)
owns the exact fields, statuses, bounds and lifetime requirements.

## Evidence and remaining work

The existing native storage owner adds focused fixtures covering successful
initialization, allocation and credit, all pointer-pair conflicts, limit and
header rejection, fresh-state refusal, preserved registers and provider-failure
rollback. Exact final commands, tested revision and results belong in the commit
summary. No extra verifier entry point or immutable bootstrap artifact is added.

Normal console construction/admission must next provide an explicit owned-domain
layout and call this constructor through the shared startup path. Hosted console
format two already has a different contract; it cannot be repurposed silently.
Current-source packaging, normal success/trap cleanup, paired host execution and
independent reconstruction remain to be completed before launcher qualification.
Shared immutable backing, broader ownership composition, interpreter working
storage and the maintained typed Package-Lock consumer also remain open.
