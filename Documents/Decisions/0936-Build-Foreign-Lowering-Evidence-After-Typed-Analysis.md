# Decision 0936: build Foreign lowering evidence after typed analysis

## Status

Accepted and implemented locally on Windows on 2026-09-03. This decision
changes the private production phase order and removes the former pre-analysis
`wvbind` command form. It does not change the public source language, create a
compatibility path, complete native Foreign invocation, or claim Linux or
paired-host qualification.

## Context

The first authenticated production path constructed WVFB before the Analyzer.
That ordering made the private binder responsible for another complete source
body and generic-binding pass even though the Analyzer is the owner of typed
WVIR. It also made generic Foreign-call source depend on two independently
evolving interpretations before the carrier could be paired with the actual
typed operations.

The existing full binding module remains valuable as a focused semantic oracle,
but importing both it and a second production builder into one compiler product
exceeded the native profile's 128-type staging limit. Production needs one
small, ordered path rather than parallel body binders or a larger staging limit.

## Decision

1. After target admission and independent authentication, run the Analyzer's
   private Foreign-aware route before constructing WVFB. The Analyzer remains
   the only production owner of body parsing, generic binding, and typed WVIR.
2. Add a focused portable lowering builder that validates the retained WVSS,
   WVTD, and WVFC; reconstructs exact source symbols and registered callable
   facts; and constructs WVFB without repeating body or generic binding.
3. Replace the old four-path `wvbind` form with the private command:

   ```text
   wvbind --internal-bind-analyzed <input.wvss> <input.wvtd>
       <input.wvfc> <input.wvir> <output.wvfb>
   ```

4. Before publishing WVFB, require this command to pair the candidate carrier
   with the exact retained typed WVIR. The coordinator then validates and
   snapshots the carrier and invokes the independent pairing mode again before
   emission.
5. Recheck the authenticated source, target, catalog, evidence, lock, and
   profile snapshots at every existing boundary. Analyzer output is not
   authentication, and direct use of either private `wvbind` form grants no
   publication or call authority.
6. Keep `Compilerˉsourceˉforeignˉbinding` as the standalone semantic-test owner;
   it is not imported into the production `wvbind` product and is not a
   backward-compatibility execution route.
7. Route the production builder, driver, and project only to the
   `language-1-production-admission-ingress` owner. That owner builds and
   executes the complete private product; replaying the separate full-binding
   suite cannot reveal an additional defect in these production-only files.

## Implementation standing

Implementation commit `98334495cc7c501e1262a5939ebf68f473e55745` passes the
23-case `language-1-production-admission-ingress` owner on the local Windows
host. The rebuilt `wvbind` WVB is 774,256 bytes at SHA-256
`60a3b0b90b5a2f6d44bba49ae489aeb5352ae0cb86c77c0ea5e149f0c206aa3b`,
down from the preceding 988,400-byte candidate. The owner compiles an
authenticated generic source using the real Foundation Memory, Result, and
Unsafe modules before executing its emitted WVB.

The exact command, related runtime checkpoint, artifact identities, and
limitations are recorded in the
[authenticated Foreign scalar-execution evidence](../Evidence/2026-09-03-Authenticated-Foreign-Scalar-Execution.json).

## Consequences

- Production has one generic-aware body-analysis owner and one smaller
  post-analysis carrier builder.
- WVFB publication now proves agreement with the actual typed WVIR before the
  coordinator retains it, and the later independent pairing remains a separate
  fail-closed check.
- The obsolete pre-analysis command is intentionally unsupported; early
  development does not preserve it as a compatibility surface.
- The next compiler checkpoint is native ABI lowering and invocation of the
  same registered binding, followed by one real library boundary and Linux
  reproduction.

## Reconsideration triggers

Revisit this phase split if the Analyzer can own authenticated catalog facts
without crossing its size or authority boundary, if multiple bindings require
typed facts not reconstructible from WVIR, or if measured process boundaries
become the dominant compiler latency. Any replacement must keep authentication,
typed analysis, carrier construction, pairing, and final publication explicit
and fail closed.
