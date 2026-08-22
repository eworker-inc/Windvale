# Decision 0821: Implement fixed arrays as WVB 1.17

## Status

Accepted by the project owner on 2026-08-22 as the next Language 1.0 compiler
migration checkpoint. Current evidence is focused Windows development evidence;
paired-host conformance and release qualification remain separate gates. The
tracked segmented-compiler and hosted-container development candidates have
been refreshed and promoted locally to their canonical candidate directories.

## Context

Decision 0762 fixed contextual `[E0, E1, ...]` construction under one exact
`Array<T, N>` expected type. The compiler already had ordinary generic nominal
identity and materialization after Decisions 0804 through 0819, but no array
expression, typed fixed-array layout, portable bytecode representation, verifier,
or executable value existed. The growing analyzer also exceeded the earlier
32 MiB native hosted-product payload and eight-fragment construction profile.

The first three-element experiment additionally exposed a development-cache
bug: sorting dependencies by project filename can differ from the canonical
ordinal UTF-8 order of their declared module identities. A `*-Main.wv` file is
the common counterexample.

## Decision

`Foundationˉcollections.Array<T, N>` is the exact edition-1 fixed-array
identity only when reached through the canonical Foundation module import. `N`
is an exact `u64` compile-time value from zero through 4,095. A contextual array
literal evaluates its exact `T` elements left to right once. Checked indexing
uses an exact `u64` and traps with `WVR3008` when the index is not below `N`.
No common-type inference, conversion, repetition, hidden capacity, growth, or
dynamic backing contract is added.

The current source parser and WIR operation representation admit at most 64
items in one literal. This is an explicit bounded compiler construction limit;
it does not reduce the serialized fixed-array type bound or create a different
source type.

WVGT intrinsic kind `10` owns the concrete `T, N` identity. WVIR operation `165`
constructs it and operation `166` reads one element. WVB 1.17 adds:

- Types kind `4`: private name, exact element shape, and `u32` length zero
  through 4,095;
- shape `22` plus a canonical kind-4 Types index;
- opcode `C5` plus a kind-4 Types index for construction; and
- opcode `C6` for exact-array plus checked-`u64` element access.

The compiler-aligned verifier accepts these forms only under minor 17 and checks
their complete nominal, count, operand, result, local, call, and control-flow
types. The scalar runner stores fixed arrays in its existing traced immutable
aggregate arena. Nested arrays, records, and variants participate in the same
type-directed mark/sweep ownership walk. The runner's 768-cell arena remains an
explicit finite consumer resource: a valid larger value may report bounded
resource exhaustion, but successful semantics, fixed length, and bounds behavior
cannot change.

Native hosted compiler packaging advances to a bounded 64 MiB product and
sixteen fragments. Construction and metadata paths keep exact checked geometry,
identity, and cleanup. No unbounded buffer or fragment inventory is admitted.

The split compiler cache advances to the version-3 analysis and emission
families. It keeps the root first and sorts already bounded dependency snapshots
by the ordinal UTF-8 bytes of their declared module identities. It does not use
filenames as semantic identities or preserve a pre-fix checkpoint.

The Language 1.0 grammar remains unchanged. Candidate freeze is reopened only
for a demonstrated language ambiguity, missing semantic distinction, or
material usability defect; an implementation inconvenience is not sufficient.

## Consequences

The deterministic three-element fixture is 375 WVB bytes with SHA-256
`e2125aba54aca71af5d10a6c7c4228460f2de28230503ad61b0b2877e8b593a7`.
The current source-built verifier accepts it and the source-built runner returns
`42`. A valid out-of-bounds mutation remains verifier-accepted and traps with
`WVR3008`; earlier-minor, excessive-count, and unknown-constructor-type mutations
are rejected before execution.

This checkpoint implements immutable fixed-array construction and read access.
It does not implement mutable slices, vectors, iteration protocols, direct native
array lowering, browser execution, Windvale OS execution, or every future
collection operation.

The complete Windows Language 1.0 front-door gate passes all 356 declared cases.
Its current analyzer is 1,077,512 WVB bytes with SHA-256
`9fa2a7a7b37329b399252eaa353a43599bd393f2c29dd1deb351b2bf1b512068`;
the packaged Windows analyzer is 33,997,312 bytes with SHA-256
`87ba2718b9f219a69f9e102045bcbb3331c37c96f1923eb605652fc9e0896e4f`.
The current emitter is 998,402 WVB bytes with SHA-256
`53b22d621cd3d169a69deb99bed0c4c5f9f1a15c11bac189076916625cef9743`;
its packaged Windows application is 21,970,432 bytes with SHA-256
`5224d55da8b201515dc7f15394cc3e7b21950a90242d2157b85d78f55241cfc1`.
The compiler-scale Generic-WIR sentinel is 1,236,227 WVB bytes with SHA-256
`37f6a8eeefb522e18685e3d96cfc9b27ee77e07698cd77f184cbb38280d59868`.

Segmented-toolset reconstruction passes four exact cases, including a 992,412
WVB-byte compiler-scale stage. Hosted packaging passes five exact Windows and
cross-target cases, including malformed-input rejection, destination
preservation, and private-scratch cleanup. The hosted-container candidate has
72 digest-bound artifacts under inventory SHA-256
`7f323dabafff6ef6c158ad1ad45c40474c60c282fda3baba3928b4d7cac8a2e4`.
The verification registry retains 108 owners and 5,114 cases under SHA-256
`c57a0d8bca9f940392a192aff978f7716cbc1356c36d0a36e3d61c280fc1674e`.

The 64 MiB publication/lifetime source cores and source bridge builds have new
exact development identities. Previously retained runtime-private WVNF
fragments remain the earlier qualified products; they are not evidence for the
expanded compiler-packaging profile and are refreshed only through their
separate paired-host artifact-qualification boundary.

## Reconsideration triggers

Reconsider the 64-item source-construction limit when a real Language 1.0
workload requires a larger literal and the compiler can retain bounded evidence
without an accidental quadratic path. Reconsider the serialized 4,095 bound only
with a versioned format change and complete verifier/runtime capacity analysis.
Reconsider the shared runner arena only with preserved type-directed collection,
deterministic failure, and resource accounting.
