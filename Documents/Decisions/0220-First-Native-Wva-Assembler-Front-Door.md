# Decision 0220: First native WVA assembler front door

- Date: 2026-08-05
- Status: Source-qualified at `3aa5ba2`; pinned artifact/front-door commit requires its own dual-host gate
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Contract: [Windvale native WVA assembler](../../Specifications/Windvale-Native-Wva-Assembler.md)

## Context

Windvale already owns a complete WVA scanner, semantic validator, object measurer, encoder, hosted shell, and differential suite. Normal assembly still entered the C# CLI, so the product behavior existed in Windvale while its ordinary executable front door remained a .NET bootstrap dependency.

The existing compiler-authority Windows and Linux packages already provide the required process entry, bounded arguments, input snapshot, exact output service, diagnostics, and independent PE/ELF verification. Creating another startup would add architecture-specific source without adding Windvale ownership.

During native execution, a canonical assembler fixture exposed a general descriptor-return bug in the x64 backend: the returned start offset in `RAX` was overwritten while reloading the call watermark, forcing an overlapping forward copy for a value that crossed the watermark. This repeated the first byte of the WVO. The bug affected general immutable-byte helper returns, not WVA parsing or encoding.

## Decision

### Make the assembler an explicit Windvale project

Add `Windvale-Wva-Assembler.wvproj` with `Assembler/Windvale/Wva-Assembler-Core.wv` as root and the four exact Foundation dependencies used by the implementation. Add the nominal zero enum member required by native lowering without changing the existing serialized values of the accepted token kinds.

### Correct descriptor-return ownership once

Preserve the already-loaded call watermark in `R9` while checking a returned descriptor's start/end range. Do not reload it through `EAX`, because that aliases and destroys the return start in `RAX`. Update the independent fragment decoder for the ten-byte-shorter sequence and add a focused helper-return regression that constructs `WVO1`, one-byte slices, and a little-endian scalar across later allocations.

This is a general native-backend correction. No assembler-specific copy workaround is permitted.

Because the corrected sequence is ten bytes shorter in every affected descriptor-returning function, current AOT reconstruction identities also change for existing native tool profiles. Previously qualified or explicitly pinned front-door binaries remain byte-for-byte unchanged and keep their existing launchers and digests. They are historical artifacts built from the commits recorded in the native-front-door manifest, not outputs that an evolving backend must silently overwrite.

The current corrected-backend reconstruction candidates are:

| Profile | Windows bytes / SHA-256 | Linux bytes / SHA-256 |
| --- | --- | --- |
| Exact compiler | 27,547,136 / `10d859929f05c0fae6eb747b52720fdfd153f25a6fca8b83ea6e7f7f4744c88b` | 27,549,696 / `b2dc418a95c79802e2804433f77f779f17ca40033840e02e28417430df21178d` |
| Build driver | 28,917,248 / `9d6de1b4b9fd6c9359357c70e282add08ab895f18fcf8712a14010ab6a7c13b9` | 28,917,760 / `2e6480adbccdffbe75ac5442bd1e2e841770274766c7392061131fb831a91999` |
| WVB publisher | 1,119,232 / `7fa123d48a208765c7bdd5994f9f96a58e9becda817caa8769491f49e4616967` | 1,119,173 / `224821ca193fd9d95b9e7d2602c337ee4ae42e7d9eb867e0842e2ceceadb1ad6` |
| WVB verifier | 1,004,032 / `aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a` | 1,003,520 / `26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b` |
| WVB inspector | 793,600 / `31b958fa446e7b4776ba1db0469a6c9ab32c53d960f55a476a6a202cd322194c` | 794,624 / `cc87e9b7dc9bd74d5e14ab079c94cec9e77669953e301d9d32c06c3cefff9f9e` |
| WVB runner | 778,240 / `6231a60404fc49f85695eddcc2e0690e372c64c0cf2d2ca847fd0ffc3f76b028` | 778,240 / `74180ac7cd80192647f46df166a8ea97af17c9676afbe0b2ecb2c8c824db6944` |

Focused package tests bind current reconstruction identities. The native-front-door inventory separately binds the retained qualified or pinned bytes to their producing commits. Promotion, repinning, and launcher-digest changes happen only after dual-host evidence, never as an incidental consequence of building a later source tree.

### Add one fixed native assembler profile

Add `windows-x64-wva-assembler-v1` and `linux-x64-wva-assembler-v1` under distinct `WVHS 1` metadata and outer container format 6. Require the canonical module identity, one exported `Main`, the exact six capabilities, and the exact nine fragment services specified by the contract.

Reuse the existing compiler-authority startup and runtime layout. Retain its internal `enum.name` service slot so the startup and service table do not need a new WVA assembly variant. No WVA scanning, diagnostics, instruction encoding, object construction, or other assembler semantics may enter platform assembly.

### Keep qualification and retirement separate

The Stage 0 CLI can construct both candidates and remains the normal `windvale assemble` path for now. A focused test reconstructs both applications, exercises the current-host raw executable, compares canonical output byte for byte with the frozen oracle, rejects malformed input without output, and inspects loaded modules or mappings for .NET.

After one exact commit passes that evidence on Windows and Linux, pin both platform applications and add digest-bound native launchers in a provenance commit. Only after that second exact artifact commit passes both hosts does ordinary assembly move to the native launcher. The C# assembler then remains reachable solely through named recovery and differential paths until the complete Decision 0057 archive gate permits deletion.

Exact source commit `3aa5ba27f0ae4f96bc80d8bd521363015e884ab3` passes GitHub [Verify run 31004212797](https://github.com/eworker-inc/Windvale/actions/runs/31004212797) on Windows and Linux. The provenance slice pins its exact WVB, PE, and ELF identities in `Artifacts/Native-Front-Door`, adds digest-bound `Tools/Native/Assemble-Wva.cmd` and `.sh` launchers, and exercises the current-host launcher with accepted and rejected WVA. Those launchers become the ordinary front door only when the exact commit containing the pinned artifacts passes the same dual-host gate; the commit's GitHub status is the promotion evidence and avoids embedding a self-referential commit identity in its own bytes.

## Consequences

- WVA parsing, validation, encoding, and WVO construction remain one Windvale implementation.
- The additional machine-specific code is zero for this profile; it reuses an existing startup and service boundary.
- The native backend now preserves immutable byte descriptors returned across helper calls and later allocation.
- PE/ELF candidate construction still uses Stage 0; this decision does not claim complete .NET retirement or delete recovery source.
- Normal post-cutover tests can use stable WVA/WVO vectors and independent WVO verification. A live C# comparison remains useful only in the bounded differential lane and is not a permanent requirement.

## Reconsideration triggers

Reconsider this profile if the assembler requires a capability outside the exact six-entry set, if the shared startup prevents a narrower future binding, if WVA or WVO changes require a versioned contract, or if dual-host evidence changes any candidate identity.
