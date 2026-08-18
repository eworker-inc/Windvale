# Workload 4 semantic review

## Ownership inventory

| Value | Owner/publication | Lifetime |
| --- | --- | --- |
| input bytes | launcher shared immutable | entry call |
| root and nine child budgets | move-owned entry values | transferred to phase owners or released on path exit |
| decoded text | shared immutable, source child charge | through parser/binder; final frame release |
| token vector | lexer owner | consumed into sequence before parser |
| node arena | parser owner | consumed into immutable arena before binder |
| declaration vector | parser owner | consumed into sequence before binder |
| handles | Copy, non-owning generation evidence | valid only with matching live mutable/immutable arena identity |
| binding map | binder owner | destroyed after canonical symbols are copied/shared |
| symbol/operation vectors | binder owners | consumed into immutable sequences before encoder |
| diagnostic sink | entry owner, mutably borrowed by phases | consumed exactly once into report |
| bytes builder | encoder owner | private until consuming freeze or destroyed on failure |
| report | immutable return | launcher domain |

Every mutation is through one `var` owner or exclusive borrow. Shared source/name
text never becomes mutable. No phase returns a borrow or mutable owner to its
consumer.

## Evaluation and cleanup

Budget splits occur in fixed phase order. A split failure releases earlier
children lexically. A source decode failure releases every unused child and
freezes one diagnostic report; physical decode allocation failure remains a
typed outer failure. Lexer/parser/binder failures destroy all mutable and
immutable phase owners built so far. A diagnostic-only result continues within
bounds but skips encoding. Encoder limit failure destroys private partial bytes,
appends one diagnostic, and returns no artifact. Successful freeze transfers
output accounting once.

No `try`, recursive return, recovery loop, limit, or builder failure bypasses an
owned value. There is no resource with fallible completion and no external
mutation.

## Effects and authority

The transitive effect closure is `memory.allocate`, `resource.acquire`, and
`resource.release`, arising only from rights-reduced child budgets. Capability,
I/O, time, entropy, task, unsafe, and provider effects are empty. Compiler input
and output are typed values supplied by the launcher, not ambient files or
standard streams.

## Quantitative source/compiler plan

The bundle has eight source modules, 2,415 physical source lines, 73 top-level
declarations, and 78,109 source bytes. `Compile` is the largest function at 245
physical lines. `Limits` is the widest record at eleven fields. The paper build
admits at most 256 generic instances, depth 32, 4,096 functions, 65,536 WIR
blocks, 1,000,000 WIR operations, 64 MiB retained compiler evidence, and a
16 MiB WVB candidate. These are conservative planning ceilings, not
implementation measurements.

The source itself expects seven collection specializations, one text-ordering
protocol selection, one recursive node nominal type, and ordinary Option/Result
instances. No monomorphization should duplicate lexer/parser bodies per runtime
capacity because capacities are owner values, not type arguments.

## Failure and trap boundary

Structured diagnostics cover malformed source and admitted output limits. Typed
outer failure covers invalid configuration, child allocation, collection
capacity/corruption, work exhaustion, and impossible builder infrastructure.
Terminal traps remain post-proof programming defects: checked sequence/rank
access outside a proved count, validated-borrow precondition violation, use after
move/freeze, borrow conflict, or impossible checked arithmetic under admitted
limits. Untrusted bytes/handles never reach those calls without validation.

## Usability conclusion

The longest routines are orchestration and parser recovery, not manual packed
record manipulation. The successful path reads as decode → lex → parse → bind →
encode. Named variants make diagnostics and recursive node shape local. Explicit
generic constructors are used only where an empty typed owner has no value
argument; normal operations remain inferred. This is a reasonable ownership
surface for compiler code without classes, GC, exceptions, or unsafe escapes.
