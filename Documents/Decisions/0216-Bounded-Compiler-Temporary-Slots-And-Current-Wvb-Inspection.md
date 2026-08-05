# Decision 0216: Bounded compiler temporary slots and current WVB inspection

- Date: 2026-08-05
- Status: Implemented; dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0215](0215-Native-Wvb-Verify-And-Inspect-Front-Door.md)
- Contracts: [WVB inspector core](../../Specifications/Wv-Dump-Core.md) and [WVB report](../../Specifications/Wv-Dump-Report.md)

## Context

The next retirement item is native execution of a verified portable `Main() -> i32` WVB. Windvale already has a bounded scalar interpreter written in Windvale source, but composing it behind a small hosted runner exposed two independent prerequisites.

First, the Windvale compiler assigned one WVB local to every WVIR temporary. The interpreter has only 331 named source declarations but produced 4,613 compiler-created temporary slots, exceeding the source compiler's 4,096-slot admission bound and the native backend's 2,048-frame-slot bound. Copying the interpreter into C#, C, or assembly would recreate the duplicate semantics this retirement is intended to remove.

Second, the qualified native `wvdump` candidate decoded only the earlier implemented opcode and value-shape surface. It structurally rejected the valid runner at opcode `0xA1` (`u32.divide`), even though the compiler-aligned verifier and Stage 0 inspector accepted the module.

## Decision

### Reuse temporary slots inside the Windvale compiler

Add a focused Windvale module that computes deterministic temporary lifetimes and greedily reuses a physical slot only when the earlier value's last use does not cross the later definition and both slots have the same shape. Phi-result slots remain reserved because their edge semantics require a conservative lifetime. The WVB emitter records the physical slot mapping and emits the corresponding local shapes.

Preserve byte-for-byte Stage 0 output for the already admitted compiler surface: the identity mapping remains in use while parameters, named locals, and one slot per temporary fit the existing 4,096-slot source-compiler bound. Lifetime allocation activates only when that historical layout would be rejected. This avoids adding the allocator to C# merely to keep ordinary golden output aligned.

The composed runner now contains four functions. Its internalized Windvale interpreter uses 861 locals, 71,221 code bytes, and a maximum operand stack of three. The resulting 74,286-byte WVB is accepted by the native compiler-aligned verifier. This is implementation evidence for the prerequisite, not yet a distributed runner or a native execution qualification claim.

The slot allocator changes the canonical compiler closure, so its source project, pinned WVB, and existing platform launchers advance together:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Compiler build-driver WVB | 1,078,247 | `c609e6a4ed90da3e8e3a52cfe6266da7501ebb3142df4d1746bfcd9457051b00` |
| Windows compiler build driver | 28,920,320 | `ee338c635aa817a26081c4327da4b36b78557f10518268162b8039d1f82316f4` |
| Linux compiler build driver | 28,921,856 | `7a4451e10fbc0eaa92c08f9752112f3933b7d6d519c6e3dc08f78517d7ac6e52` |

### Bring the Windvale inspector to the current WVB 1.11 surface

Extend the existing Windvale source, rather than creating a parallel inspector, to decode:

- value tags through builder `13`, including `i64`, `u64`, variant, sequence, and builder shapes;
- record, enum, and variant declarations with optional variant payloads;
- the 8,192 combined parameter/local bound; and
- every defined WVB 1.11 opcode through `0xBF`, including exact two-word reporting for wide constants and two-operand nominal/collection constructors.

The ordinary inspector remains structural and still requires the compiler-aligned verifier first. The focused native package test now compares its report with the reference runtime on a real compiler-produced module containing `u32.divide`, then executes the native package on the same bytes.

The replacement inspector artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Inspector WVB | 76,527 | `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753` |
| Windows inspector | 795,136 | `61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381` |
| Linux inspector | 794,624 | `d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627` |

### Keep assembly at the host boundary

This slice adds no assembly source. The runner and scalar interpreter semantics remain Windvale `.wv` modules. A packaging probe confirms that the existing read-only launcher shape is reusable; the remaining native-backend gap is exactly `bytes.from_i32_little` and `bytes.sha256_hex`. The first can share the existing 32-bit little-endian lowering, and the repository already has a qualified native SHA-256 implementation. The next slice may bind those backend services, but must not move WVB decoding or execution semantics into assembly.

## Consequences

- Large generated temporary sets no longer force a second-language interpreter implementation.
- Existing admitted source-to-WVB outputs retain their Stage 0 byte identity.
- Native `Inspect-Wvb` understands the current compiler-produced WVB 1.11 opcode surface used by the runner.
- The new temporary-slot module provides a natural source-ownership boundary instead of growing `Source-Wvb-Core.wv` further.
- Stage 0 still constructs the pinned inspector applications for recovery, and a full native compiler fixed-point reproduction remains open.
- This decision does not claim that arbitrary WVB execution, test orchestration, or .NET retirement is complete.

## Reconsideration triggers

Reconsider the allocator if control-flow-sensitive liveness can reduce pressure materially without weakening deterministic output or phi safety. Reconsider the inspector profile if WVB gains a new format version, metadata ceases to be optional for ordinary compiler output, or a shared report library can split scanning and formatting along a real ownership boundary without changing report bytes.
