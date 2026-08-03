# Decision 0133: Frame-owned direct native records

- Date: 2026-08-02
- Status: Implemented and Windows-qualified; cross-host qualification pending
- Advances: Native ABI 21, target `x86-64-wvb-baseline-v21`, and kernel memory `WVKMEM11`
- Retains: Execution-context version 7, service-table version 5, the 2,048-cell frame ceiling, the record-arena fields, Probe 32, protected-process format 11, firmware format 32, and the 34 MiB publication-image ceiling
- Refines: [Decision 0111](0111-Bounded-Exact-Compiler-Fragment-Publication.md), [Decision 0112](0112-Bounded-Exact-Compiler-Record-Arena.md), [Decision 0115](0115-Exact-Compiler-Record-Lifetime-Pressure.md), [Decision 0117](0117-Nominal-Native-Record-Storage-Plan.md), [Decision 0118](0118-Deterministic-Native-Record-Storage-Offsets.md), [Decision 0119](0119-First-Windows-Console-Application.md), and [Decision 0122](0122-First-Linux-Console-Application.md)

## Context

ABI 20 represented every immutable record as a 32-bit offset into one execution-scoped monotonic arena. That model is simple and bounded for small programs, but full native compiler reproduction constructs records in loops and across helper calls. Decision 0115 measured more than 77 million constructed fields in the successful reference run, so increasing the arena would turn transient compiler work into an accidental multi-gigabyte lifetime promise.

Decisions 0117 and 0118 retained exact nominal identities, proved liveness-bounded storage, and published deterministic frame-cell offsets. The exact compiler requires at most 1,489 of the existing 2,048 cells in any function. The remaining step is to make selected machine code consume those offsets without changing Windvale value semantics or creating a second native backend.

Direct field copies make the current unoptimized baseline fragment 16,905,513 bytes. This exceeds Decision 0111's 8 MiB fragment ceiling even though it remains well below the already qualified 34 MiB final publication extent. The limit and the Windvale-owned publication planner must therefore move together or fail closed.

The backend is shared by host execution, the Windows and Linux console containers, and the Windvale OS build. Rebuilding any of those consumers through ABI 21 changes its selected fragment even when the source-level or guest-visible behavior is unchanged. They must therefore consume the same physical-frame plan and revise measured bounds in this decision rather than retain an ABI-20 artifact beside the current compiler.

## Decision

