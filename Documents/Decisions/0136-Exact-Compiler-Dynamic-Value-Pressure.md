# Decision 0136: Exact-compiler dynamic-value pressure

- Date: 2026-08-03
- Status: Implemented with local Windows evidence; cross-host qualification pending
- Retains: Native ABI 21, execution-context version 7, target `x86-64-wvb-baseline-v21`, and the 16 MiB dynamic-value arena
- Refines: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md), and [Decision 0133](0133-Frame-Owned-Direct-Native-Records.md)

## Context

Decision 0133 removes monotonic record allocation from the exact native compiler. Its successful function-only compilation retains 4,340,388 dynamic text/byte bytes, but the complete 12-module native bootstrap reaches `WVR3018` at the fixed 16 MiB arena before stdout, diagnostics, or output publication.

The same canonical compiler succeeds in the reference runtime because its immutable `bytes` representation is a balanced tree rather than a flat copy per concatenation. The native ABI still represents every dynamic text/byte result as one contiguous pointer/length descriptor backed by an execution-scoped monotonic arena. Raising that arena without measuring the successful workload would hide either temporary lifetime or repeated-builder amplification.

## Decision

- Add opt-in semantic dynamic-value profiling to the Stage 0 reference runtime. The profiler attributes each result's flat UTF-8 or byte length to the function and exact allocation-bearing operation that constructs it.
- Cover the operations implemented by the ABI-21 dynamic arena: `enum.name`, signed and unsigned integer formatting, `text.concat`, `text.quote`, `bytes.concat`, and the one-, two-, and four-byte constructors. Borrowed constants, slices, `text.from_utf8`, and `text.to_utf8` do not count as allocations because the current native path preserves them as borrowed descriptors.
- Report both constructed-value count and constructed bytes through `Readˉfunctionˉdynamicˉvalues`. Return only positive function/class rows, ordered by descending constructed bytes, descending value count, function index, and class.
- Add CLI option `--report-function-dynamic-values`. It writes deterministic `Function dynamic-bytes=<bytes> values=<count> kind=<operation> index=<index> name=<name>` lines to standard error after success or failure. The default runtime allocates no profiling matrices and emits no profile output.
- Keep the 16 MiB native arena unchanged. Constructed bytes are a flat-copy workload measure, not a claim that all values are simultaneously live and not a proposed capacity.
- Do not select reclamation from aggregate allocation alone. The next slice must preserve dynamic-backing identity through descriptors, locals, calls, and direct-record fields, then measure which roots remain live when caller locals are replaced and frames return.
- Compare at least ownership-aware move/reuse, a bounded chunked or rope-like construction representation, and reclamation over typed roots. Any chosen mechanism must remain a general Windvale runtime contract shared by the compiler and OS path; it must not be a compiler-name special case or make mutable aliases observable.

## Local evidence

The opt-in reference profile executes the canonical 12-module Stage 1 compiler in exactly 6,700,562,174 instructions. It returns zero, emits the established success report, produces a byte-identical 599,868-byte Stage 2 compiler, and retains SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

The successful run constructs 1,852,773 allocation-bearing values representing 902,262,268 flat result bytes:

| Operation | Values | Constructed bytes |
| --- | ---: | ---: |
| `bytes.concat` | 962,611 | 899,106,127 |
| `bytes.from_u32_little` | 755,201 | 3,020,804 |
| `bytes.from_u8` | 134,925 | 134,925 |
| `text.concat` | 7 | 342 |
| `bytes.from_u16_little` | 25 | 50 |
| `u32.format` | 3 | 15 |
| `enum.name` | 1 | 5 |

`bytes.concat` accounts for approximately 99.65% of constructed bytes. The largest function/class rows are:

| Constructed bytes | Values | Function and class |
| ---: | ---: | --- |
| 315,298,984 | 1,640 | `Compilerˉsourceˉwirˉmergeˉfunction` / `bytes.concat` |
| 265,306,656 | 451,269 | `Compilerˉsourceˉwirˉemit` / `bytes.concat` |
| 102,543,288 | 671 | `Compilerˉcompileˉsourceˉwvb` / `bytes.concat` |
| 97,484,938 | 73,641 | `Compilerˉsourceˉwvbˉencodeˉfunction` / `bytes.concat` |
| 34,672,248 | 656 | `Compilerˉsourceˉbindingsˉphaseˉmerge` / `bytes.concat` |
| 23,648,876 | 4,870 | `Compilerˉsourceˉsymbolsˉcount` / `bytes.concat` |
| 13,443,916 | 15,162 | `Foundationˉbytesˉreplace` / `bytes.concat` |
| 11,240,964 | 3,822 | `Compilerˉsourceˉsymbolsˉlookup` / `bytes.concat` |

The leading merge function returns five newly concatenated byte payloads inside a direct record. Those results escape the callee and replace older caller payloads across the outer compiler loop. A callee-only arena reset therefore cannot solve this workload: the next model needs caller-visible ownership/liveness or a construction representation that does not copy the complete prefix on every append.

The fast runtime test proves disabled behavior, exact class attribution, deterministic ordering, and reset rather than accumulation across repeated runs. The Windows CLI gate uses the existing Foundation byte-construction demo to require three exact report rows. The focused Release runtime test passes in 0.521 seconds after a zero-warning build. The one intentionally long full-bootstrap profile completes locally in 426.148 seconds and reproduces the exact compiler identity above.

No WVB/WVO serialization, source semantics, native ABI, generated machine byte, native execution behavior, OS source, or guest artifact changes in this measurement slice. QEMU is not rerun because every OS input and identity remains unchanged.

## Consequences

The next native compiler blocker is no longer described only as a 16 MiB failure. The successful workload is overwhelmingly immutable byte-builder pressure, with both large escaping merges and high-count small appends. A fixed arena increase, a text-specific repair, or callee-only cleanup would not be a durable answer.

The profiler is a Stage 0 diagnostic seam. It measures the bytes a contiguous ABI-21 implementation would construct while leaving the reference runtime's balanced immutable representation and all program output unchanged.

This decision does not qualify native Stage 1-to-Stage 2 reproduction, choose a garbage collector or ownership system, change the observable `bytes` contract, retire .NET, or advance the OS guest ABI.

## Reconsider when

- Dynamic-backing liveness shows that a smaller safe region or ownership-transfer rule covers the exact compiler.
- A chunked representation cannot preserve bounded reads, slices, capability transfer, deterministic failure, and final contiguous publication.
- Descriptor-bearing nested records, concurrency, or long-lived graphs require a tracing boundary rather than acyclic ownership.
- The successful native bootstrap reaches a different bounded resource after dynamic storage is revised.
