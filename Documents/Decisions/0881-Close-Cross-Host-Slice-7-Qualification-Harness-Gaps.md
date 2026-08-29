# Decision 0881: close cross-host Slice 7 qualification harness gaps

## Status

Accepted implementation correction on 2026-08-29. Final paired-host evidence
for the corrected source state remains pending.

## Context

Qualification run 33252478692 against commit `62de9c22` passed classification,
both native bootstrap jobs, both native WebAssembly jobs, and the first native
qualification shard on both hosts. The six failing behavior shards exposed five
independent verification integration gaps:

- the common split-compiler runner allocated its internal product below the
  compatibility spelling of the Windows temporary directory. A runner using
  `RUNNER~1` therefore rejected its own otherwise valid product as noncanonical
  in the enum and asynchronous-call owners;
- the Linux Language 1.0 front-door wrapper passed its implemented 466 cases but
  did not contain the 16 bounded `Vector.Constructˉreserved` cases already owned
  by its Windows peer and the 482-case registry contract;
- the WVDB query capability passed all six Linux cases and published the current
  exact WVB identity, but the owner registry retained its predecessor digest;
- the historical standard-byte-output owner could not inspect the exact frozen
  recovery commit inside the Debian container because Git classified the mounted
  checkout as having dubious ownership; and
- the ordinary database durable-commit owner still rebuilt the current lowerer.
  Its current output no longer matched identities produced by the older lowerer
  even though database behavior was unchanged.

These failures occurred after the relevant compiler, runtime, or database
behavior had either passed or reached its exact artifact boundary. They do not
justify changing the immutable Seed or weakening any semantic or binary check.

## Decision

1. Resolve the common split compiler's temporary root and allocated child with
   the operating system's native canonical path operation. Validate cleanup
   against that canonical root and retain bounded removal of only the allocated
   child.
2. Give the Linux front-door owner the same bounded vector-reservation valid,
   malformed, source-rejection, ownership-rejection, and WVB-boundary cases as
   Windows. Both hosts report the same 482-case terminal contract.
3. Bind `wvdb-query-capability` to its current exact WVB SHA-256
   `77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b`.
   Refresh the unchanged 114-owner, 5,616-case registry identity to
   `c2868e57d513b4d51c1356c93fcf12108e73c3471be0f322189e7f2ec67c4765`.
4. Allow the historical recovery owner to trust only the already-resolved
   repository path through Git's per-command `safe.directory` option. Do not
   write global Git configuration, admit a wildcard, change the recovery commit,
   or bypass its exact reconstruction evidence.
5. Make the database durable-commit behavior owner verify and consume the exact
   retained lowerer rather than reconstructing compiler tooling. Keep duplicate
   WVB compilation and WVO lowering, exact comparison, validation, linking,
   cross-host packaging, and all 12 local executions.
6. Bind that retained-tool chain to these exact identities:

   - WVB: 107,828 bytes, SHA-256
     `479e631466733ae421d3477f61cedf1f716aa993cfecd7da560818a9d6dc4b60`;
   - WVO: 2,011,950 bytes, SHA-256
     `39eaa1823df0e4dfabda085eb3894d47b940a06a4d44a4f0d637aa08a5a4a4a5`;
   - linked image: 2,008,436 bytes at unchanged entry 151,017, SHA-256
     `2f1182f785ad22e1011b0c76e1202b3fc436548c76d70d2be8fb5aa1f175e929`;
   - Windows application: 2,029,568 bytes, SHA-256
     `680d56c853b502b5bb76bffc3526752290da697eba707fa768ace644fb144b15`;
   - Linux application: 2,031,616 bytes, SHA-256
     `6969a296c7d0819175b9a5b1dd4c64c5245d056be9d674b947f08d92f3ab0a5e`.

## Consequences

- one canonical split-compiler repair covers every nested user of that runner;
  individual conformance owners do not need path-specific exceptions;
- Windows locally passes the complete 482-case Language 1.0 front door,
  including the enum boundary and all 16 vector-reservation cases;
- the database durable-commit owner locally passes all 12 cases and both host
  images without rebuilding the lowerer;
- the historical standard-byte-output owner locally passes all ten cases while
  retaining its exact recovery commit and deterministic byte oracle;
- the verification planner passes 31 general and 205 native routing cases with
  no owner-coverage gap; and
- Debian execution of the restored vector cases and scoped Git trust, plus the
  Windows short-path async case, still require the same-state final
  qualification run before Slice 7 can close.

## Reconsideration triggers

Rebuild a lowerer inside the durable-database owner only if compiler
construction becomes part of that owner's declared behavior. Broaden Git trust
only if a future isolated checkout has its own exact resolved repository path.
Replace exact byte identities only after a named compiler or format change is
validated structurally and the complete downstream chain is regenerated.
