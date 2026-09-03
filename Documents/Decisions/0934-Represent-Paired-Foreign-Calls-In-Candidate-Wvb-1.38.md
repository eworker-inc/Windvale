# Decision 0934: represent paired Foreign calls in candidate WVB 1.38

## Status

Accepted and implemented locally on Windows on 2026-09-03. The authenticated
production path publishes the exact registered Foreign call as candidate WVB
1.38 only after retained WVFB-to-WVIR pairing and independent emitter
revalidation. The complete verifier, runtime/provider execution, native ABI
invocation, Linux reproduction, and Slice 8 qualification remain pending.

## Context

[Decision 0932](0932-Represent-Typed-Foreign-Calls-In-Wvir-1.31.md) assigned
typed WVIR operation `190`, and
[Decision 0933](0933-Pair-Authenticated-Foreign-Calls-Before-Wvb-Emission.md)
made the production coordinator pair every such call with retained authenticated
WVFB facts. That path deliberately stopped before WVB emission.

The next boundary must preserve the selected registry, pointer, and ABI
identities in distributable bytecode without treating WVIR, WVFB, or WVB as an
authentication certificate. A raw emitter invocation must not turn possession
of non-authoritative intermediate files into native-call authority.

## Decision

1. Add candidate WVB 1.38 opcode `E0`, `foreign.call`. It is 13 bytes and
   carries three little-endian `u32` immediates: registered binding identity,
   pointer-record Types index, and ABI-enum Types index. It consumes the exact
   pointer, `u64` capacity, and `u64` expected generation and produces `i64`.
2. Admit only registered binding identity `1`, the exact
   `windvale.paper.buffer_source.sysv_amd64_c_v1` contract for native symbol
   `wv_paper_buffer_source_read_v1`: System profile, SysV AMD64 C v1,
   `ffi.call`, unsafe, no-retain, no-unwind, three by-value parameters, and one
   `i64` result. WVB contains no symbol spelling, address, library path, or
   capability grant.
3. Select minor 38 only when reachable WVIR operation `190` is emitted. Require
   its kind-`9` target, arity, zero auxiliary, pointer generic arguments, ABI,
   ordered operands, and result shape to be exact.
4. Keep the ordinary five-path `wvemit` form closed to Foreign-bearing WVSS.
   Add the private form
   `--internal-paired-foreign-source <wvss> <wvca> <wvlb> <wvir> <wvfb>
   <wvb>` for the authenticated coordinator. Require all six paths to be
   distinct and independently revalidate the WVFB/WVSD/WVIR pairing before
   prepared emission.
5. Require the coordinator to recheck its six authenticated source, target,
   catalog, evidence, lock, and profile snapshots plus WVFB immediately before
   emission and again after the private WVB is complete. Publish atomically only
   after the second check.
6. Keep the emitter-side pairing check compact and bounded within the existing
   profile-7 native type-table limit. It must not pull the general target,
   carrier, and pairing module closures into the already large emitter merely
   to repeat the narrow byte relationship it consumes.
7. Add a bounded independent WVB 1.38 reader for publication evidence and seven
   malformed mutations. Keep the complete compiler-aligned verifier and every
   execution consumer closed to minor 38 until they implement their own
   metadata, typed-stack, affine-lifetime, provider, and ABI-call rules.

## Verification

Implementation commit `e23a34060ce824c96a0b469816cb5a54ef08c8e8` passes the
21-case `language-1-production-admission-ingress` owner on the local Windows
host in 85.973 seconds. The exact Foreign fixture passes admission,
authentication, binding, typed analysis, pairing, private emission, retained-
snapshot rechecks, and atomic publication. A second authenticated build is byte
identical.

The focused reader accepts the canonical minor-38 module and rejects an old
minor, unknown opcode, unregistered binding, invalid pointer index, invalid ABI
index, ABI-as-pointer, and pointer-as-ABI. The existing compiler-aligned
`wvverify` rejects the canonical candidate, proving execution remains closed.
A post-emission carrier mutation also prevents publication.

## Consequences

The authenticated compiler front door now reaches a deterministic candidate
WVB artifact for one exact registered Foreign contract. Authentication,
semantic pairing, bytecode representation, and execution authority remain
separate boundaries.

The next checkpoint can admit `E0` to the complete compiler-aligned verifier and
prove that the consumed pointer is affine, live, generation-matched, ABI-exact,
and unable to escape. This decision does not resolve or load a native library,
form a host address, execute Foreign code, migrate a real boundary, or complete
Language 1.0.

## Reconsideration triggers

Revisit the encoding if a second registered ABI requires additional immutable
facts, if the binding registry can no longer identify one exact callable
contract, if complete verification cannot reconstruct all containment facts from
WVB, or if the private emitter check cannot remain bounded within its current
native packaging limits.
