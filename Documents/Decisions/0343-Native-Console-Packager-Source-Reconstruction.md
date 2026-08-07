# Decision 0343: Native console-packager source reconstruction

- Status: Accepted local implementation; dual-host qualification and host-container reconstruction pending
- Date: 2026-08-07
- Advances: [Decision 0303](0303-Digest-Bound-Native-Console-Packager-Candidate.md) and [Decision 0342](0342-Native-Segmented-Console-Application-Construction.md)
- Uses: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) and [Decision 0253](0253-Native-Built-WebAssembly-Interpreter.md)

## Context

The ordinary and segmented console-packager projects both failed through the
pinned native Project 1 build route with the generic typed-WIR report
`Sourceˉbindings`. That report was initially treated as an accepted-subset
capacity frontier.

The dedicated native binding diagnostic instead accepts the ordinary closure
at 51 functions and 159 locals and the segmented closure at 64 functions and
181 locals. Both are far below the 4,096 per-function binding limit and the
4 MiB evidence limit. The actual difference was manifest order: both projects
listed the console plan before the console construction module, while canonical
module-name order places construction first.

Project 1 source directives are semantically order-independent. The pinned
native build driver still passes manifest order into its compiler unchanged,
so canonical inventory order is a bounded compatibility correction rather than
a new Project 1 requirement. Decision 0253 records the same retained driver
defect for the WebAssembly interpreter.

## Decision

- Put both console-packager dependency inventories in canonical module-name
  order without changing any source module, import, artifact, or semantic rule.
- Add one focused .NET-free command that rebuilds both WVBs through the normal
  digest-bound `Build-Wvb` front door, including compiler-aligned verification
  and native atomic publication.
- Require the rebuilt WVB sizes, complete SHA-256 identities, and both native
  build/publication reports to match the existing Stage 0-provenance candidates.
- Record native source reconstruction separately from host-container
  construction. The WVBs are now reconstructible without .NET; their checked-in
  PE/ELF tool containers remain Stage 0 recovery artifacts.
- Do not change the compiler's binding bounds or mislabel the native driver's
  order sensitivity as a language or Project 1 requirement.

## Evidence

The detailed native binding tool reports:

```text
source bindings status=Valid modules=6 functions=51 parameters=164 locals=159 reads=1386 assignments=263 calls=647 directory-bytes=12172
source bindings status=Valid modules=9 functions=64 parameters=198 locals=181 reads=1528 assignments=297 calls=734 directory-bytes=14332
```

The ordinary project reconstructs 58,127 WVB bytes at SHA-256
`7b055d4e6a456680a79eb28eaafa577e0019ea0ff1e34d9e713e9178428acc29`.
The segmented project reconstructs 68,451 WVB bytes at SHA-256
`33d7619c6115295a9eb612fd559031ab99c85196e3133a9405f880a19ac9ded2`.
Both identities exactly match the checked-in recovery-provenance candidates.

The focused Windows command passes 2/2. The reviewed retirement plan grows to
23 suites and 3,030 cases; the passing child is not rerun through the grouped
coordinator.

## Consequences

The source-to-verified-and-atomically-published-WVB seam for both console
packagers no longer needs .NET. This closes the falsely identified binding
blocker but not the complete packager row. Native construction of the paired
host containers, Linux execution of these exact builds, grouped qualification,
and ordinary-path promotion remain.

## Reconsideration triggers

Remove the canonical-inventory workaround when a qualified native build driver
implements Project 1 dependency-order independence. Rebuild and requalify the
evidence if either project source, compiler semantics, WVB format, verifier,
publisher, or candidate identity changes.