- Advance the single shared x86-64 backend to ABI 21. Do not create a parallel record compiler, runtime, or compatibility selector.
- Represent a record handle as one absolute 64-bit pointer to verified frame-owned backing. A handle is never a host object, record-arena offset, or ownership transfer.
- Use the exact Decision 0118 maps as the selector's only record-backing source. Persistent record locals use their whole-function liveness offsets; semantic record results use their block-liveness offsets.
- Construct a direct record by copying each field's complete 16-byte cell into the result's planned backing, then publishing that backing address in the result handle.
- Loading a record local creates an immutable value copy in planned scratch backing. Storing a record copies into the local's planned persistent backing. An unassigned record parameter may continue to borrow its caller pointer because no write can invalidate it; an assigned parameter receives owned backing before ordinary operations execute.
- Pass record arguments as backing pointers. The first four use full `R8`, `R9`, `RCX`, and `RDX`; later arguments use the low machine word of their existing 16-byte outgoing cells. Borrowed descriptors keep their complete two-word stack representation.
- Return a record through caller-owned backing. The caller places the planned destination address in hidden `RAX`; the callee saves that pointer, copies every direct field into it, and returns zero status in `RAX`. The caller then publishes its own destination address as the result handle.
- Tag every selected record construction, copy, field access, record-returning call, and record return with its nominal type. Independently decode exact widths, pointers, frame ranges, call kinds, return kinds, and control-flow boundaries before publication.
- Reject record-valued fields in ABI 21. Nested records require a separate recursive layout and deep-copy decision.
- Retain execution-context version 7 and its record-arena fields as a dormant bootstrap and OS compatibility seam. ABI-21 generated record operations do not read, advance, or branch to that arena; successful native record programs report zero record-arena use. The historical packed status 7 / `WVR3017` suffix remains structurally decoded but is unreachable from admitted generated record operations.
- Rebuild both version-1 console targets through ABI 21 and the same verified native-fragment preparation boundary. The ABI transition does not change scalar native selection; outer PE and ELF identities remain owned by their independently versioned startup contracts. Their fixed record-arena context field remains a dormant compatibility extent; a nominal-record container exercise must consume zero record-arena bytes.
- Increase the hard native-fragment ceiling from 8 MiB to 32 MiB. Apply the same exact bound in selection, independent fragment verification, Stage 0 publication requests, and the Windvale-written `WVPQ 1` planner. Retain the 34 MiB final-image ceiling, so a maximum-size fragment leaves a separately checked 2 MiB envelope for alignment and service leaves.
- Make the Windvale OS native-stack proof consume each function's ABI-21 projected frame cells from the shared record-storage planner. Probe 32's `Executeˉmain` grows from 745 to 755 cells and its deepest call path from 23,824 to 24,240 bytes, which still proves six pages are the minimal whole-page stack envelope.
- Rebuild the exact Probe-32 interpreter under ABI 21. Its WVO grows from 418,372 to 445,684 bytes and the linked client from 417,773 to 445,085 bytes, requiring 109 rather than 102 RX pages. Retain the 1,024-byte context record-arena extent as a dormant compatibility field and change its exact successful use from 528 to zero bytes.
- Advance kernel memory to `WVKMEM11`. The client memory budget becomes 118 pages, its reclaimable root becomes 120 pages, and the complete bounded kernel arena becomes 141 pages. Retain Probe 32, `WVPROC11`, its exact WVB, process/resource semantics, six stack pages, paging format 4, firmware format 32, and serial contract.
- Retain WVB, WVO, execution-context, service-table, publication-request, and publication-response format versions. The ABI target change and exact fragment digest prevent old selected fragments from being mistaken for ABI 21.

## Evidence

The small nominal program publishes the unchanged canonical Decision 0118 maps, including a 32-cell maximum projected frame. Interpreter, independently decoded W^X JIT, and linked WVO/AOT all return `42`. Corrupting the first planned construction target is rejected as `WVN3030`. A loop performs 131,072 record constructions, returns `131071`, and consumes exactly zero record-arena bytes.

The independent decoder distinguishes record pointers from borrowed descriptors in registers, outgoing stack cells, and incoming stack cells. Because both stack forms begin with a pointer-sized load/store, it accepts a descriptor only after reconstructing its complete canonical 32-byte two-word form; otherwise it admits the exact 16-byte record-pointer form. It reconstructs typed frame construction, owned and borrowed copies, field access, caller-owned destinations, direct-field returns, frame ranges, and record call-graph agreement.

The exact 328-function compiler retains the Decision 0118 map digest `aff287fba46a840e454e4cc7bf4751d3152474caf09331a526f3730ba280816e` and 1,489-cell maximum. ABI 21 selects a deterministic 16,905,513-byte fragment with SHA-256 `29a8b354e185fad4b4d8967ee8e263ce68cb9939373d91fb1e7919be887c8569`. It passes complete independent decoding, compiles the function-only fixture to the exact canonical 815-byte WVB, emits the existing success report without diagnostics, retains 4,340,388 bytes of dynamic text/byte use, and reduces record-arena use from 1,480,096 bytes to zero.

The full 12-module native bootstrap clears `WVR3017` and reaches the next independent boundary, `WVR3018`, at the retained 16 MiB dynamic text/byte arena. It produces no output module or diagnostic before that checked failure. ABI 21 therefore removes the measured record-lifetime blocker without claiming complete native self-reproduction.

The Windvale publication core remains 7,189 bytes with SHA-256 `f2c315c4c52099b8682396358563eef2eb9dceecf1feb84ce5bef5f8465bdeba`; the regenerated retained bridge remains 7,105 bytes with SHA-256 `b21e1136fc9087f530391127a1e1400e7248fa1831a51f00d86d467cf5133cb0`. Stage 0 and Windvale planners accept exactly 32 MiB and reject 32 MiB plus one while preserving the 34 MiB final-image boundary.

