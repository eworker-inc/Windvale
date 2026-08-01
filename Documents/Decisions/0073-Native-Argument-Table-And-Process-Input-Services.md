# Decision 0073: Native argument table and process-input services

- Date: 2026-08-01
- Status: Qualified on Windows and Debian x64
- Extends: [Decision 0072](0072-Final-Pure-Runtime-Native-Services.md)'s exact runtime-native service pattern
- Advances: Native ABI 12, execution-context version 4, kernel native bridge 7, and firmware probe 14
- Retains: Service-table version 4, WVB 1.6, WVO 1.0, and all generated service-call shapes

## Context

After Decision 0072, all six deterministic pure runtime services execute in exact platform-neutral x86-64 leaves. Five hosted/capability services still cross managed delegates and Windows/System V adapter thunks: console output, diagnostic output, argument count, argument text, and file-byte input.

Arguments are a useful first hosted slice because the launcher has already captured and validated the complete immutable argument snapshot before native execution begins. Counting or selecting that snapshot needs no operating-system call, blocking I/O, callback exception, or mutable host state. Embedding process addresses into generated code or WVO would be unsafe and nondeterministic, while appending execution-owned data to the existing context preserves one portable fragment identity.

## Decision

- Advance the native target to `x86-64-wvb-baseline-v12` and execution context to version 4. Append a runtime-private argument-table pointer, `u32` count, and zero reserved field; preserve every earlier field and offset.
- Represent the table as a contiguous array of exact 16-byte borrowed-text descriptors. Each entry contains pointer, strict-UTF-8 byte length, and zero reserved field. The count remains bounded to 67, each argument to 4 KiB, and the aggregate to 64 KiB by the existing hosted-resource contract.
- Construct the table eagerly only when `process.argument_count` or `process.argument` is required. Pack argument bytes into one execution-owned immutable allocation, construct every descriptor from that allocation, then independently reread and verify every pointer, length, reserved field, bound, and byte sequence against the prevalidated resource snapshot before publishing the context.
- Keep explicit capability authorization and host-support checks before table construction. The native leaves do not turn argument access into a pure capability or permit ambient process inspection.
- Replace `process.argument_count` with one exact 5-byte platform-neutral x86-64 leaf. Its SHA-256 is `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829`. It reads the context count into `EAX` and returns.
- Replace `process.argument` with one exact 70-byte platform-neutral x86-64 leaf. Its SHA-256 is `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1`. It checks the unsigned index before loading the table, copies one complete descriptor into the compiler-verified result cell, and returns status zero.
- Add service-failure detail 3 for an argument index outside the snapshot. The failed leaf performs no table load, returns service status one, and the executor maps the detail to `WVR3020`.
- Require both leaves to preserve `R10`, `R11`, and `R15`. Deterministically reconstruct and require their exact bytes before W^X publication. Keep the service table at version 4 and preserve its argument slots and generated-code call conventions.
- Remove both managed argument delegates and both platform-specific argument adapters. Console output, diagnostic output, and file-byte input remain the three managed hosted adapters after this slice.
- Keep the argument table runtime-private and per-execution. Do not serialize process pointers into WVB, WVO, or a native code cache; release the table and packed bytes after `Main` returns.
- Advance the service-free Windvale OS bridge to version 7 and firmware probe to version 14. The bridge constructs the complete 88-byte context with a zero argument pointer/count/reserved tuple because the guest probe remains service-free.
- Do not describe this as a Windvale-written native runtime or .NET retirement. C# still constructs and verifies the table and service leaves, publishes W^X memory, owns execution and arenas, and supplies the three remaining hosted adapters. It remains the independent reference/recovery implementation while Windvale-written ownership grows.

## Qualification evidence

The existing hosted-input test now reconstructs and verifies both exact leaves, rejects corrupted service bytes, retains service-table load and ordering corruption coverage, and compares the reference interpreter with native execution over the maximum 67-entry snapshot. The snapshot includes empty text, ASCII, euro, and supplementary Unicode arguments; every descriptor is consumed. Separate coverage proves the zero-count path and exact `WVR3020` out-of-range behavior. Its warm Windows Standard pass takes 20 milliseconds.

The zero-warning solution build passes. All 33 runtime-tagged tests pass in 65.794 seconds, pre-commit Windows Standard passes all 56 tests in 220.013 seconds, and all 15 deterministic OS tests pass. The service-free kernel WVB and 8,010-byte WVO retain their exact identities. The new bridge has 128 code bytes and produces a 340-byte object with SHA-256 `0f6d4f00e6a66c23dedc7c6224cdae3f556c5d1c0ff927e596c927a73fd9829f`. Firmware probe 14 is 15,872 bytes with SHA-256 `aadfbc5cb56f6afea94605ad31ee6af6a60b1e821403dfb8e1c2550631b6d548`. The pre-commit pinned QEMU 11.0/Q35/TCG gate emits the complete version-14 marker and returns guest-controlled host exit code 1.

Exact commit `328e4552829b236e5baeb1f01447010855842730`, tree `cc5dc70848655e3bc1a403d28c700bf4e3c3f71d`, was published to both configured remotes and archived as 2,900,102 bytes with SHA-256 `692774c8780eeabc30f758aec49373b23685d542252ea80475c04289ec84f3d7`. Its digest and size matched after transfer to the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 56 tests, exact compiler reproduction, and the complete native CLI verifier. Their suite times are 238.075 and 252.295 seconds; their native hosted-input cases take 0.020 and 0.022 seconds. The 15,563-byte Windows conformance report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 11,916-byte timing report has SHA-256 `aff0fece57fb555ecfad153dbbb9c2745ae305f67826aa2248da10ee6eb98c9f`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,523-byte timing report has SHA-256 `a0c8e76975837d2bb9f5a1179b58976d9be71990926812258becc0d3421cce7c`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The 2,299,022-byte Debian evidence bundle has SHA-256 `c9043ab9fdc72241affda5dabd8250adf5d39ecc73b9bf48901e27dc4e056c94`. Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG boots the exact probe-14 image, emits the complete version-14 marker, and returns guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

GitHub [Verify run 30698761104](https://github.com/eworker-inc/Windvale/actions/runs/30698761104) passes its independent Windows and Linux jobs for the exact candidate. After evidence retrieval and comparison, the resolved QA directory, transferred source archive, remote evidence bundle, and temporary QA inputs were removed and confirmed absent.

## Consequences

Eight of eleven closed runtime-service slots now execute without managed callbacks or platform adapter thunks. The two argument services are still capabilities, but their captured immutable data plane is native after one checked publication step. Only console output, diagnostic output, and file-byte input cross managed callbacks during execution.

ABI 12 changes the execution context but not portable WVB/WVO formats or generated service-call shapes. Older qualified native artifacts remain historical evidence and are rejected by the current fragment verifier.

The next adapter slice should address output or file input only with an explicit native Windows/Linux system boundary, stable error mapping, and the standalone service/container metadata it needs. Moving C# table/service construction into Windvale remains a separate ownership milestone from removing callbacks.

## Reconsider when

- A native process launcher supplies `argc`/`argv` directly and needs a compatible zero-copy validation path.
- A standalone PE, ELF, or Windvale-native container must describe, authorize, and verify hosted services without the original `Nativeˉfragment`.
- Argument lifetime or encoding changes from one immutable strict-UTF-8 snapshot per execution.
- A Windvale-written runtime owns table construction and can replace the Stage 0 builder while retaining exact validation and recovery evidence.
