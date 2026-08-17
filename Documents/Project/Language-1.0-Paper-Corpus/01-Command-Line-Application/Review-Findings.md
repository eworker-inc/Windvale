# Workload 1 review findings

## Status

Draft-reviewed under
[Decision 0755](../../../Decisions/0755-Resolve-Language-1.0-Command-Workload-Findings.md).
The workload passes its paper acceptance criteria. Its four general Foundation
signature groups, independent standard-stream authority split, and command
launcher status policy are accepted. The stream operation identities remain
provisional pending later I/O workloads.

## Acceptance matrix

| Required pressure | Evidence | Status |
| --- | --- | --- |
| Edition/module/profile/platform/authority metadata | Five complete modules with one Core-to-Hosted import direction and exact target scopes. | Pass on paper |
| Named arguments and configuration records | Fixed named calls and one two-field `Configuration` record; option traversal is deterministic. | Pass on paper |
| Strict numeric parsing | Type- and policy-specific whole-input decimal `u64` parsing; overflow and application maximum remain separate. | Pass on paper; normative-candidate signature accepted |
| `Option`, `Result`, `try`, and domain variants | Optional parser state, exact recoverable failures, explicit adapters, and no catchable exceptions. | Pass on paper |
| Text and byte builders | Reserved construction, atomic appends, invariant decimal formatting, canonical UTF-8 encoding, and consuming freeze. | Pass on paper; normative-candidate signatures accepted |
| Bounded immutable package usage | One exact 73-byte object, digest, type, charge, and shipped content identity with no filesystem grant. | Pass on paper |
| Explicit output capability | Independently bound normal and diagnostic roots returning four-state mutation outcomes. | Pass on paper; authority split accepted, stream identities provisional |
| No ambient environment or locale | Launcher-bound arguments/budget, invariant numeric text, strict UTF-8, and no environment/current-directory/host encoding access. | Pass on paper |
| Deterministic exit mapping | Six closed status members and exact precedence for complete, partial, rejected, inconsistent, and indeterminate writes. | Pass on paper; command-profile policy accepted |
| Required boundaries | Launcher maxima, every argument failure, numeric overflow, input maximum, malformed UTF-8, output rejection, partial progress, and uncertainty are recorded. | Pass on paper |
| Adding one unrelated option | Calls resolve by declared name and exact argument types; no overload or result-context choice can change. | Pass on paper |

## Main design findings

### Ordinary launcher arguments are sufficient

The entry needs no `argv` keyword, special `Main`, process global, command-line
reflection, or argument capability. A launcher-bounded `Sequence<text>` is easy
to parse and leaves quoting, executable naming, native pointer layout, and host
encoding outside source semantics.

The missing piece is not syntax; it is an exact borrowed sequence length/access
surface. Both operations satisfy Decision 0754 because `T` is uniquely derived
from the explicit sequence argument.

### Strict parsing needs a type- and policy-specific name

The Foundation direction listed all parsing policies but did not yet supply a
call that source can resolve without result-context inference. The source reads
more clearly with `Parseˉu64ˉdecimalˉwhole` than with a broad parser plus a large
policy record for this common case. The name fixes destination, radix, sign,
separator, whitespace, and whole-input policy; only the input byte ceiling is a
runtime argument.

This is a generated Foundation matrix member, not an overload or a special CLI
intrinsic.

### Reserved builders make allocation timing understandable

The application must be able to reserve diagnostic capacity before input and
reserve complete output capacity before provider work. A reserved constructor
cleanly separates recoverable physical allocation from later all-or-nothing
capacity checks. Append remains fallible for a programmer-selected insufficient
maximum, but it cannot discover physical allocation failure after external input
has already been accepted.

The text builder handles invariant number formatting; the byte builder owns the
explicit UTF-8 boundary and exact provider bytes. Using both is somewhat longer
than a host string interpolation call, but it keeps maximum, encoding, memory,
and mutation behavior visible and reusable.

### Three small stream capabilities are preferable to a console object

Input, normal output, and diagnostics are materially separate grants. Keeping
them separate supports redirected input, pipes, files, terminals, tests, and
Windvale OS providers without granting terminal control or filesystem access.
The source does not need an owned stream instance for this one-shot workload.

The four-state Foundation mutation outcome is essential. A Boolean or exception
would hide partial and indeterminate visibility.

### Output failure should override a hidden primary diagnostic status

