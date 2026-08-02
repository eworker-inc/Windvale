# Decision 0089: Bounded native stack arguments

- Date: 2026-08-01
- Status: Accepted, implemented, and cross-host qualified
- Extends: [Decision 0063](0063-Shared-Budget-Native-Calls-And-Static-Data.md)'s four-register internal call convention and [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)'s descriptor result convention
- Advances: Native ABI 16
- Retains: Execution-context version 7, service-table version 5, all 12 native services, WVB 1.6, WVO 1.0, and the host entry convention

## Context

Decision 0087's exact compiler preflight stopped at `WVN2002` in `Compilerˉbodyˉblockˉstepˉvalid`. The diagnostic described the complete admitted function shape, so the first interpretation named record-shaped parameters or locals as the missing feature. Exact inspection disproves that interpretation: the function has eight `u32` parameters and returns a record, while record parameters, locals, results, and returns are already admitted as bounded record-arena offsets.

The exact 599,868-byte compiler WVB contains 328 functions. Only 225 have four or fewer parameters; 103 have five through 23 parameters. Patching only the first eight-parameter failure would therefore create another immediate loop. Windvale source already limits a function to 64 parameters, so the native convention needs one equally bounded general answer rather than a function-specific rewrite.

## Decision

- Advance the current target to `x86-64-wvb-baseline-v16` and admit at most 64 internal parameters, matching the existing source-language declaration limit.
- Retain the first four parameter positions in `R8`, `R9`, `RCX`, and `RDX`. Scalars, enums, and record-arena offsets use the low dword. Borrowed `text` and `bytes` use the complete register as a pointer to the caller's verified 16-byte descriptor.
- For positions 4 through 63, reserve exactly `(parameters - 4) * 16` outgoing bytes immediately before the call. Cell `i` begins at caller offset `(i - 4) * 16`. A scalar occupies the low dword; a borrowed descriptor copies both verified machine words.
- Keep the outgoing reservation 16-byte aligned and bound it to 960 bytes. The caller loads the first four registers before moving `RSP`, copies later arguments from its adjusted frame into canonical outgoing cells, passes any hidden descriptor-result pointer relative to the adjusted frame, calls, and releases the exact reservation before inspecting or propagating the packed return status.
- After allocating its own frame, the callee copies a later argument from `RSP + frame-bytes + 8 + cell-offset`; the extra eight bytes account for the internal call's return address. Callees never retain pointers into the outgoing area.
- Preserve existing packed scalar/enum/record/status results in `RAX` and the existing hidden descriptor-result cell convention. Do not serialize this experimental ABI into WVO 1.0 or expose it as a public host FFI.
- Require the fragment verifier to reconstruct reservation size, every source and destination cell, scalar versus descriptor shape, hidden-result adjustment, direct call target, exact release, caller/callee type agreement, and reachable control flow before either JIT or WVO/AOT execution.

## Acceptance

Focused differential coverage must execute the maximum 64-parameter scalar call, a borrowed descriptor beyond the register positions, a descriptor return, and a wide void call through the reference interpreter, W^X JIT, and linked WVO/AOT image. It must prove deterministic code and reject corrupt reservation sizes, outgoing cells, descriptor copies, and releases.

The exact compiler WVB must pass `Compilerˉbodyˉblockˉstepˉvalid` and name its next real blocker. Complete qualification must retain zero-warning Windows and Debian builds, normalized-contract equality, portable-artifact equality, native CLI coverage, OS tests, and the permanent redirected-process UTF-8/macron regression.

## Qualification evidence

Focused Windows evidence passes a maximum-width 64-scalar call, a fifth-position borrowed-byte descriptor with a descriptor return, and a five-parameter void call through the interpreter, actual W^X execution, and linked WVO/AOT execution. Deliberately changing the 960-byte reservation, first outgoing cell, exact release, or descriptor-cell high word is rejected as `WVN3030` by independent fragment reconstruction.

