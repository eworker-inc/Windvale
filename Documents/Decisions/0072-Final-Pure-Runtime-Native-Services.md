# Decision 0072: Final pure runtime native services

- Date: 2026-08-01
- Status: Qualified on Windows and Debian x64
- Extends: [Decision 0071](0071-Native-Text-Arena-And-Core-Text-Services.md)'s exact runtime-native service pattern
- Retains: Native ABI 11, execution-context version 3, service-table version 4, kernel native bridge 6, and firmware probe 13
- Advanced by: [Decision 0358](0358-Windvale-Owned-Native-Text-Quote-Leaf.md), [Decision 0359](0359-Windvale-Owned-Native-Enum-Name-Leaf.md), [Decision 0361](0361-Windvale-Owned-Bounded-Native-Enum-Metadata.md), [Decision 0362](0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md), [Decision 0363](0363-Direct-Native-Enum-Name-Leaf-Consumption.md), and [Decision 0364](0364-Direct-Fixed-Native-Service-Leaf-Consumption.md)

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

## Qualification evidence

The existing native dynamic-text test now reconstructs and verifies all five services supplied by the native text-service builder, exercises enum lookup across multiple nominal types, covers the full deterministic quote escape families including supplementary Unicode, checks exact `WVEN` structure, rejects corrupt leaf and metadata bytes, and proves separate value-limit and arena-exhaustion failures. Its final focused Windows Release pass takes 0.429 seconds. The existing complete Windvale-written `wvdump` case also passes without a new full-suite test. The pre-commit Windows Standard gate builds with zero warnings and passes all 56 tests in 209.841 suite seconds; its warm dynamic-text and complete-`wvdump` cases take 0.176 and 0.466 seconds.

Exact commit `f97d22154d1c7562f72552a617dd912d18943880`, tree `f79cf95eb5d53b005bbd5f1b67733ba7648ce98d`, was archived as 2,893,521 bytes with SHA-256 `992235ac005c9f953a3d43a2a101f44693729c9ccd606ac5540c5c515bff66d8`. The digest and size matched after transfer to the isolated Debian GNU/Linux 12 x64 QA host. Windows and Debian with .NET SDK `10.0.302` build Release with zero warnings, pass all 56 tests and exact compiler reproduction, and complete the CLI verifier. Their suite times are 220.741 and 233.171 seconds; their native dynamic-text cases take 0.184 and 0.208 seconds.

The 15,563-byte Windows conformance report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 11,917-byte timing report has SHA-256 `f3bd9d9f47b8ae75cd1f1eaa7e7ed64d3040561f616254653d00c44ae4d3445b`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,522-byte timing report has SHA-256 `64fc010062ca258a394ac584e94e3b68e675233a1cc3431823521ba5d3fa7c5a`. Their normalized contracts match exactly.

All 61 portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The directly retrieved 2,299,000-byte Debian evidence bundle has SHA-256 `489702b53e1ffb8f2630d8c33c9d1fe43897f7ed3e4467be29f3ddf725eafc1e`. Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG boots the unchanged exact 15,872-byte firmware-probe-13 image with SHA-256 `ceffc3e33bf007e47b109f3b6a71db2fdceac3c0e908d1471f056909ee42532d`, emits the complete success marker, and returns guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

GitHub [Verify run 30696841988](https://github.com/eworker-inc/Windvale/actions/runs/30696841988) passes for the exact candidate. After evidence retrieval and comparison, the resolved QA directory, transferred archive, remote evidence bundle, and temporary QA inputs were removed and confirmed absent.

## Consequences

All six deterministic pure runtime services now have exact platform-neutral native implementations: UTF-8 validation, enum naming, concatenation, quoting, signed integer formatting, and unsigned integer formatting. None requires a managed callback or a Windows/System V adapter during execution.

The ABI and portable formats remain stable. Enum metadata is execution-support material reconstructed from already verified fragment types; it does not add process-specific pointers to WVB or WVO and does not make service-bearing WVO images independently loadable.

The next .NET-retirement work is no longer another pure service leaf. It is the more consequential boundary around hosted capabilities, standalone executable/container metadata, runtime ownership, and eventually Windvale-written construction and publication of the execution stack.

## Reconsider when

- A standalone PE, ELF, or Windvale-native container must carry and independently verify service and nominal metadata without the original `Nativeˉfragment`.
- The language adopts scalar-value rather than UTF-16-code-unit quote semantics.
- Enum reflection expands beyond exact declared-name lookup or requires a more general runtime type-information format.
- A Windvale-written service builder can replace the Stage 0 exact-byte construction while retaining deterministic identity and recovery provenance.
