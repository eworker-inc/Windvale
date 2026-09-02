# Decision 0920: execute contained WVB 1.37 write pointers in the scalar provider

## Status

Accepted and implemented locally on Windows on 2026-09-02. This decision opens
the bounded scalar provider and published runner to the exact compiler-verified
WVB 1.37 write-pointer derivation. It does not form a host address, lower the
operation to native x64, authenticate a Foreign call, complete Slice 8, or
claim Linux execution qualification.

## Context

[Decision 0918](0918-Verify-WVB-1.37-Write-Pointer-Lifetime-Containment.md)
admitted opcode `DF` only through the compiler-aligned verifier. The execution
provider remained closed because a verified opaque pointer still needed a
private representation, bounded allocation behavior, ownership-preserving
source emission, and deterministic teardown.

The first execution boundary should prove containment without prematurely
granting native authority. A scalar provider can carry the region descriptor
inside a nominal pointer record while keeping host addresses and Foreign calls
unrepresentable.

## Decision

1. Admit WVB minor `1.37` to the bounded scalar runner only after the embedded
   compiler-aligned verifier accepts the complete module.
2. Execute opcode `DF` by validating the exact region, pointer, and ABI nominal
   identities, allocating one bounded pointer record, and copying the region's
   private `{offset u32, length u32}` descriptor into that record. The record is
   a logical capability token and never contains a host address.
3. Permit the region source to be an owned shape-`7` local or the exact direct
   borrowed shape-`28` parameter admitted by Decision 0918. A borrowed shape is
   valid only in the parameter prefix.
4. Permit extraction of an authenticated successful write-region Result while
   transferring, rather than copying, its affine region payload. Continue to
   reject creation or forgery of that Result and every mismatched control-flow
   ownership state.
5. Require the source WVB emitter to use `local.take` when transferring an owned
   write region into its destination local. Ordinary record loads remain
   copying operations and cannot duplicate the affine region.
6. Keep pointer construction, field access, embedding, call/return escape, and
   use after move closed. Function teardown reclaims the private pointer record
   and the scratch allocation under the existing scalar heap bounds.
7. Refresh the source-built compiler verifier and runner WVB plus their Windows
   and Linux hosted containers. Windows executes the artifacts locally; Linux
   execution remains a later paired-host qualification gate.

## Implementation standing

Implementation commit
`91eb4115e1c7edd719d2614cb3299391d137cc39` publishes these exact identities:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Compiler verifier WVB | 493,060 | `a7ed497149a215526f220b0b55c2742a8f203ba1f0c57b47622c2c0d48ef90fe` |
| Windows compiler verifier | 3,983,360 | `acfdf67d0c93ef9a7c503263d573f5466d4825841007149d7c0e7e1fbfc4b9a6` |
| Linux compiler verifier | 3,985,408 | `bacd557c03dd92ebd9a11d32ae85e4c243822d2819a8b22730043b240a4b145f` |
| Scalar runner WVB | 1,020,604 | `05fd4635781f2660922760a1c96cbfd675a7a3ebb74fcd780c965db56f9b9b51` |
| Windows scalar runner | 10,368,512 | `d5743801003ac0c43ce6b5b2b3c4bb195d8334f84f5a7f84c6e1edd04b8cf7a7` |
| Linux scalar runner | 10,371,072 | `e63bce623c470418ed3bede36ce2c4c3964c245c78766e45bb71090b637e3d0b` |

The focused matrix covers 22 source cases, seven malformed write-region WVIR
cases, seven malformed pointer WVIR cases, five malformed WVB 1.36 cases, ten
malformed WVB 1.37 cases, 30 compiler-verifier decisions, five scalar runtime
cases, and one pre-execution opcode mutation. The successful case returns `42`
with contained pointer allocation and bounded teardown.

## Consequences

- WVB 1.37 pointer derivation is executable in the reference scalar provider,
  while the opaque pointer still cannot reveal or carry a native address.
- Affine write-region extraction and compiler emission now agree: ownership is
  transferred through `local.take` instead of copied by an ordinary load.
- The public `wvverify` command understands the current compiler's WVB 1.37
  output and accepts compiler-scale modules.
- Native opcode `DF` lowering and one authenticated no-retain Foreign call are
  the next compiler boundary. Required Libraries 1.0 profiles and paired-host
  qualification remain pending.

## Reconsideration triggers

Reconsider the private pointer record before native lowering if a real Foreign
ABI requires more than the authenticated region extent and ABI identity, or if
multiple simultaneously live derived pointers require a stronger verifier
model. Any replacement must keep host addresses private, preserve affine
non-escape, reject malformed input before allocation, and retain bounded
teardown.
