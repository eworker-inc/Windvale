# Decision 0072: Final pure runtime native services

- Date: 2026-08-01
- Status: Implemented; cross-host qualification pending
- Extends: [Decision 0071](0071-Native-Text-Arena-And-Core-Text-Services.md)'s exact runtime-native service pattern
- Retains: Native ABI 11, execution-context version 3, service-table version 4, kernel native bridge 6, and firmware probe 13

## Context

Decision 0071 gives allocation-bearing runtime services one explicit execution-owned text arena and moves concatenation plus signed and unsigned integer formatting into exact platform-neutral x86-64 leaves. Strict UTF-8 validation is already native under Decision 0070. Only deterministic enum-name lookup and text quoting still cross managed callback and platform-thunk boundaries.

Enum naming needs the verified nominal type directory at run time. That metadata must remain bounded, deterministic, independently checked, and separate from portable WVB/WVO identity because a process address cannot be serialized. Quoting must preserve Windvale's current deterministic Foundation semantics, reject malformed UTF-8, calculate its full result before allocation, and share the same arena and failure details as the existing native text services.

Neither migration changes a generated-code call shape, context field, service-table slot, portable format, or service-free OS consumer. Advancing an ABI or firmware version would therefore create version churn without a contract change.

## Decision

- Keep native ABI 11, context version 3, service-table version 4, WVB 1.6, WVO 1.0, kernel native bridge 6, and firmware probe 13 unchanged.
- Replace the managed/platform-adapter implementation of `Enumˉname` with one exact 323-byte platform-neutral x86-64 leaf. Its SHA-256 is `fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a`.
- Append one canonical runtime-private `WVEN` version-1 metadata block immediately after that leaf. The leaf reaches it position-independently. The service table points to the leaf, and the complete code-plus-metadata bundle is published read/execute only.
- Bound one `WVEN` block to 32 MiB. Its 24-byte header contains magic, version, total byte length, nominal type count, enum member count, and directory offset. Each nominal type has an 8-byte first-member/count directory entry; records have count zero. Each enum member has a 16-byte signed-value/name-offset/name-length/reserved entry followed by concatenated strict-UTF-8 names. The reserved field is zero.
- Derive `WVEN` only from the fragment's independently verified canonical nominal declarations. Before W^X publication, independently parse and validate every bound, count, directory entry, signed value, name range, reserved field, and strict-UTF-8 name, then require deterministic reconstruction and complete byte equality.
- Replace the managed/platform-adapter implementation of `Textˉquote` with one exact 1,165-byte platform-neutral x86-64 leaf. Its SHA-256 is `4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7`.
- Make quoting a two-pass operation. The first pass strictly validates UTF-8, decodes Unicode scalar values, measures the specified UTF-16-code-unit escape result, and proves both the 1 MiB value limit and the 16 MiB arena capacity before changing the cursor. The second pass writes the exact bytes.
- Preserve the Foundation quote contract: quote and reverse solidus use short escapes; backspace, form feed, line feed, carriage return, and tab use their short escapes; other ASCII controls use uppercase `\u00XX`; printable ASCII is preserved; other BMP scalars use uppercase `\uXXXX`; and supplementary scalars use two uppercase UTF-16 surrogate escapes.
- Require both leaves to preserve `R10`, `R11`, and `R15`, accept only compiler-generated verified inputs/output cells, share the context-owned text arena, and publish the existing exact failure details for value-limit and arena exhaustion.
- Remove their managed callback delegates, platform thunks, and allocation/quote helpers. Keep managed reference semantics as the differential oracle.
- Keep the five hosted/capability adapters managed for this slice: console output, diagnostic output, argument count, argument text, and file-byte input.
- Do not describe the result as a Windvale-written native runtime or .NET retirement. C# still constructs and verifies service bundles, publishes W^X memory, owns arenas and execution, supplies hosted adapters, and remains the reference/recovery implementation.

## Candidate evidence

The existing native dynamic-text test now reconstructs and verifies all five services supplied by the native text-service builder, exercises enum lookup across multiple nominal types, covers the full deterministic quote escape families including supplementary Unicode, checks exact `WVEN` structure, rejects corrupt leaf and metadata bytes, and proves separate value-limit and arena-exhaustion failures. Its final focused Windows Release pass takes 0.429 seconds. The existing complete Windvale-written `wvdump` case also passes without a new full-suite test. The pre-commit Windows Standard gate builds with zero warnings and passes all 56 tests in 209.841 suite seconds; its warm dynamic-text and complete-`wvdump` cases take 0.176 and 0.466 seconds. All 15 OS tests pass.

Cross-host Qualification, portable-artifact comparison, all OS tests, pinned-QEMU confirmation, and GitHub verification remain required before this decision becomes qualified.

## Consequences

All six deterministic pure runtime services now have exact platform-neutral native implementations: UTF-8 validation, enum naming, concatenation, quoting, signed integer formatting, and unsigned integer formatting. None requires a managed callback or a Windows/System V adapter during execution.

The ABI and portable formats remain stable. Enum metadata is execution-support material reconstructed from already verified fragment types; it does not add process-specific pointers to WVB or WVO and does not make service-bearing WVO images independently loadable.

The next .NET-retirement work is no longer another pure service leaf. It is the more consequential boundary around hosted capabilities, standalone executable/container metadata, runtime ownership, and eventually Windvale-written construction and publication of the execution stack.

## Reconsider when

- A standalone PE, ELF, or Windvale-native container must carry and independently verify service and nominal metadata without the original `Nativeˉfragment`.
- The language adopts scalar-value rather than UTF-16-code-unit quote semantics.
- Enum reflection expands beyond exact declared-name lookup or requires a more general runtime type-information format.
- A Windvale-written service builder can replace the Stage 0 exact-byte construction while retaining deterministic identity and recovery provenance.
