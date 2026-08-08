# Decision 0357: Windvale-owned native text-concatenation construction

- Status: Accepted current-host ownership transfer; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0071](0071-Native-Text-Arena-And-Core-Text-Services.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0071 moved text concatenation out of a managed callback and into one
exact x86-64 leaf shared by Windows and Linux, but a C# method still emitted
that leaf for every normal service bundle. The already qualified leaf checks
the combined 1 MiB value limit, reserves one contiguous arena range, copies
both borrowed inputs, and publishes an exact descriptor or failure detail.

The 249-byte leaf is a descendant input to many pinned applications. This
ownership transfer therefore preserves its exact bytes rather than selecting
an equivalent instruction sequence.

## Decision

- Add a focused portable Windvale generator for the unchanged
  `Textˉconcat` leaf and a capability-free byte-result bridge.
- Add one reusable Windvale x64 service-code builder that owns byte emission,
  bounded branch-patch records, and two's-complement relative displacements.
  Keep service semantics and label layout in the service-specific module.
- Use the shared builder for this and later service transfers without
  rewriting already retained UTF-8 or integer-format sources merely to share
  code; their pinned WVB identities remain unchanged.
- Require an ordinary Project 1 closure to reproduce the exact bridge through
  the native source front door.
- Retain the bridge WVB in the runtime. The C# recovery wrapper may verify,
  lower, execute, identity-check, and cache it, but no longer contains the
  text-concatenation emission algorithm.
- Preserve the native ABI, service-table slot, arena offsets, failure details,
  leaf placement, and every descendant bundle/application identity.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Shared Windvale service-code builder WVB | 4,135 | `adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06` |
| Windvale text-concatenation core WVB | 10,253 | `6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73` |
| Retained text-concatenation bridge WVB | 10,232 | `87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08` |
| `Textˉconcat` leaf | 249 | `75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0` |

## Evidence and consequences

Stage 0 first compiled the new closure for precise diagnostics. The ordinary
native source front door then published the same 10,232-byte WVB with the same
digest. After reviewing the focused test against this ownership change, the
affected Release solution built with zero warnings and errors in 35.49
seconds. The single named case passed 1/1 in 2.331 seconds.

The focused test pins all three source results, compares the retained WVB byte
for byte, rebuilds it through the native source front door, verifies the
unchanged leaf identity, and requires the reference interpreter and verified
x64 backend to return the same 249 bytes. Existing dynamic-text coverage
remains the qualified semantic evidence for successful copying, the combined
value limit, arena exhaustion, and mixed allocation; it was not rerun for this
exact-byte ownership transfer.

The final qualification scripts now reproduce and inspect the shared builder,
core, and bridge and compare the retained bridge exactly. Only their
PowerShell and shell syntax is checked in this slice. C# still constructs
enum-name and quoting leaves and still owns retained-WVB loading, native
lowering, W^X execution/publication, runtime arenas, service-bundle
construction, and host-container orchestration. Linux execution and the
grouped broad gate remain deferred.

## Reconsideration triggers

Replace the retained bridge when native service-bundle construction can consume
the Windvale result without the managed loader/executor. Change the leaf
identity only through a new explicit runtime contract and complete descendant
qualification, not as an incidental generator refactor.