The rebuilt OS retains the exact 815-byte admitted WVB, 199 guest instructions, result `6`, two protected generations, typed resources, revocation/reuse, and fault behavior. Its current deterministic identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVO | 445,684 | `3840f10bacf8b7b498f28646b947a53841baf00241cd21bc94423ab5a43e8e31` |
| Linked normal client | 445,085 | `95a80e8998571dfb1cb8af7e2d14fd40804f648ce73e136e7aa00265934d7f30` |
| Linked fault client | 445,085 | `b1e64138a6f247b2806524e8ab0c9fd044e1a7111721f1e03b19c0fc3cf437bc` |
| Normal process-machine WVO | 473,134 | `785cb201783889d62777f1807cebfe346c16105cd4e3476485e99a27a010c130` |
| Fault process-machine WVO | 473,198 | `1824e69abe3a1a7b8cb224442e471de9c651f917cf0e7ac8c015aca8b41c6370` |

Immediately before the Decision 0131 WebAssembly and Decision 0132 console-verification rebases, the integrated native/console/OS candidate passed complete Windows Qualification in 518.6 seconds: a zero-warning Release build, all 77 Seed tests in 262.591 seconds including the 179.808-second golden compiler contract, all 31 OS tests, and the complete native CLI gate. Decision 0131 changes no native, console, or OS implementation/artifact input. After its rebase, focused WebAssembly, console, native-record, and all 31 OS checks passed. Decision 0132 retains the exact console bytes and changes no native or OS input; after its rebase, all four console checks and the native-record check pass. All four pinned Windows QEMU 11.0/Q35/TCG scenarios also pass with the rebuilt firmware:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| Normal | 555,520 | `d394eb13ae93b71450bb9a3fd42e3dee0aaa17c58c6c920f031eacaf9deb5e8d` | 0 |
| Invalid opcode | 555,520 | `ad77085ef6a9dbbd56dd938c3492748f363fb99c5719341387a6e87a75624ade` | 3 |
| General protection | 555,520 | `3a651d555f2c334c53b67385ac1fb731a513deb058e742ba5cbc58a985970de2` | 3 |
| Contained user fault | 556,032 | `198e01840c888d344e40ef985c574e33c77cdafccf36cdb56f9f9218bf74e8ca` | 0 |

Debian and exact committed-candidate GitHub qualification evidence is pending.

## Consequences

Record construction is now bounded by verified frame shape and call depth rather than aggregate construction count. Direct records die with their owning frame or block region, and caller-owned return copying prevents a callee frame pointer from escaping. The exact compiler's ordinary native execution no longer needs the host record arena.

The baseline code is intentionally large because every direct field copy is explicit and independently decoded. A later compact copy stencil, helper, or instruction-selection optimization may reduce the 16.9 MiB fragment, but must preserve nominal tags, exact widths, status behavior, reproducibility, and independent verification. The 32 MiB ceiling is a bounded admission decision, not a target size.

The next native self-reproduction blocker is dynamic text/byte lifetime, not records. It should be measured by allocation class and liveness before increasing the 16 MiB arena or selecting reclamation. WVO and flat-linker 4 MiB ceilings remain separate AOT-container work; this decision qualifies in-memory fragment selection and publication, not a standalone 16.9 MiB WVO.

The Windows and Linux console targets and Windvale OS now consume the same ABI-21 compiler path rather than retaining parallel ABI-20 backends. The ABI transition does not change scalar native fragments; independently integrated container hardening changes only the outer startup and publication contract. Nominal-record programs gain frame ownership and zero record-arena consumption. The OS guest contract also does not change, but explicit record copies increase the interpreter and firmware sizes enough to require seven additional RX and kernel-arena pages. That cost is visible, bounded, and reversible by later code-density work.

## Reconsider when

- Nested records require recursive backing, deep copies, or owned aggregate descriptors.
- Record references may escape a frame through globals, asynchronous work, closures, or a public FFI.
- Baseline code approaches 32 MiB or service leaves approach the remaining 34 MiB publication envelope.
- A compact verified copy mechanism materially reduces code without creating a second semantic path.
- Register allocation or a different calling convention changes the pointer, hidden-result, or frame-map contract.
- A broader Windvale OS program needs nested records, escaping record lifetimes, or materially different native storage.
