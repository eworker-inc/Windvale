# Decision 0163: Bounded hosted compiler runtime data

- Date: 2026-08-03
- Status: Qualified on Windows and digest-pinned Debian
- Adds: fixed paired format-3 initial runtime headers and RW/NX layout plans
- Retains: ABI 22, context 7, service table 5, exact native/service bundles, standard 4 MiB behavior, and all existing PE/ELF bytes

## Context

Decision 0161 fixes the exact compiler's six capabilities and ten service leaves, but a service bundle alone cannot run. Startup must own arguments, console and diagnostic channels, file snapshots, file input/output scratch, the record arena, and the 64 MiB dynamic-value arena. Auditing the qualified native Stage 1 proof also found that the ordinary one-million-instruction default is insufficient: the exact compiler is intentionally executed under an eight-billion-instruction bound.

Choosing these placements inside separate PE and ELF writers would risk host drift and make malformed evidence redundant. The next step is one checked platform-parameterized runtime plan and one initial header contract consumed by both outer formats.

## Decision

- Extend `WVHA 1` to serialize the exact 24,000,000,000 instruction ceiling at header bytes 88 through 95. Keep call depth fixed at the qualified default 1,024. This bound preserves 20% headroom above the observed failure at 20,000,000,000 while native Stage 2 reproduction succeeds under 24,000,000,000.
- Use one 4,096-byte file-backed runtime header containing context 7, service table 5, output table 1, file-input table 1, file-output table 1, and the complete `WVHA 1` record.
- Keep every runtime-derived pointer and every service/platform function pointer zero in the initial header. Startup may bind only the planned regions and canonical platform functions.
- Retain the qualified hosted limits: 67 arguments, 4 KiB per argument, 64 KiB aggregate argument bytes, 64 file snapshots, 1 MiB per file name, 4 MiB per file value, a 2 MiB record arena, and a 64 MiB dynamic text/byte arena.
- Allocate 64 fixed 1 MiB name strides and 64 fixed 4 MiB data strides. This preserves exact existing file-leaf semantics without host allocation callbacks or a new file service.
- Use two 2,097,154-byte UTF-16 path scratches on Windows and two 1,048,577-byte UTF-8 path scratches on Linux. Page-align each later scratch and the final extent.
- Bound the complete RW/NX runtime mapping below 512 MiB. Require checked arithmetic, exact offsets, target-specific platform identities, zero initial state, exact metadata and bundle agreement, and zero reserved bytes.
- Extend the existing exact-compiler AOT transport case. Compile the compiler once, build both bundles once, then check both metadata records and runtime headers through shared mutation loops rather than adding another compiler or malformed-input suite.

## Local evidence

The focused case passes after a zero-warning Release build. Both plans share argument descriptors at 4,096, argument bytes at 5,168, snapshots at 70,704, records at 73,728, text at 2,170,880, names at 69,279,744, data at 136,388,608, and input scratch at 404,824,064.

| Target | Complete RW/NX bytes | Initial-header SHA-256 |
| --- | ---: | --- |
| Windows x64 | 409,026,560 | `5d61f926461fc19e46e04a7e5dd3636fcbaa554e30370fc10a5eeb7992f5e634` |
| Linux x64 | 406,929,408 | `ee0e58ef5c82f65a48150f886ce7349753bb0af05145c46dafae000eff576c4a` |

The verifier checks all fixed context and table fields, the eight-billion budget, both arena sizes, target identities, file bounds, initial zero pointers, reserved bytes, exact `WVHA 1`, and actual service-bundle input. Mutated budget, output flags, snapshot capacity, adapter metadata, and cross-target inputs fail closed.

This is focused local Windows construction and verification evidence. GitHub's independent Windows and digest-pinned Debian Qualification jobs remain responsible for cross-host identity.

## Cross-host evidence

Exact descendant `db20fefaa3333b7b78392ba12141d1ae2b6bb0c2` passes GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 87 Seed tests including the golden compiler contract, all 38 OS tests, and the native CLI gate. Both platform plans retain their exact sizes and initial-header identities.

## Successor local candidate evolution

Decision 0184's intermediate language-evolution candidate first exceeded the retained 64 MiB dynamic-value arena. The completed coherent batch has since exceeded that intermediate 80 MiB candidate. [Decision 0201](0201-Expanded-Exact-Compiler-Native-Capacity.md) owns the current measured 104,885,093-byte peak, 128 MiB ordinary/version-2/3 arena, and 48,000,000,000-instruction ceiling without changing the 32-bit context field, ABI 22, context 7, service table 5, individual value limits, or the narrow version-1 16 MiB containers. This section is a successor pointer; the qualified 64 MiB evidence above remains historical evidence for its exact compiler.

The current runtime planner retains every other limit and moves only regions after the dynamic arena. The text, name, data, and input-scratch offsets are 2,170,880, 136,388,608, 203,497,472, and 471,932,928. Windows uses output scratch at 474,034,176 and a final 476,135,424-byte extent; Linux uses output scratch at 472,985,600 and a final 474,038,272-byte extent. Their initial-header SHA-256 values are `127cee36736a40b3825757cc5a831b83e36373cb52f7fc57a6d487da4aa1784b` and `69612958cb82aba4334f6243847441bdaf280b0762bbccb64419fc8688e1c442`, respectively.

Focused Windows construction, malformed-input verification, native self-reproduction, and direct raw-PE execution pass locally. Independent Debian construction and raw-ELF execution remain pending, so this section is candidate evidence and does not rewrite the 64 MiB cross-host result above.

## Consequences

The paired startup implementations now have one exact data contract. Windows must populate standard handles and canonical API pointers; Linux retains descriptors 1 and 2 plus direct syscalls. Both must validate and populate argument descriptors, bind all ten service pointers and five context table pointers, and invoke the unchanged compiler entry within the serialized budget.

The roughly 409 MiB virtual extent is a security ceiling over demand-paged RW/NX storage, not expected committed use. File-data capacity dominates at 256 MiB and file-name capacity at 64 MiB because the existing leaf guarantees 64 immutable maximum-sized snapshots. A later measured segmented allocator may reduce virtual address cost without changing application-facing semantics.

Decision 0201 raises the largest planned extent to 476,135,424 bytes, still below the fixed 512 MiB ceiling. It remains a checked admission bound rather than expected committed use.

This decision does not yet produce PE/ELF files, write startup code, bind platform functions, directly execute the compiler, reproduce Stage 2 outside .NET, or satisfy the native-retirement gate.

## Reconsider when

- A platform loader cannot reserve the bounded RW/NX extent without eagerly committing it.
- Live startup needs an unrepresented table, limit, or platform function.
- Measured compiler input needs fewer snapshot strides and a smaller compatible file-leaf contract is accepted.
- Windows and Debian do not reproduce the pinned initial headers.
