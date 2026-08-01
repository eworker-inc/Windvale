# Decision 0076: Native Windows and Linux file input

- Date: 2026-08-01
- Status: Qualified at exact commit `ef0861980f7309ca7cac709f6930b5e11a4c8208`
- Extends: [Decision 0074](0074-Native-Windows-And-Linux-Output-Services.md)'s runtime-private host-I/O pattern
- Advances: Native ABI 14, execution-context version 6, kernel native bridge 9, and firmware probe 16
- Retains: Service-table version 4, WVB 1.6, WVO 1.0, and all generated service-call shapes

## Context

Decision 0074 leaves `file.read_bytes` as the only native runtime service that enters a managed callback. Windvale programs already pass a compiler-verified borrowed-text descriptor and a verified output cell. The remaining work is therefore a bounded host-I/O leaf and execution-owned resource state, not a language or generated-call change.

The portable contract must not expose Win32 handles, Linux descriptors, host error numbers, current process internals, or native pointers. Source still observes only a named immutable byte snapshot or a stable `WVR302x` failure.

## Decision

- Advance the shared target to `x86-64-wvb-baseline-v14` and append one file-input-table pointer to the 104-byte execution-context version 6.
- Define runtime-private `WVFI` table version 1 as 136 bytes. It carries the platform identity, a 64-record snapshot table, bounded name/data arenas, one path scratch buffer, and seven Windows function pointers. Linux requires all Windows pointers to be zero.
- Replace the managed file delegate and both platform adapter thunks with exact runtime-native x86-64 leaves. The generated fragment and version-4 service table remain unchanged.
- Use a 1,218-byte Windows leaf with SHA-256 `3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8`. It validates the name, converts strict UTF-8 with `MultiByteToWideChar`, and calls verified `CreateFileW`, `GetFileSizeEx`, `ReadFile`, `CloseHandle`, and `VirtualAlloc` pointers.
- Use a 996-byte Linux leaf with SHA-256 `55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb`. It validates and copies the path, then issues direct `openat`, `read`, and `close` syscalls, retrying interrupted reads.
- Preserve exact ordinal first-success snapshots. A repeated name returns the original descriptor without reopening the file. Failed requests publish no record. A run may publish at most 64 distinct names, each name is at most 1 MiB of strict UTF-8, and each result is at most 4 MiB.
- Add service-failure details 5 through 10 for invalid name, not found, permission denied, unavailable, too large, and snapshot limit. The executor maps them to existing stable codes `WVR3021`, `WVR3022`, `WVR3023`, `WVR3024`, `WVR3025`, and `WVR3028`.
- Require explicit `Nativeˉfileˉinput.Hostˉfileˉsystem()` configuration in addition to capability authorization. A Stage 0 `IHostedˉfileˉreader` remains available to the reference interpreter but is neither accepted nor called by native file execution.
- Advance the service-free OS bridge to version 9 and firmware probe to version 16. The bridge constructs the complete context with a zero file-input-table pointer because the guest module requires no hosted service.

## Runtime-private state

Each run reserves 64 canonical snapshot records. Record `i` names canonical name slot `i * 1 MiB` and canonical data slot `i * 4 MiB`; the runtime independently verifies every published pointer, length, reserved field, strict-UTF-8 name, and ordinal uniqueness after native return. Windows reserves the arenas and commits the selected slots from the native leaf. Linux uses private anonymous mappings whose pages materialize on access. All state is released after `Main` returns, so no borrow escapes its execution.

The table also contains one platform path scratch area: up to 1 MiB plus NUL on Linux, or the corresponding UTF-16 capacity on Windows. Embedded NUL, empty, oversized, malformed Windows UTF-8, and invalid host paths fail before a snapshot is published.

## Safety and migration boundary

The fragment verifier proves that the incoming name and destination descriptor use compiler-generated, bounded provenance. The runtime reconstructs each platform leaf, verifies its exact size and digest, validates the complete table before W^X publication, and validates all published records afterward. Native code never calls a C# delegate and native failures never unwind through managed frames.

