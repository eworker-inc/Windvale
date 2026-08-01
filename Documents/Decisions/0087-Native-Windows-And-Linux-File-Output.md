# Decision 0087: Native Windows and Linux file output

- Date: 2026-08-01
- Status: Accepted and cross-host qualified at exact commit `12e9e2e`
- Extends: [Decision 0076](0076-Native-Windows-And-Linux-File-Input.md)'s runtime-private host-file pattern and [Decisions 0085](0085-First-Wva-Owned-Q35-Clean-Shutdown.md) and [0086](0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md)'s OS contracts
- Advances: Native ABI 15, execution-context version 7, service-table version 5, kernel native bridge 10, and firmware probe 20
- Retains: WVB 1.6, WVO 1.0, every existing service slot, and all source-visible hosted-resource semantics

## Context

The exact qualified 599,868-byte `Compilerˉsourceˉwvbˉtool` declares six capabilities. ABI 14 already supports five of them; `file.write_bytes` is its sole unsupported declared native capability and native compilation therefore stops at `WVN2001` before lowering.

The capability already exists in Seed, the reference interpreter, the CLI, and the hosted-resource specification. The missing work is one native whole-file leaf and its explicit host authority. Treating this as a general file or FFI project would widen the trusted surface without helping the compiler.

## Decision

- Accept the bounded [`file.write_bytes` native contract](../../Specifications/Windvale-Native-File-Output.md).
- Advance the target to `x86-64-wvb-baseline-v15`; append `Fileˉwriteˉbytes` as native service 12 and append its pointer at byte 96 of the 104-byte service-table version 5.
- Append one file-output-table pointer at byte 104 of the 112-byte execution-context version 7. Keep the existing file-input table separate so compiler runs can read immutable snapshots and publish output in the same execution.
- Define runtime-private `WVFO` version 1 as 80 bytes. It owns one bounded path scratch buffer and six Windows function pointers; Linux keeps those pointers zero and uses direct system calls.
- Add exact Windows and Linux x86-64 leaves that validate the bounded inputs, create or replace one file, complete partial writes, durably flush, close, and map expected platform failures to existing `WVR3021` through `WVR3025` classifications.
- Require explicit `Nativeˉfileˉoutput.Hostˉfileˉsystem()` configuration in addition to exact capability authorization. The Stage 0 `IHostedˉfileˉwriter` remains the independent reference path and is not called by native execution.
- Preserve non-atomic replacement semantics. A failure may leave a created, truncated, or partially written host file.
- Advance the service-free kernel bridge and firmware probe because the execution-context ABI changes, while supplying a zero file-output-table pointer in the guest probe.

## Acceptance

Focused coverage must reconstruct and corrupt both exact leaves and the static table; cover authorization, missing configuration, empty/Unicode/maximum values, replacement, truncation, invalid and missing-parent paths, and contained platform rejection; and compare direct JIT with linked WVO/AOT publication.

The exact qualified compiler WVB must be preflighted again after this change, and `file.write_bytes` must no longer be the reported blocker. If compilation or execution exposes another unsupported construct or service, the evidence names that next blocker rather than claiming the compiler is already self-hosting natively.

Complete qualification must reproduce portable artifacts on Windows and Debian x64, compare normalized contracts, pass Seed and OS suites, and qualify the advanced pinned-QEMU probe identities.

## Qualified implementation evidence

The implementation adds exact 787-byte Windows and 823-byte Linux leaves with SHA-256 values `a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1` and `fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422`. Focused Windows and real Debian tests pass direct JIT and linked WVO/AOT publication, Unicode and empty files, replacement and truncation, the exact 4 MiB value boundary, authorization/configuration failures, invalid names, missing parents, and platform rejection.

