# Workload 4 package and build plan

## Mapping

Package identity: `windvale.paper.compiler_front_end.v1`.

| File | Module | Profile | Authority |
| --- | --- | --- | --- |
| `Source/Front-End-Types.wv` | `Frontˉendˉtypes` | Core | library |
| `Source/Front-End-Work.wv` | `Frontˉendˉwork` | Core | library |
| `Source/Front-End-Diagnostics.wv` | `Frontˉendˉdiagnostics` | Core | library |
| `Source/Front-End-Lexer.wv` | `Frontˉendˉlexer` | Core | library |
| `Source/Front-End-Parser.wv` | `Frontˉendˉparser` | Core | library |
| `Source/Front-End-Binder.wv` | `Frontˉendˉbinder` | Core | library |
| `Source/Front-End-Encoder.wv` | `Frontˉendˉencoder` | Core | library |
| `Source/Front-End-Application.wv` | `Frontˉendˉapplication` | Core | application |

Platforms are Windows, Linux, and Windvale. Required/optional capabilities are
empty. Entry is the exact monomorphic `Frontˉendˉapplication.Compile` signature.

## Reference limits

| Limit | Value |
| --- | ---: |
| source bytes/runes | 65,536 / 65,536 |
| tokens including End | 8,192 |
| AST nodes | 4,096 |
| declarations/symbols | 512 / 512 |
| bound operations | 16,384 |
| diagnostics | 16 |
| parse/traversal nesting | 64 |
| total runtime work | 200,000 |
| output bytes | 262,144 |
| compile-time generic instances/depth | 256 / 32 |
| tasks, queues, capabilities, unsafe blocks | 0 |

The build plan may lower positive runtime limits without changing semantics.

## Root memory plan

The launcher supplies one 8 MiB root budget admitting nine children. The entry
splits exactly:

| Child | Bytes |
| --- | ---: |
| decoded source | 262,144 |
| diagnostics | 131,072 |
| tokens | 786,432 |
| nodes | 1,048,576 |
| declarations | 262,144 |
| binding map | 262,144 |
| canonical symbols | 262,144 |
| operations | 1,048,576 |
| output | 524,288 |
| total child maxima | 4,587,520 |

The remaining root authority is not allocated implicitly. Every child returns
on failure or after its owner/publication backing dies. Immutable source slices
and names may share one decoded backing and one retained charge.

## Generic/protocol plan

Explicit empty constructors instantiate `Vector<Diagnostic>`, `Vector<Token>`,
`Vector<Declaration>`, `Arena<Node>`, `Map<text,Binding>`,
`Vector<Symbolˉrecord>`, and `Vector<Boundˉoperation>`. Later calls infer their
parameters from explicit collection/value arguments. The map selects the one
visible Foundation `Ordering<text>` implementation. No overload, protocol
search for types, result-context inference, or import-order selection occurs.

## Artifact plan

The paper `WVFE 1` output is test evidence only. Compiling these eight source
modules should lower through ordinary records, variants, loops, recursion,
generic specialization, protocol calls, handles, builders, and immutable
values. It needs no compiler-front-end-specific WIR opcode. Actual edition-1
implementation must record WIR/WVB/object sizes and reuse the single shared
backend architecture.

There is no package data, schema digest, installer content, or shipped payload
in this workload.
