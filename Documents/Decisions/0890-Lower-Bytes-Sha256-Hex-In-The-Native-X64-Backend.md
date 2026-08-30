# Decision 0890: lower bytes.sha256_hex in the native x64 backend

## Status

Accepted implementation candidate with registered Windows and local Linux
development evidence on 2026-08-30. The exact-current Windows owner passes all
eight cases in 185.1 seconds, and the same registered owner passes all eight in
234,000 milliseconds on local Debian 13.5 under WSL. Paired-host CI
qualification and promoted artifact identities remain pending.

## Context

[Decision 0829](0829-Keep-Profile-Admission-Sha256-Native-Lowerable.md)
corrected a real compiler-product regression while the native x64 backend did
not lower WVB opcode `0x7D`, `bytes.sha256_hex`. It kept affected compiler
products on the portable Foundation implementation and named native lowering
as its first reconsideration trigger. Later admission and foreign-catalog
checkpoints correctly retained that source-owned route rather than hiding the
unsupported opcode or widening an execution arena.

The backend now has a small bounded implementation path. The intrinsic already
has stable language and WVB semantics: it hashes one immutable byte sequence or
slice of at most 4 MiB and returns exactly 64 lowercase ASCII hexadecimal
characters as `text`. The remaining choice is how to include one exact native
implementation without cloning it per call, changing WVO 1.0, or disturbing
objects that do not use the opcode.

## Decision

1. Admit opcode `0x7D` in the native x64 descriptor-operation family. Require
   one `bytes` operand and produce one `text` result. Preserve the existing
   4,194,304-byte input ceiling and exact 64-byte lowercase-ASCII output.
2. Emit the ordinary ten-byte instruction-budget charge plus one exact
   152-byte raw wrapper per occurrence. The wrapper reserves the complete
   64-byte result before publication, calls the private helper once, and uses
   the existing runtime-failure path when the arena cannot satisfy that exact
   reservation.
3. Append one exact 1,640-byte relocation-free helper per object when and only
   when at least one admitted function uses the opcode. Its fixed layout is
   1,350 instruction bytes, two zero alignment bytes, 32 initial-state bytes,
   and a 256-byte round table. Publish the existing local-function symbol
   `$native_sha256_hex` at the exact end of declared function code.
4. Keep WVO 1.0 unchanged. The wrapper and helper add no public symbol kind,
   section kind, relocation kind, platform import, or format version. Static
   data relocations remain bounded to declared function code; every wrapper
   call and helper-internal data reference is resolved directly.
5. Treat the optional helper as the final ordinary code region for bounded
   publication and staging. The staged symbol reader must prove its unique
   name, size, and contiguity; the per-text-chunk validator must compare every
   covered helper byte with the canonical suffix and reject corruption before
   linking. Text padding follows the helper.
6. Preserve exact SHA-free WVO identity. A plan with no opcode `0x7D` carries
   no helper or helper symbol and must reproduce every prior object byte,
   symbol, relocation, and padding decision unchanged.
7. Do not regenerate retained artifact pins merely to exercise the candidate.
   Temporary reconstructed tools own focused evidence. Promotion may update
   only the changed lowerer, WVO staging-producer, and compiler-image-staging
   identities after Windows and Linux agree; the transport identities and
   SHA-free fixed-vector identities must remain exact.
8. Do not automatically replace the Foundation SHA-256 implementation in an
   existing compiler product. Each consumer migration still requires a named
   source change, retained-memory measurement, reconstruction, and its own
   evidence. Host hashing services and caller-supplied digests remain
   prohibited substitutes for the semantic intrinsic.

This decision reconsiders and supersedes only Decision 0829's current-direction
requirement to retain Foundation SHA-256 because native publication could not
lower opcode `0x7D`, together with that correction's prohibition on expanding
native opcode support. It does not rewrite or invalidate Decision 0829's
historical context, artifact identities, evidence, or the correctness of its
then-required restored project closures. Its explicit-final-target and
per-consumer verification rules remain applicable.

## Initial evidence

The implementation candidate adds the one-byte opcode to the native descriptor
classifier, canonical instruction-width table, typed value analysis, emitted
byte accounting, descriptor emitter, and independent record-storage scanner.
It adds the optional helper to complete-object and segmented publication and
extends staged symbol, relocation-range, helper-content, and native status
validation. Every project that compiles one of those consumers carries the
helper source in its explicit closure.

A read-only completeness audit found no additional opcode-classifier,
instruction-width, typed-stack, record-provenance, project-closure, or staged
consumer omission after the record-storage correction. The registered owner
reconstructs the lowerer from current Windvale source and passes all eight
cases on exact-current Windows in 185.1 seconds and on local Debian 13.5 under
WSL in 234,000 milliseconds. Both runs publish the exact summary
`native SHA-256 lowering status=Passed cases=8 kats=2 arena=64/63
helper-bytes=1640 sha-free=Identical staged-corruption=Rejected`. They prove
byte-identical SHA-free Return-42 WVO;
one oracle-exact 1,640-byte local helper with every wrapper call targeting it;
exact empty-input and `abc` native known answers, including a `bytes` parameter
and an owned `text` result returned across a helper boundary; segmented
publication plus coalesced-helper staging acceptance and same-length corruption
rejection; the wrapper's atomic-allocation and detail-2 machine contract; exact
64-byte arena success with exit 42; and exact 63-byte atomic failure with exit
1 and detail 2.

Node syntax checking, the harness checks, diff checks, and the
verification-plan suite pass. The plan suite covers 31 general and 224 native
routing cases. The registered native inventory contains 122 owners and 5,849
cases in 21,412 LF-only bytes with SHA-256
`79c2549aa0d6ab4f123ba33b61033af6c90a6c71c2f1c0d06fd4158ac204fb87`.
The temporary Node runtime used for the local Linux run was removed afterward;
it is not a retained toolchain dependency.

These are registered Windows and local Debian/WSL development results, not a
paired-host CI qualification claim. A final candidate pin, tracked artifact
promotion, the paired CI gate, and consumer migration are not recorded by this
decision.

## Consequences

Compiler products may now choose the semantic intrinsic without being
intrinsically unstageable by the implemented source candidate. Objects that use
it gain 162 declared-function bytes per occurrence and one 1,640-byte private
suffix per object; repeated calls do not repeat the helper. Staging performs an
exact content check instead of treating manifest geometry as helper
authentication.

The retained admission and WVFC producer generations remain valid at their
recorded Foundation-based identities. They do not migrate until their owners
make and measure that separate choice. The current candidate is registered as
a changed-file owner but is not promoted, does not complete the native backend,
and does not establish paired CI qualification.

## Reconsideration triggers

Reconsider the private suffix when a shared native object owner can express the
same helper with byte-identical ABI, metering, failure, and SHA-free behavior;
when another accepted backend needs a common helper representation; when the
4 MiB value or 64 MiB object bounds change; when measured compiler products
show that this exact helper materially harms retained memory or staging; or
when Windows and Linux evidence differs in bytes, linking, execution, or
failure behavior. Any replacement must retain a simple exact SHA-256 oracle and
staged corruption rejection.