Repeating preflight over the exact 599,868-byte compiler WVB with SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066` proves that `file.write_bytes` is no longer its admission blocker. Native compilation next stops at `WVN2002` in `Compilerˉbodyˉblockˉstepˉvalid`, whose record-shaped parameters or locals are outside the backend's currently bounded scalar/borrowed-descriptor function shape. This is progress evidence, not a claim of native compiler execution.

The implementation also advances the Windvale publication planner to the closed 12-service domain. Its current portable core remains 7,189 bytes with SHA-256 `b25fa550518caa4ef43c7ae886cce328148777782f70e3faa25ac19821b6d439`; the regenerated 7,105-byte retained bridge has SHA-256 `750b6134395c46c9e1c703ae2a56449bd1710f517e516397e10a1ccc951c503e`.

Exact commit `12e9e2ebcd4960f856b90064f6343ea5856b5b43`, tree `3b42f22fcc2f181029847ac6e1549650b0b49031`, was published to both configured remotes. Its 2,868,384-byte archive has SHA-256 `a52e68505f7ac9aeef5a3bfacf16c5c722fa60efaac75b3c9d7ddb85c0b600a7` and retained the same digest on the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Qualification, all 66 Seed tests, exact compiler and retained-WVB reproduction, and the complete native CLI gate. Complete Qualification takes 523.3 and 524.3 seconds wall-clock, with suite times of 257.247 and 267.047 seconds. The native-file-output case takes 1.015 and 1.162 seconds; the golden contract remains the dominant case at 189.984 and 197.493 seconds.

The 15,798-byte Windows report has SHA-256 `2b394b61a01e03a5e81623e203028303b4e29595dfec36b15bdd8d5ca6105182`; its 13,566-byte timing report has SHA-256 `f26b28352ef1170113842c51491f45c338243d4e609ae0c12c69f2ca405bc508`. The 15,705-byte Debian report has SHA-256 `59fb87fd8695ba2199bf6d404424366112b322e197c2c7434624d431f80a6ccb`; its 13,125-byte timing report has SHA-256 `30a1ee9fa772d06bf638706eb6393f0b3d9f71b7cc5477a0120200a2bddfc95f`. The built-in comparator confirms exact normalized-contract equality; canonical compact contract JSON has SHA-256 `e41b9c1ccae150ebac3465cdfee74f89796451d528f4f246f70290926e02e621`.

All 69 portable artifacts, totaling 7,848,859 bytes, match byte for byte. Their canonical name/size/SHA-256 manifest has SHA-256 `2545622421f77c8f40180732d779e7c82a9575b15e4deef45f8072ccf4aac676`. The directly retrieved 2,334,180-byte Debian portable-evidence bundle has SHA-256 `97795302a4bb03ccae660121a3b592f990c91504004bd7b6adeca13dcc222ae4`.

Both hosts pass all 18 OS tests. Firmware probe 20 composes the ABI rebuild with Decisions 0085 and 0086's WVA-owned clean-shutdown and normalized-trap paths. Exact-archive pinned QEMU qualifies all three 20,992-byte images: normal SHA-256 `d4a9e3625779dd3ef2a03fd71ecfe1502c1ad39378da7adbcf7e4b55636eed8c` exits 0 after the complete shutdown marker; invalid-opcode SHA-256 `705670b1054589b80e3c918c03e9f751304e3f4b5bda77485f606433db68a757` exits 3 with `(6, 0)`; and general-protection SHA-256 `df45d8e0f69581e5ed3b46608598e6170413f80c5c1bbba9233e9842cdd7a04d` exits 3 with `(13, 0)`. GitHub [Verify run 30721964387](https://github.com/eworker-inc/Windvale/actions/runs/30721964387) independently passes classification plus Windows and Linux verification for the exact commit.

The first exact implementation archive, commit `54ff401`, passed all 66 in-process tests on both hosts but qualification correctly rejected stale hard-coded publication-core and bridge identities in the verifier. Commit `12e9e2e` updates those qualification pins to the already-produced exact artifacts and reruns the complete gate; no artifact or runtime semantic was normalized away.

After evidence retrieval and comparison, the exact Debian QA trees, transferred source archives, remote evidence bundles, and earlier working-copy QA inputs were removed and confirmed absent. The exact local archive, both host reports/artifacts, and retrieved portable-evidence bundle remain retained.

## Consequences and limits

Native compiler-shaped programs gain bounded file publication with no managed callback in the live leaf. Windvale source still receives no pointer, handle, descriptor, path parser, platform error number, or ambient file authority.

C# Stage 0 still constructs and verifies the table and leaves, resolves Windows exports, publishes executable memory, invokes the entry, maps failure details, and owns process containment. This decision does not add atomic replacement, a recoverable source-level error model, general file I/O, or a general FFI.
