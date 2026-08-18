# Workload 10 review findings

## Status

First-author review is complete. The project owner authorized direct acceptance
of all recommended correctness/completeness findings on 2026-08-17; all seven
findings are accepted under
[Decision 0764](../../../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md).
They are normative-candidate/source-freeze inputs, not implementation or final
freeze claims.

## Finding 1: System ABI modules need a concrete target predicate

`platform linux` is too broad for a SysV AMD64 foreign declaration. Accept
`linux.x86_64.sysv_amd64_c_v1` as the first concrete System ABI registry key.
Target-specific adapters remain separate modules over portable Core logic; no
preprocessor or current-host selection is added.

## Finding 2: the ABI literal names a registered complete contract

The existing foreign-declaration grammar is sufficient only if its first
literal resolves to an immutable contract fixing architecture, address width,
calling convention, scalar/pointer layout, byte order, ownership/retention,
alignment, symbol scope, error boundary, unwind, and target predicate. Accept
that rule. Do not infer any field from the compiler host or symbol spelling.

## Finding 3: nullable and non-null foreign pointers are distinct

Accept opaque non-null `Foreignˉpointer<T,Abi>` and distinct
`Nullableˉforeignˉpointer<T,Abi>`. Neither is an integer or safe reference.
Nullable validation is named and unsafe; a non-null result still needs range,
alignment, lifetime, alias, initialization, and access proof before dereference.
There is no implicit `null` or pointer-sized portable integer.

## Finding 4: caller-owned foreign scratch needs exact Foundation calls

Accept bounded aligned zero-initialized `Foreignˉscratch<Abi>`, checked exclusive
write-region construction, borrow-tied pointer extraction, region length, and
post-region safe slice observation. Region construction performs relative and
native address-width arithmetic, bounds, alignment, live-generation, and alias
checks before pointer publication.

## Finding 5: unsafe memory contracts and untrusted data are different

Accept recoverable validation of returned status, length, bytes, enums,
Booleans, generations, and format geometry. A callee write outside the admitted
region, forbidden retention/use-after-return, ABI corruption, or forbidden
unwind may already destroy process integrity and is terminal containment, not a
safe typed failure. This distinction must be explicit in every ABI contract.

## Finding 6: status translation and unwind remain adapter-owned

Accept the exact one-call i64 outcome contract and no-retry behavior. Negative
status maps explicitly; nonnegative length converts exactly and is range-checked;
stale status retains expected/observed generation. Recoverable foreign failure
does not unwind, and foreign unwind never crosses a safe Windvale frame.

## Finding 7: safe publication contains no System value or new authority

Accept the Core decoder and independent payload copy as the publication gate.
The report imports only Core modules. System/unsafe enables the audited call but
does not grant filesystem, network, device, process, clock, entropy, allocator,
or other authority; a real adapter declares those capabilities separately.

## Quantitative record

| Measure | Recorded value |
| --- | --- |
| Source | 5 files; 943 lines / 30,309 UTF-8 bytes; 44 top-level declarations; largest 289 lines. |
| Unsafe surface | 1 concrete target / 1 ABI contract / 1 foreign declaration / 1 unsafe value block / 1 call. |
| Memory | 64-byte aligned scratch; 4-byte copied payload; three 4,096-byte child budgets. |
| Record | 24 bytes / 10 validated fields or geometry rules / one SHA-256. |
| Report | 62 UTF-8 bytes / 5 LF-terminated lines / one SHA-256. |
| Failure surface | exact compile/target/pointer/status/record/containment cases in the 58-case rejected corpus. |
| New general surface | 1 target key, registered ABI rule, nullable pointer kind, 7 pointer/scratch/region operations; no capability or general pointer arithmetic. |

## Owner resolution

The owner accepted all seven recommendations. Workload 10 is draft reviewed,
so every mandatory paper workload now has owner-reviewed findings. No current
compiler/runtime/backend/ABI implementation, performance result, or source-
freeze claim follows.
