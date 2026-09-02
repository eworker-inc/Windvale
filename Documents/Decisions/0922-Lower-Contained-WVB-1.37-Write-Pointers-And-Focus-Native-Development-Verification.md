# Decision 0922: lower contained WVB 1.37 write pointers and focus native development verification

## Status

Accepted and implemented locally on Windows on 2026-09-02. This decision opens
native x86-64 execution only for the already compiler-confined logical pointer
descriptor and replaces unrelated lowerer test fan-out with one current-source
development owner. It does not authorize a Foreign call, form a host address,
replace fresh release qualification, or claim Linux execution.

## Context

[Decision 0920](0920-Execute-Contained-WVB-1.37-Write-Pointers-In-The-Scalar-Provider.md)
opened bounded scalar execution for opcode `DF`; native x86-64 lowering remained
closed. At the same time, one edit to a normal native-lowerer source selected 21
owners, including database, model, and reconstruction suites that consumed
retained generated artifacts instead of the changed source. Those suites could
take hours without being able to reveal a defect in the edit.

Windvale compilation itself was not the hours-scale operation. The complete
648-function lowerer compiled in about 31 through 35 seconds on the measured
Windows host. Development verification therefore needs to build that current
source once and spend the remaining time on bounded behavior owned by the
lowerer. Fresh reconstruction and paired-host work remain separate qualification
products.

## Decision

1. Admit WVB 1.37 to the native x86-64 lowerer only after its structure,
   metadata, types, functions, control flow, and affine ownership all validate.
2. Lower opcode `DF` to a metered 45-byte sequence that copies the private
   packed logical `{start u32, length u32}` region descriptor into pointer-owned
   record backing. Never materialize or expose a host address.
3. Revalidate exact generated region and pointer token identities, their
   distinct nominal types, the ABI enum, local indices, record storage, and a
   maximum of 4,096 pointer derivations per module.
4. Permit only the compiler-generated move between two distinct exact pointer
   locals. Reject pointer `local.take`, direct record construction, copying,
   call or return escape, use after move, and ownership-state disagreement.
5. Add one development owner that builds the complete current lowerer, packages
   it through the content-addressed development cache, compares exact inherited
   Return-42 and metadata WVO bytes, executes one contained pointer program with
   native result `42`, and rejects ten independent malformed cases.
6. Route normal native-lowerer implementation and tool-root edits to that
   current-source owner. Keep staging-only edits on segmented reconstruction and
   keep the complete project-manifest boundary on its downstream reconstruction
   closure.
7. Do not select database, model-provider, application, or retained-artifact
   owners merely because they were historically produced by a lowerer. Select
   them for their own sources or contracts and at deliberate final gates.
8. Preserve fresh deterministic reconstruction and Windows/Linux qualification
   without using the development result cache as release evidence.

## Measured result

Implementation commit
`4537c4ca132d1ad9b381238d750b6152230b015d` contains the native lowering,
focused owner, routing, fixtures, and accepted contract updates.

On the Windows x64 development host, the current 717,047-byte, 648-function
lowerer WVB compiled in 31 through 35 seconds. Cold current-host packaging took
about 93 through 100 seconds; an exact warm package hit reduced the complete
13-case development owner to about 32 seconds. The broader pointer matrix using
the already built lowerer completed in 19.4 seconds with five native lowering
and execution cases. Verification-plan self-tests passed 31 general and 260
native routing cases.

The machine-readable evidence is
[`2026-09-02-WVB-1-37-Native-Pointer-And-Focused-Verification.json`](../Evidence/2026-09-02-WVB-1-37-Native-Pointer-And-Focused-Verification.json).

## Consequences

- Candidate WVB 1.37 pointer derivation now executes in both the bounded scalar
  provider and native x86-64 code while remaining a logical, nonescaping token.
- A normal lowerer edit gets current-source feedback in tens of seconds instead
  of launching unrelated hours-scale suites.
- The development owner also protects inherited baseline and metadata byte
  identity; it is not only a pointer-specific compile smoke.
- Cold packaging is still materially slower than mature toolchains and remains
  an explicit profiling target rather than hidden repeated work.
- Authenticated no-retain Foreign lowering, one real migrated boundary, Linux
  execution, required Libraries 1.0 profiles, and final qualification remain
  pending.

## Reconsideration triggers

Reconsider the native pointer representation before a Foreign thunk if the ABI
needs authority beyond the authenticated region extent and ABI identity.
Reconsider development routing if the focused owner ceases to build the exact
changed source, a lowerer-owned behavior lacks a representative case, or a
qualification path consumes a cached development result.
