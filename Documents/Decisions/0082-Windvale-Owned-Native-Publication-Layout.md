# Decision 0082: Windvale-owned native publication layout

- Date: 2026-08-01
- Status: Accepted, implemented, and cross-host qualified
- Extends: [Decision 0080](0080-Native-Byte-Result-And-Live-Stencil-Consumption.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, the qualified firmware-probe-17 identities, and all final runtime-service leaf identities
- Advanced by: [Decision 0365](0365-Native-Publication-Planner-Execution.md)

## Context

Decision 0080 makes bounded Windvale-produced bytes usable by the live native path, leaving W^X publication and lifetime as the next measured transfer. Inspection of that boundary found a narrower and safer first slice.

The native fragment verifier already requires every relative patch field in `fragment.Code` to contain its exact final base-independent displacement. The executor nevertheless performed a second C# patch rewrite after verification, writing the same bytes again. C# also chose the image extent and the 16-byte offsets of all runtime-service leaves. That duplicated validated semantics inside the most privileged publication path.

Moving Windows `VirtualAlloc`/`VirtualProtect`, Linux `mmap`/`mprotect`, invocation, and teardown into Windvale in one step would require a new FFI and lifetime system before the current planner could safely publish itself. A general Windvale linker at this seam would also duplicate the existing WVO linker and create a publication cycle.

## Decision

- Remove the redundant C# patch rewrite. Verified relative displacement bytes are copied unchanged from the fragment into the executable image. The independent fragment verifier remains the authority that rejects a missing, wrong, overlapping, out-of-range, or otherwise inconsistent patch before publication.
- Accept the bounded internal [`WVPQ 1` request and `WVPL 1` response contract](../../Specifications/Windvale-Native-Publication-Plan.md). It carries only fragment extent and the canonical ordered service ID/size list; it carries no machine bytes, addresses, handles, or ambient authority.
- Add portable `Compiler/Windvale/Native-Publication-Core.wv`. It validates every request field, computes 16-byte service placement and the final image extent with checked arithmetic, enforces the closed 11-service table and 34 MiB image ceiling, and emits one deterministic success or failure response.
- Add hosted `Compiler/Windvale/Native-Publication-Bridge.wv` and retain its exact WVB in `Runtime/Windvale.Native/Consumers`. It reads one immutable in-memory request through its sole `file.read_bytes` capability and returns the response through Decision 0080's bounded byte-result path under the Stage 0 reference interpreter. It does not invoke the native executor whose image it plans.
- Make every ordinary native execution use the accepted Windvale plan before writable allocation. The C# host independently reconstructs and validates every successful response field and placement before trusting it. Planner rejection maps to `WVN4013`; malformed successful output maps to `WVN4014`.
- Preserve existing image bytes: the fragment-to-first-service gap stays zero, later service alignment gaps stay `0x90`, and every already-qualified service leaf remains byte-identical.
- Keep operating-system allocation, protection, instruction-cache publication, invocation, service/context construction, arenas, and teardown in the C# platform adapter. This slice narrows that adapter but does not pretend the W^X or lifetime transfer is complete.

## Why the native ABI is retained

The generated fragment, entry convention, execution context, service table, runtime-private tables, leaf bytes, and invocation contract do not change. The planner describes where already-verified bytes are copied inside one allocation; it does not change how generated code addresses fragment data or calls a service. Previous executable results remain the required oracle. Advancing ABI 14 or rebuilding the service-free OS probe would describe no observable ABI change.

## Evidence contract

- Portable `Native-Publication-Core.wvb`: 7,189 bytes, SHA-256 `9d75d59e4ba0fc689ae9bc4ac3ac019e520db06d21f54d4ee1480a0bb356e967`.
- Retained hosted `Native-Publication-Bridge.wvb`: 7,105 bytes, SHA-256 `5102fd0119e37bb7e5f83bb3c4d1bff6303f37818bfe48825b320bf28f27eada`.
- The bridge has exactly one capability, `file.read_bytes`, and exactly one `Main() -> bytes` export.
- Focused coverage must exercise service-free and all-11-service plans, every request status family, size/order/reserved/range/image boundaries, deterministic repeated output, corrupt response envelopes and placements, retained-source reproduction, and live execution of a relative-data fragment whose verified patch bytes remain unchanged.
- Complete qualification must reproduce both WVBs and the retained bridge on Windows and Debian x64, compare all portable artifacts and normalized contracts, pass both Seed and OS suites, and retain the pinned-QEMU probe unless an applicable contract changes.

## Qualification

Exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`, tree `7ca8f6fdbf5a7caff8300ad4b1dbd490295930d6`, was published to both configured remotes. Its 3,014,724-byte archive has SHA-256 `ef6892d6c37cdc5e1461e66c8f2d5b72d7514f847078950220124b92690daa3c` and retained that exact identity on the isolated Debian GNU/Linux 12 x64 QA host with .NET SDK `10.0.302`.

Windows and Debian pass zero-warning Release builds, all 63 Seed tests, exact reproduction of both publication WVBs and the retained bridge, and the complete native CLI/reproduction gate. Complete Qualification takes 485.476 and 495.249 seconds, with suite times of 241.821 and 247.813 seconds. The new publication-layout case takes 17 and 22 milliseconds; its initial cold Windows development run took 0.565 seconds.

The 15,798-byte Windows report has SHA-256 `9196b6f7049cd810c8bfd09037886e6c8628c1631c053543250290bc2acd50d7`; its 13,084-byte timing report has SHA-256 `4b1bc7a87126bdb5de8961b707f8f03931876760aba1d45faf0144086efef77d`. The 15,705-byte Debian report has SHA-256 `c05f408eeb3d356e719382321705918bb6f9e6da803cd8e4efd5ddcfb1ebcc1f`; its 12,654-byte timing report has SHA-256 `b1b7b7210eebe5dce7465ae2afbed630c018c29bd23fa384148678675f932f72`. Their normalized contracts match exactly with SHA-256 `77cd1d742509fc235f23b2dd743518a5694bd94c80d4bb6d81340dfe37e506e4`.

All 67 portable artifacts, totaling 7,836,727 bytes, match byte for byte. Their canonical name/size/SHA-256 manifest has SHA-256 `527ac023a43dc6ca455c3b79654617138cc0bd5dd8b36d0801ba6f788ae3634c`. The retrieved 2,334,586-byte Debian evidence bundle has SHA-256 `a6a7441b7f790e2d1052e63ecd04f92bec17f8325acc6b7aaeeab74a91fb8e33`. Both hosts pass all 17 OS tests, both pinned-QEMU probe-17 scenarios pass with their exact qualified identities, and GitHub [Verify run 30715672194](https://github.com/eworker-inc/Windvale/actions/runs/30715672194) independently passes Windows and Linux verification.

## Consequences

- Windvale now owns executable-image extent and canonical service-leaf placement, and verified fragment code is no longer rewritten after acceptance.
- C# remains an independent response validator and owns exact leaf reconstruction, platform memory calls, byte copying, W^X transition, instruction-cache flush, context/service/arena ownership, invocation, status mapping, and cleanup.
- The extra planner evaluation is intentionally kept in the focused runtime path. Its measured development cost determines whether a later qualified cache can reuse plans by exact fragment/service identity without weakening validation.
- [Decision 0083](0083-Windvale-Owned-Native-Publication-Lifetime.md) performs the next measured transfer by defining explicit publication-lifetime state and isolating the platform adapter without a general FFI or arbitrary pointer escape.
- This is not native self-hosting, a standalone runtime, a stable executable container, general dynamic linking, or .NET retirement.