When argument or input handling fails but its diagnostic cannot be completely
accepted, returning only status 2 or 3 would claim the planned diagnostic path
succeeded. The paper source instead returns status 4 for rejected/partial
diagnostic acceptance and status 5 for uncertainty. A completely accepted
diagnostic preserves the primary status.

This is one named command-launcher policy, not a universal rule for services or
structured completion records. It keeps the simple process status honest while
the source retains the primary error until the diagnostic outcome is known.

## Owner resolutions

### 1. Exact sequence observations accepted

`Sequenceˉlength<T>` and borrowed checked `Sequenceˉat<T>` are added to the
normative-candidate Foundation collection surface with argument-derived generic
resolution and the one-owner borrowed-result rule.

### 2. First exact strict numeric parser accepted

`Parseˉu64ˉdecimalˉwhole(Value, Maximumˉinputˉbytes)` is accepted with ASCII digits,
no sign/prefix/separator/whitespace, whole-input consumption, byte-first limit
checking, exact offsets, and checked overflow.

### 3. Text observations and reserved builders accepted

Exact `Byteˉlength`, `Runeˉcount`, reserved text/byte builder constructors,
atomic append calls, invariant `u64` decimal append, UTF-8 append, and consuming
freeze are accepted as recorded in [Command-Contract.md](Command-Contract.md).

### 4. Three-capability paper split retained

`standard.input`, `standard.output`, and `standard.diagnostic` remain separate
version-1 hosted capability candidates into workloads 2, 5, and 6. Do not freeze
their signature-set identities until those streaming, cancellation, and
provider-loss workloads confirm the boundary.

### 5. Command status precedence accepted

For launcher profile `windvale.launch.command.v1`, preserve the six statuses and
let incomplete or uncertain diagnostic mutation override the primary argument,
input, or resource status. Never retry and never switch channels after a
non-complete mutation.

## Quantitative review record

| Measure | Recorded value |
| --- | --- |
| Source size | 5 modules, 748 lines, 19 functions, 2 records, 2 enums, 3 variants, 1 package-data declaration. |
| Maximum source width | 3 function parameters; 2 record fields; 10 cases in the largest variant; largest module 331 lines. |
| Explicitness | 3 capabilities, 4 child budgets, 2 mutable builders, 1 input call, at most 1 output mutation, 0 tasks, and 0 provider-instance resources. |
| Resources | 98,304 root bytes; 16 arguments/2,048 aggregate bytes; 65,536 input bytes; 32-byte text and byte outputs; 256 diagnostic bytes. |
| Failure surface | 10 argument cases, 4 input cases, allocation/limit rendering, 4 mutation outcomes, 6 process statuses, terminal invariant traps. |
| Compiler planning | Planned ceilings of 32 generic instances, 256 WIR blocks, 2,048 WIR operations, 16 call-depth units, 32 compiler diagnostics, and 512 KiB retained compiler evidence. |
| Artifacts | 73 package-resource bytes; WVB/native sizes unknown until implementation; no new source syntax, WIR family, or backend format is shown. |
| Usability | Successful flow is linear. One shared reporting helper removed repeated diagnostic construction while error-to-message and capability boundaries remain explicit. |

The compiler-planning values are admission ceilings for a future executable
fixture, not measurements or expected exact output. Implementation must record
actual tokens, parse/bind/type time, generic instances, WIR blocks/operations,
retained evidence, WVB/native bytes, elapsed time, and peak memory.

## First-author revisions

1. Selected a strict UTF-8 input summary rather than file copy, leaving owned
   files and uncertain durable completion to workload 2.
2. Reduced operations to byte and rune count so line-ending policy would not
   enter the first command contract.
3. Excluded executable identity from the semantic argument sequence.
4. Reserved diagnostic capacity before parsing and all output capacity before
   input provider work.
5. Kept normal and diagnostic capabilities independently bound.
6. Used one four-state mutation mapper and prohibited every retry/fallback path.
7. Consolidated four repeated diagnostic builders into one explicit reporting
   helper without hiding error classification or authority.
8. Made package usage an exact bytes object so provider output has no implicit
   host encoding step.

## Owner resolution

The project owner accepted all five recommendations through Decision 0755 with
no source revision required. The general Foundation calls and command profile
are normative-candidate contracts. The three stream capability signatures remain
provisional until workloads 2, 5, and 6 test their wider I/O boundary.
