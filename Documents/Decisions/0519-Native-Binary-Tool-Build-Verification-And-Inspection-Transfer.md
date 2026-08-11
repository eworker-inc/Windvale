# Decision 0519: Native binary-tool build, verification, and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

Decision 0518 transferred the final source-compiler-phase builds and core
inspections. The next contiguous construction block in both broad Seed scripts
built four complete binary tools through the managed CLI, verified each WVB,
and inspected its profile and ownership surface before exercising the tool:

- the Windvale-written WVB inspector (`wvdump`);
- the WVO 1.0 read-only object verifier and inspector;
- the complete WVA 1 assembler; and
- the standard flat-image Wv linker.

All four products already have explicit Project 1 manifests and qualified
native construction dependencies. Their capability-bearing executions,
malformed-input behavior, assembler and linker artifact publication, and
Stage 0 differential/oracle comparisons are separate behavior contracts. This
slice can transfer construction, independent verification, and inspection
without claiming those execution boundaries.

The former broad-script source lists for the WVO and linker products had also
become stale. The WVO list omitted `Wvo-Object-Verification.wv`; the linker list
omitted `Foundation/Sha256.wv`. Repeating those lists through Stage 0 no longer
constructed the current products. The checked-in Project manifests are the
authoritative complete closures.

## Decision

Make the paired `Verify-Seed-Native-Front-Door` helpers build, independently
verify, and natively inspect these exact Project 1 products:

| Product | Functions | Code bytes | Module bytes | SHA-256 |
| --- | ---: | ---: | ---: | --- |
| WVB inspector | 39 | 59,277 | 76,527 | `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753` |
| WVO object core | 42 | 51,298 | 61,008 | `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db` |
| WVA assembler | 101 | 145,748 | 180,071 | `a50e261fb690b1b2836b7b05da2d94ec7f023ef531ddd2432fc6a9001ae7049c` |
| Wv linker | 96 | 112,099 | 135,740 | `02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874` |

The independent verifier must accept each exact product before inspection. The
inspection contract binds these serialized headers and ownership surfaces:

| Product | Profile | Capability section | Export section | Type section | Required ownership surface |
| --- | --- | --- | --- | --- | --- |
| WVB inspector | hosted | offset 48, bytes 145, count 5 | offset 75,635, bytes 17, count 1 | offset 75,660, bytes 867, count 5 | `Main`, five hosted capabilities, WVB inspection types |
| WVO object core | hosted | offset 51, bytes 145, count 5 | offset 59,468, bytes 17, count 1 | offset 59,493, bytes 1,515, count 13 | read-only object verification/inspection and SHA-256 helper |
| WVA assembler | hosted | offset 54, bytes 172, count 6 | offset 177,876, bytes 17, count 1 | offset 177,901, bytes 2,170, count 19 | WVA scanning, semantic analysis, WVO construction, and hosted publication |
| Wv linker | hosted | offset 50, bytes 172, count 6 | offset 133,297, bytes 17, count 1 | offset 133,322, bytes 2,418, count 20 | object admission, resolution/layout, image reconstruction, SHA-256, map construction, and hosted publication |

The WVO inspection additionally rejects any `file.write_bytes` capability. Its
current flattened SHA helper is serialized as
`name="__WvM2F0" parameters=1 result=bytes`; the former managed assertion
`__WvM2F0(bytes) -> bytes` was a stale rendering, not a bytecode contract. The
linker's explicit source SHA closure similarly serializes its flattened helper
as `name="__WvM5F0" parameters=1 result=bytes locals=903`; the obsolete
`bytes.sha256_hex` opcode assertion cannot describe the current product.

Changed-file routing now selects the native front-door helper for each of the
four source closures and manifests. A change to a product cannot silently use a
managed or unfiltered fallback to satisfy this construction boundary.

The broad scripts retain every tool execution and behavior comparison after
the transferred block. In particular, they still own capability refusal,
self-tests, hosted valid and malformed inputs, WVO sample creation and
read-only reports, assembler output and preservation, linker output and map
construction, and the Stage 0 assembly/object/link differential lane.

## Evidence

The current Windows helper was first run over the unchanged 156-case prefix and
the new tail. In 953.2 seconds it passed the complete prefix, all three WVB
inspector cases, and WVO build and verification before exposing only the stale
WVO textual signature assertion described above. After that assertion was
corrected, a fresh four-product tail ran in 48.6 seconds: WVB inspector, WVO,
and WVA build/verify/inspect passed; linker build and verification passed; and
the stale linker SHA assertion was isolated. The corrected exact linker
inspection then passed in 2.9 seconds. Together these narrow reruns exercise all
101 artifacts and 168 current helper cases; they are deliberately not reported
as one uninterrupted passing invocation.

Focused retained behavior also passed:

- the complete Windvale-written `wvdump` agrees across interpreter, JIT, and
  WVO AOT, 1/1 in 4.862 test seconds;
- the digest-bound native WVO launchers preserve read-only reports, 1/1 in
  0.467 test seconds after synchronizing the test with the authoritative
  candidate-2 manifest;
- all eleven native WVA rejection families preserve existing output, 1/1 in
  3.435 test seconds on the clean rerun; and
- the digest-bound native WVO linker launcher preserves canonical output, 1/1
  in 31.204 test seconds.

Two retained current-writer comparisons expose pre-existing drift rather than
failures in the transferred products. The managed current WVO application
writer emits 606,720 bytes while the pinned, internally consistent candidate-2
applications remain 606,208 bytes. The managed current WVA application writer
also no longer reproduces the pinned Windows application digest, while the
180,071-byte WVB and its digest remain exact. Neither native candidate is
repinned from one-host evidence; current-writer reconciliation and independent
dual-host evidence remain explicit follow-on work.

This removes four managed builds, four managed verifications, and four managed
inspections from each broad host script: twelve calls in this change and 174
cumulatively across Decisions 0505, 0506, 0508 through 0519. It removes no
direct managed entry file. The inventory remains three normal direct files plus
nine recovery files, and T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 97 to 101 exact artifacts and from 156 to
168 owned cases. Construction, independent WVB verification, and deterministic
inspection of all four canonical binary tools no longer load .NET in either
permanent-host broad script. Project manifests, rather than duplicated manual
source inventories, own their exact current closures.

The transferred checks establish product identity and ownership surfaces only.
They do not transfer capability-bearing execution, mutation or publication,
malformed-input coverage, Stage 0 differential/oracle behavior, the broad test
harness, independent Linux execution, clean or previous-seed bootstrap, grouped
qualification, artifact promotion, or recovery deletion.

## Reconsideration triggers

Continue with the immediately following WVB-inspector, WVO, assembler, and
linker execution block. Reconcile the two retained current-writer drifts before
any candidate repin, then replace managed behavior only where a digest-bound
native owner preserves exact success, rejection, and existing-output contracts.