This completes native execution for all eleven service-table slots, but it does not complete .NET retirement. C# Stage 0 still selects and reconstructs leaves, creates and verifies tables and arenas, allocates W^X memory, applies relocations, invokes `Main`, maps traps, and packages the OS image. It also remains the independent reference/recovery compiler and interpreter. The next ownership step should move construction of one bounded runtime artifact into Windvale while retaining byte-for-byte comparison with this implementation.

## Qualification evidence

The existing hosted-input test now reconstructs and corrupts both platform leaf identities; runs the WVB inspector through direct JIT and linked WVO/AOT against a real OS file; proves its supplied Stage 0 reader remains at zero calls; verifies first-success cache reuse; and covers invalid, missing, oversized, and 65th-name failures. The complete 1,441-line Windvale `wvdump` also runs through JIT and linked WVO/AOT using the direct host-file boundary.

The first Debian run exposed two exact Linux-leaf defects before qualification: the NUL scan cleared the register holding the `WVFI` table, and a four-byte read-request slot overlapped the upper half of the saved scratch pointer. Commit `96ebe70` reloads the table and separates the stack slots. The complete native `wvdump`, a 4 MiB + 1 boundary file, and all other hosted-input cases then pass on Debian. Commit `ef08619` additionally clears the expected `EX_IOERR` result from the final negative Qualification command so PowerShell callers receive the gate's successful exit status.

Exact commit `ef0861980f7309ca7cac709f6930b5e11a4c8208`, tree `bcafa888e4f9bf470a4c42e3aa0173acd47bd13f`, was published to both configured remotes and archived as 2,931,444 bytes with SHA-256 `d9fdfead5c1d42586d4c9a96102389491553f494059df120df2318d7867fafb7`. The archive retained the same size and digest after transfer to the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 57 Seed tests, exact compiler reproduction, and the complete native CLI verifier. Their suite times are 238.115 and 257.556 seconds; complete Qualification takes 499.037 and approximately 529 seconds wall-clock. Their native hosted-input cases take 0.130 and 0.101 seconds. The 15,563-byte Windows report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 12,074-byte timing report has SHA-256 `deb25a16aaaadeabaf15395167e58fc435f260c4f792416f80089072deb2a232`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,675-byte timing report has SHA-256 `0d0de32e961bc112f2f1e03f64b4f87de2cb50a3ab97953e7963d7646c7b6aac`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The 2,299,009-byte Debian evidence bundle has SHA-256 `2ac389f97d5f94b4ae60a5dd0ee8fee3cf9a62a0851b5fbd20e35fcf7e829a89`.

Both hosts pass all 15 OS tests. Bridge 9 has 138 code bytes and produces a 350-byte object with SHA-256 `3cbf50a4828a1a69ca7441a667cb95e569055468c345ed26b8a580fda3facfc5`. Pinned QEMU 11.0/Q35/TCG boots the exact 15,872-byte firmware-probe-16 image with SHA-256 `206a036f8cbe3198544b6878bf52c80ef8d489c14d5437c6c7004ff1d6599504`, emits the complete version-16 success transcript, and returns guest-controlled host exit code 1. The Debian QA host does not provide QEMU. GitHub [Verify run 30704485295](https://github.com/eworker-inc/Windvale/actions/runs/30704485295) passes its independent Windows and Linux jobs. After retrieval and comparison, every resolved ABI-14 QA directory, transferred source archive, transient trace, and remote evidence bundle was removed and confirmed absent; the temporary diagnostic package was also removed.

This supersedes Decision 0074 as the latest qualified native-runtime and OS evidence. It qualifies native leaves for every closed service slot without claiming Windvale-owned runtime construction, W^X publication, arena ownership, standalone native tools, in-guest WVB loading, or .NET retirement.
