# Decision 0074: Native Windows and Linux output services

- Date: 2026-08-01
- Status: Qualified on Windows and Debian x64
- Extends: [Decision 0073](0073-Native-Argument-Table-And-Process-Input-Services.md)'s execution-owned host-input pattern
- Advances: Native ABI 13, execution-context version 5, kernel native bridge 8, and firmware probe 15
- Retains: Service-table version 4, WVB 1.6, WVO 1.0, and all generated service-call shapes

## Context

Decision 0073 leaves three managed native-runtime callbacks: console output, diagnostic output, and file-byte input. Output is the next bounded slice because generated code already supplies verified strict-UTF-8 pointer/length pairs, both services have identical line semantics, and Windows and Linux expose narrow handle-based byte-write primitives.

The language contract must remain platform-independent. Windvale writes text plus LF; it does not expose Windows handles, Linux descriptors, console code pages, system calls, or host-native error values to `.wv` source.

## Decision

- Advance the shared target to `x86-64-wvb-baseline-v13` and append one output-table pointer to the 96-byte execution-context version 5.
- Define runtime-private `WVIO` table version 1 as 48 bytes: magic, version, size, platform, presence flags, reserved zero, console target, diagnostic target, and a Windows `WriteFile` pointer. Linux requires the final pointer to be zero.
- Replace the managed console and diagnostic delegates plus platform adapter thunks with exact runtime-native x86-64 leaves. Generated fragment code and the version-4 service-table layout do not change.
- Use a 258-byte Windows leaf that calls the verified `WriteFile` pointer and a 213-byte Linux leaf that issues direct `write` syscalls. Both complete partial writes, emit one LF after the text, preserve `R10`, `R11`, and `R15`, and reject zero or oversized progress. Linux retries `EINTR`.
- Give console and diagnostic leaves distinct exact identities because they select different table fields. The Windows console/diagnostic SHA-256 values are `10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48` and `1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2`. The Linux identities are `c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226` and `1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe`.
- Add service-failure detail 4 and stable `WVR3029` for an OS-rejected native output write. The reference host contains rejected `TextWriter` writes under the same code so the failure contract agrees across execution modes. Capability denial remains `WVR3010`; an absent channel remains `WVR3001`.
- Model host output as explicit `Nativeˉoutputˉchannel` values over safe file handles. The caller owns supplied handles; one execution add-references them, independently verifies the published table, and releases them after native return. Process stdout/stderr helpers do not transfer ownership.
- Advance the service-free OS bridge to version 8 and firmware probe to version 15. The bridge constructs the complete 96-byte context with a zero output-table pointer because the current guest module requires no runtime service.

## Safety boundary

The fragment verifier already proves output arguments are complete borrowed-text descriptors backed by immutable fragment data or execution-owned allocations and bounded by the WVB text limit. The native leaves receive only those compiler-generated arguments and a runtime-verified table. They do not decode UTF-8, allocate, retain pointers, unwind managed exceptions, or publish host identities into WVB/WVO.

Windvale always writes strict UTF-8 bytes followed by LF. File and pipe behavior is therefore exact across hosts. Interactive Windows console display encoding remains launcher policy until a native executable host owns process-console initialization; it is not a reason to change portable text semantics or return to ASCII-only output.

Stage 0 still constructs, verifies, allocates, and publishes executable leaves and the output table. `file.read_bytes` remains the sole managed service callback. This decision does not claim a standalone PE/ELF host, Windvale-owned runtime construction, or .NET retirement.

## Qualification evidence

Focused Windows execution covers direct JIT and linked WVO/AOT output, separate console and diagnostic handles, empty lines, euro and supplementary-Unicode text, maximum argument-table Unicode output, exact leaf reconstruction and corruption, authorization and missing-channel preflight, and output rejection mapping to `WVR3029` in both the reference and native runtimes. The zero-warning Release build passes, the native-output case takes 43 milliseconds, and Standard passes all 56 tests in 213.076 seconds. All 15 deterministic OS tests pass.

The ABI-13 rebuild keeps the service-free kernel WVB at 929 bytes and WVO at 8,010 bytes. Bridge 8 has 133 code bytes and produces a 345-byte object with SHA-256 `0a0393457200dbf5ecfbb667c6c283510a6eb13a3e7e77537a0b6d8e0f503d68`. Firmware probe 15 remains 15,872 bytes with SHA-256 `d716b77a91646da6b423bacb1faa6d70f5a097241c610fe49291b068f33d5f29`.

Exact commit `66b273f7e0839c7ebcd504cfdfdb9f0a4aad787e`, tree `278582a0f2f2465738b9ef98dd91df96ce52ec27`, was published to both configured remotes and archived as 2,909,424 bytes with SHA-256 `d36f20e21e78f31e02ac08d13452a7e759fd620a2492565b130080442f99c8ad`. Its size and digest matched after transfer to the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 56 tests, exact compiler reproduction, and the complete native CLI verifier. Their suite times are 232.193 and 247.986 seconds; their native-output cases take 0.042 and 0.037 seconds. The 15,563-byte Windows conformance report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 11,918-byte timing report has SHA-256 `70fc2082473a3b3c56f74c132f0ad626ae676bf23f3e39c9cf4f8a5c1927341f`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,526-byte timing report has SHA-256 `10d2dc708d7c43505a8fd52754cea677da0be1197dc6787057312e6a2720015e`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The 2,299,260-byte Debian evidence bundle has SHA-256 `20f600498fa1257d6586d0e3f33f9e5e1e057850f6c4cfe10547d01b1d9599ef`. Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG emits the complete version-15 marker for the exact image and returns guest-controlled host exit code 1; the Debian QA host does not provide QEMU.

GitHub [Verify run 30701244938](https://github.com/eworker-inc/Windvale/actions/runs/30701244938) passes its independent Windows and Linux jobs for the exact candidate. After evidence retrieval and comparison, the resolved QA directory, transferred source archive, and remote evidence bundle were removed and confirmed absent.
