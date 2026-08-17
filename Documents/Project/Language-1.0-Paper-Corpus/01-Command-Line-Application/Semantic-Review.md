# Workload 1 semantic review

## Review status

Complete first-author review. The source is coherent under the candidate grammar
and the paper contracts in this bundle. General Foundation selections remain
recommendations until project-owner review; the standard-stream API remains a
paper-only hosted contract until implementation planning assigns its catalog
identity.

## Module metadata

| Module | Profile | Authority | Platforms | Required capabilities |
| --- | --- | --- | --- | --- |
| `Inspectˉtypes` | Core | Library | Windows, Linux, Windvale | None |
| `Inspectˉpackage` | Core | Library | Windows, Linux, Windvale | None |
| `Inspectˉarguments` | Core | Library | Windows, Linux, Windvale | None |
| `Inspectˉsummary` | Core | Library | Windows, Linux, Windvale | None |
| `Inspectˉapplication` | Hosted | Application | Windows, Linux, Windvale | `standard.diagnostic` v1, `standard.input` v1, `standard.output` v1 |

There are no optional capabilities, reverse profile imports, platform-specific
source branches, unsafe blocks, foreign declarations, or target extensions.

## Values and ownership

| Value | Class and owner | Movement or sharing | Terminal state |
| --- | --- | --- | --- |
| Launcher arguments | Shared immutable `Sequence<text>` charged to the application domain | Passed by value to entry; parser receives one immutable borrow; `Sequenceˉat` returns nonescaping borrows | Released with the application domain |
| Root budget | Move-owned `Memoryˉbudget` | Transferred by launcher, then held in mutable local `Rootˉbudget` | Released after every entry return |
| Diagnostic budget | Move-owned child budget, 256 bytes | Split before parsing; moved either into diagnostic construction or released unused | Consumed by diagnostic builder or locally released |
| Input budget | Move-owned child budget, 0 through 65,536 bytes | Split only for `Inspect`; moved once to `standard.input.Readˉtext` | Transferred to successful text backing or locally released by failed read |
| Text-builder budget | Move-owned child budget, 32 bytes | Split before input; moved once to `Text.Constructˉreserved` | Transferred through builder freeze or released on failure |
| Byte-builder budget | Move-owned child budget, 32 bytes | Split before input; moved once to `Bytes.Constructˉreserved` | Transferred through builder freeze or released on failure |
| Usage resource | Shared immutable package bytes, 73 bytes | Borrowed for help or one diagnostic append | Charged once until domain teardown |
| Parsed options | Copy enums, integers, records, and variants | Assigned only through visible `var` bindings; no allocation | Ordinary lexical destruction |
| Input text | Shared immutable strict text | Returned once by input provider and immutably borrowed by measurement | Released after output construction or earlier return |
| Text and byte builders | Move-owned bounded mutable values | Exclusively borrowed by append; consumed exactly once by `Freeze` | Immutable result owns retained backing |
| Rendered output | Shared immutable bytes | Borrowed for one provider mutation | Released after provider outcome |
| Capability roots | Module-bound singleton dependencies | Never stored, captured, copied, or returned | Launcher unbinds at domain teardown |

There are no owned provider instances, handles, files, streams, tasks, arenas,
maps, vectors, foreign pointers, or user-visible shared mutable values.

## Visible mutation

All mutation is confined to five places:

1. `Selectedˉoperation`, `Selectedˉmaximum`, and `Index` during bounded argument
   parsing;
2. `Rootˉbudget` while rights-reduced child budgets are split;
3. the reserved text builder while the label, decimal count, and LF are appended;
4. the reserved byte builder while UTF-8 or diagnostic bytes are appended; and
5. one external normal-output or diagnostic-output mutation.

No builder alias survives freeze. No external mutation occurs before all local
input, parsing, budget, and rendering checks for that terminal path succeed.

## Capability and effect closure

| Source boundary | Exact effects | Reason |
| --- | --- | --- |
| Argument parsing and measurement | Empty | Immutable local computation only |
| Budget splitting | `memory.allocate`, `resource.acquire` | Reserve rights-reduced accounting children |
| Reserved builder construction | `memory.allocate` | Commit the builder maximum within an owned child budget |
| Lexical cleanup | `resource.release` | Invalidate and credit owned local accounting |
| Input read | `memory.allocate`, `standard.input` | Retain one bounded strict text value from the approved provider |
| Normal write | `standard.output` | Attempt one exact normal-output mutation |
| Diagnostic write | `standard.diagnostic` | Attempt one exact diagnostic mutation |

The exported entry lists the union exactly. Capability module roots are not
closure captures or ambient state. A lower-profile module cannot call them.

## Recoverable failure families

### Argument failures