The fresh zero-warning Windows Standard gate on the integrated Decisions 0088 through 0090 tree passes all 67 Seed tests in 237.692 seconds. The new bounded-wide-call case takes 39 milliseconds; the unchanged golden closure remains dominant at 177.231 seconds. All 43 compiler-area tests passed in the earlier focused sweep, and all 21 current OS tests pass, including the later fixed WVB-admission slice. The retained at-most-four-parameter native probe remains byte-identical across ABI 15 and ABI 16; probe-21 firmware identities change only because Decision 0090 adds admission artifacts and evidence. The 15,798-byte candidate report has SHA-256 `310b2b003caf04e257bd14d9ae614f2bf02668fc5f6df0c26d9267ba4d1c25b1`; its 13,729-byte timing report has SHA-256 `dd663af1fa49004896174d195d822b9d088226ca3d1c8c72d61dc917d8220053`.

Repeating native preflight over the exact compiler WVB now passes the former eight-parameter function and advances to `WVN2002` in `Compilerˉsourceˉwirˉcompileˉblock`. That function has 11 already-supported parameter shapes, 1,049 locals, and maximum WVB stack depth 12; the current native frame cap is 1,024 slots. Across the complete compiler, this is the only function at or above 1,024 locals; the observed maxima are 1,049 locals, stack depth 34, and locals-plus-declared-stack depth 1,061. Bounded frame admission is therefore the next measured slice. This is progress evidence, not a claim of native compiler execution.

Exact integrated commit `860c69c00995de6ed048cb65f8bfb158287f19a2`, tree `5c885feac990dc65ab5a7577fd44c6d39dc55c10`, was archived as 7,057,219 bytes with SHA-256 `fdcddb7ebdbab7b791ef5e1c0e98e87fc0b4415ed5b078fe427c8920ff4c08a6`. The same archive completed Qualification in 501.3 seconds on Windows and 517.2 seconds on Debian GNU/Linux 12 x64 with .NET SDK 10.0.302. Suite times are 243.529 and 257.101 seconds; the wide-call case takes 38 and 39 milliseconds, and the golden closure takes 180.409 and 191.031 seconds.

Both hosts pass zero-warning builds, all 67 Seed tests, the complete native CLI/reproduction gate—including redirected macron-bearing output—and all 21 OS tests. Their normalized contract has SHA-256 `240595dd55f602f724951e2e1d644ba577a1783606380ae584e210c82df9369b`; all 69 portable artifacts totaling 7,851,187 bytes match byte for byte. The Windows report is 15,798 bytes with SHA-256 `310b2b003caf04e257bd14d9ae614f2bf02668fc5f6df0c26d9267ba4d1c25b1`; the Debian report is 15,705 bytes with SHA-256 `f54dbe55cd43e2199423034d68917baef60d8853b23995860ad10576b6639aaa`. Independent GitHub [Verify run 30724785769](https://github.com/eworker-inc/Windvale/actions/runs/30724785769) passes its Windows and Linux jobs.

## Consequences and limits

Ordinary Windvale functions no longer need source rewrites merely to fit four machine registers. The convention remains deterministic, allocation-free, host-independent, and bounded by existing source semantics. Calls using at most four parameters preserve their prior machine bytes, so the service-free OS probe's WVO code is unchanged even though current fragments identify ABI 16.

The selector still gives every admitted native local and numbered value a dedicated 16-byte frame cell. Decision 0089 does not raise that frame bound, introduce slot reuse or register allocation, change WVO, execute the compiler natively, add general aggregates, expose raw stack access, or retire C#/.NET.

## Reconsider when

- value-slot reuse or register allocation changes the canonical frame model;
- a future source version raises or lowers the 64-parameter declaration limit;
- aggregate-by-value or managed references require roots, safe points, or a different copy rule;
- another architecture needs a target-specific register prefix while preserving the same bounded semantic cells; or
- a stable public FFI is proposed, which requires a separate host-facing ABI and compatibility policy.
