# Workload 2 implementation responsibilities

## Rule

This matrix assigns each paper dependency to one repository owner. No row
authorizes implementation before Language 1.0 source freeze. Existing Seed
filesystem code remains implementation evidence and a possible oracle, not the
definition of these edition-1 contracts.

## Responsibility matrix

| Boundary | Primary owner | Work proven necessary | Source-language change? |
| --- | --- | --- | --- |
| Edition/module/profile/platform/authority metadata | Language grammar, parser, source graph | Parse four modules and preserve Core-to-Hosted imports plus exact capability closure. | No new syntax. |
| Typed configuration and result | Type checker, nominal values, launcher profile | Bind one exact record and retain one exact structured `Result` through teardown/status mapping. | No. |
| Byte buffer | Foundation bytes/memory, ownership lowering, runtimes/backends | Construct one zero-initialized fixed buffer from an owned budget and expose checked mutable/immutable slices. | No grammar change; accepted Foundation signatures. |
| Slice exclusivity and lifetime | Borrow checker, WIR evidence, diagnostics | Prove nonoverlap in time, one owner, no provider retention, and no escaping slice. | No. |
| `using` cleanup | Parser, ownership analysis, cleanup lowering | Release nested destination/source handles on return and `try` propagation without invoking finish. | Existing candidate rule clarified. |
| Explicit durable finish | Foundation resource semantics, destination capability, providers | Keep content/length/name durability fallible and distinct from local release. | No. |
| Source snapshot | Source capability catalog and providers | Acquire one bounded immutable snapshot or reject unsupported; detect later generation change. | Capability/library contract only. |
| Destination creation | Destination capability catalog and providers | Create exclusively without replacement, enforce maximum length, and preserve partial-object behavior. | Capability/library contract only. |
| Exact read progress | Platform filesystem library, runtime boundary, providers | Mutate only a proved target prefix; keep failure atomic; reject zero progress before EOF. | No. |
| Exact write progress | Platform filesystem library, runtime boundary, providers | Preserve rejection, known partial, completion, and indeterminate outcomes; permit suffix continuation only for short acceptance. | No. |
| Cancellation and provider lifecycle | Launcher profile, capability providers, bounded-operation owner | Bind cancellation generation; separate cancellation, loss, restart, and post-dispatch uncertainty. | No general token yet. |
| Capability approval and binding | Package/launcher plan, capability catalog, service manager | Approve and bind source and destination roots independently with exact limit profiles. | No ambient filesystem. |
| Host adapters | Windows/Linux provider owners | Translate semantic names and operations without exposing host paths or weakening no-link/durability rules. | No source change. |
| Windvale OS adapter | Filesystem service and IPC owners | Bind the same two roots to isolated service instances with generation-safe handles and bounded teardown. | No source change. |
| WIR/backend | Compiler lowering and native ABI owners | Lower ordinary loops, borrows, variants, checked arithmetic, cleanup edges, and provider calls. | No file-specific WIR operation. |
| Diagnostics | Compiler, runtime, provider, launcher | Retain phase/stage, position, expected/observed count, generations, limits, and copied prefix within fixed records. | No reflection or stack traces. |
| Editor tooling | Windvale editor/formatter | Classify existing edition-1 `using`, borrows, named calls, effects, and capability roots. | No grammar delta. |
| Verification | Focused Language 1.0 file-copy owners | Convert all valid, rejected, malformed, cleanup, progress, uncertainty, and cross-host cases into bounded fixtures. | No. |

## Accepted Foundation signatures

Decision 0756 accepts this exact `Foundationˉbytes` group:

```text
export fn Constructˉbuffer(
    Budget: Memoryˉbudget,
    Length: u64,
) -> Result<Byteˉbuffer, Allocationˉfailure>
    effects(memory.allocate);

export fn Bufferˉlength(
    Buffer: borrow Byteˉbuffer,
) -> u64 effects();

export fn Borrowˉslice(
    Buffer: borrow Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Slice<u8> effects();

export fn Borrowˉsliceˉmut(
    Buffer: borrow mut Byteˉbuffer,
    Start: u64,
    Length: u64,
) -> Mutableˉslice<u8> effects();
```

Construction consumes and either transfers or locally releases the budget. It
zero-initializes exactly `Length` bytes before publication. Both slice calls
validate checked start-plus-length and trap before forming an invalid borrow.
Their results are tied to the one buffer owner. No uninitialized safe value or
unchecked Core/Hosted access is admitted.

## Representation planning

### Front end and WIR

The source lowers through ordinary nominal records/variants, two nested resource
scopes, one fixed mutable byte allocation, checked loops, exact borrows, and
capability calls. Cleanup edges are explicit WIR control-flow evidence. No
filesystem opcode, exception table, garbage collector, native pointer, or
parallel compiler path is required.

### Provider boundary

Capability requests retain root identity, signature-set identity, limit profile,
provider generation, resource generation, operation kind, explicit position,
slice length, cancellation generation, and completion class. Runtime validation
occurs before any source-visible count or borrowed-buffer mutation is accepted.

### Host mapping

Windows and Linux leaves may use different native calls but must implement
semantic single-segment names, exclusive create, no-link admission, explicit
offsets, exact progress, combined finish durability, generations, and local
release. A provider that cannot prove one guarantee returns a typed rejection;
it does not approximate silently.

## Planned verification owners

| Owner | Initial cases |
| --- | --- |
| `language-1-paper-file-copy` | Candidate parse/name/type/effect/ownership cases and stable bundle identities. |
| `language-1-foundation-byte-buffer` | Construction, zeroing, budget failure, checked slices, alias rejection, release, and boundary lengths. |
| `hosted-filesystem-copy-source` | Name/limit admission, snapshot length/generation, short/EOF reads, source change, cancellation, loss, restart, and malformed results. |
| `hosted-filesystem-copy-destination` | Exclusive create, collision, capacity, complete/partial/indeterminate writes, finish classes, cancellation, and release. |
| `file-copy-launcher` | Typed configuration, exact root grants/bindings, cancellation generation, memory/resource ceilings, result retention, and status mapping. |
| `file-copy-differential` | Identical content, positions, progress/result transcript, and cleanup order across interpreter/native and Windows/Linux/Windvale adapters. |

These are planning labels, not current registry additions. An executable boundary
adds a focused owner only when implementation begins; paper documents must not
trigger unfiltered qualification.

## Implementation order after source freeze

1. Implement the accepted byte buffer and checked slice calls with a simple
   reference oracle.
2. Lower nested `using` plus early `try` propagation and validate cleanup edges.
3. Publish exact provisional source/destination capability catalog entries only
   after later workload reconciliation.
4. Implement a deterministic in-memory provider oracle covering every outcome.
5. Bind Windows and Linux no-link providers and combined durable finish.
6. Compile and run this exact application through interpreter and native paths,
   recording time, memory, WIR, WVB, native size, and call transcript.
7. Bind the same semantic contracts to the Windvale OS filesystem service.

Replacement, deletion, resume, metadata copy, asynchronous I/O, and general path
APIs remain separate later work.