`Argumentˉfailure` distinguishes no arguments, illegal help combination,
unknown option, each repeated option, missing value, invalid operation, numeric
parse failure, application maximum rejection, and missing required operation.
The parser performs no output or allocation and preserves the exact failing
argument index as bounded internal evidence.

### Memory and rendering failures

`Memory.Split` and reserved builder construction return
`Allocationˉfailure`. Appends return `Limitˉfailure` and leave builders
unchanged. Rendering adapts those families explicitly to `Renderˉfailure`.
Source does not use result context or implicit exceptions to change error type.

### Input failures

The provider distinguishes maximum excess, first malformed UTF-8 offset,
provider rejection, and provider generation loss. Empty input and exact-maximum
input are successes. No rejected read exposes a prefix.

### Output outcomes

The standard Foundation `Mutationˉoutcome` preserves zero progress, exact
partial progress, exact complete progress, and indeterminate progress. The
application attempts one write and never retries. A provider that reports
`Completed` with a count different from the supplied value length maps to status
4 as a protocol failure.

## Terminal traps

Only verified-contract violations trap:

- checked integer arithmetic overflow in source;
- `Sequenceˉat` outside the admitted sequence;
- a compiler/runtime invariant violation after a successful reservation; or
- malformed runtime behavior that cannot construct one declared capability
  outcome.

The parser proves both `Index` and `Valueˉindex` before sequence access. Its
launcher maximum of 16 makes every `+ 1u64` operation arithmetically safe. A
valid provider result and valid builder result therefore require no expected
trap. A terminal process trap does not promise source cleanup; application-domain
teardown remains responsible for reclaiming all accounting.

## Cleanup walkthrough

### Prelaunch rejection

The launcher publishes no entry invocation. It releases any private argument,
budget, or provider-binding construction in reverse order. Source observes
nothing.

### Initial diagnostic reservation failure

`Rootˉbudget` remains live. Source attempts one static diagnostic write without
allocating. It returns status 6 if completely accepted, status 4 for rejection or
partial acceptance, and status 5 for indeterminate progress. The root then
releases.

### Argument failure

The diagnostic child already exists. No input or output-builder child exists.
Source consumes the diagnostic budget into one byte builder, appends one fixed
message and the package usage bytes, freezes it, attempts one diagnostic write,
releases the diagnostic bytes and root, and returns the mapped status.

If diagnostic construction fails, its partially constructed local owner and
child budget release before status 6 returns. Source does not allocate a second
diagnostic.

### Help

No input or output-builder child is created. Source borrows the package usage
bytes for one normal write. The unused diagnostic child and root release after
the provider outcome.

### Child-budget failure before input

Every already-created input/text/byte child releases in reverse split order.
The reserved diagnostic child is consumed to report one resource failure. The
input and normal-output providers have not been called.

### Input rejection, malformed UTF-8, or provider loss

The provider consumes and locally releases the input budget without exposing a
text value. Unused text and byte children release. Source consumes the separate
diagnostic child, writes once, then releases root state.

### Render failure

The valid input text remains immutable. Any constructed builder releases its
owned backing, and unconsumed child budgets release in reverse order. Source
uses the separate diagnostic child once; it does not reuse partially rendered
normal output.

### Successful output construction

Text builder freeze transfers its 32-byte backing to immutable text. UTF-8 append
copies the exact content into the byte builder; byte freeze transfers its
backing to immutable output. The provider borrows that output for one mutation.
After any terminal outcome, output, rendered text, input text, diagnostic child,
and root release in reverse lifetime order.

### Partial or indeterminate mutation

Source retains the provider's exact outcome long enough to select status 4 or 5.
It does not slice, retry, append the remainder, switch output channels, or claim
that local release changed external visibility.

## Cancellation and concurrency

This workload starts no task, owns no task scope, waits on no deadline, and has
no cancellation point. Every provider call is synchronous under this launcher
profile. That is an intentional zero bound, not omitted cleanup. Structured
concurrency remains owned by workloads 5, 6, 7, and 11.

## Common corpus review answers

- Every mutation and move-owned value is visible in source.
- Every borrow is local to one call or measurement and has one identifiable
  owner.
- All potentially growing values receive maxima and budgets before provider
  work.
- Capability requirements appear in the application header, entry effects, and
  package closure.
- `return` and `try` propagation release owned locals in reverse order.
- External mutation uncertainty remains an explicit outcome and is never
  retried.
- Argument evaluation, parsing, formatting, encoding, and field construction
  retain exact left-to-right order.
- Windows, Linux, and Windvale targets receive identical source semantics; only
  bound providers differ.
- Error adapters are explicit and localized to rendering and the application
  boundary.
- Records and variants carry domain meaning without packed offsets or reflection.
- Rejected ownership, effect, bound, capability, and profile cases have enough
  local evidence for bounded diagnostics.
- The source exposes four general Foundation signature gaps and no need for a
  new language production.
