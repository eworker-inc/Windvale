# Workload 10 implementation responsibilities

## Ownership matrix

| Contract | Primary owner | Required implementation evidence | Not a language feature |
| --- | --- | --- | --- |
| Concrete target scope | target registry/build admission | exact descriptor match and unsupported rejection | filename/host inference |
| ABI registry identity | ABI catalog/compiler/linker | calling convention, layout, retention, unwind, symbol binding | current-host default |
| Foreign declaration/call | parser/type/effect/WIR/backend | exact scalar/pointer signature, visible unsafe call, thunk bytes | C header import/variadics |
| Pointer kinds | System type checker/Foundation unsafe | non-null vs nullable, no integer conversion/serialization | implicit null/pointer-sized integer |
| Aligned scratch | Foundation memory/unsafe/runtime | exact extent/alignment/zeroing/budget/release | ambient native allocator |
| Write region | checked arithmetic/borrow checker | address-width, range, alignment, generation, exclusivity, no escape | unchecked pointer addition |
| Status translation | System adapter | exact i64 mapping, no retry, stale evidence | exception translation |
| Record validation | Core Foundation/application | complete field/range/malformed corpus and owned copy | packed source record layout |
| Unwind/corruption containment | ABI thunk/runtime/test harness | isolated terminal cases, no safe-frame unwind | recoverable `Result` fiction |
| Authority | build/capability/system review | prove paper interface grants none; require grants for real domains | unsafe-as-capability |
| Verification | focused Language 1.0/System owners | compile, reject, isolated ABI, exact record/report/hash | paper-only pass claim |

## Likely WIR/backend work

System source needs typed WIR evidence for unsafe regions, foreign signatures,
ABI identity, call effects, pointer lifetime, and target scope. Native lowering
needs one exact SysV AMD64 thunk and verifier rules preventing an unsupported ABI
from reaching another target. Safe Core decode/report should remain ordinary
WIR and produce the same output under interpreter/reference execution when fed
the exact bytes directly.

`Foreignˉscratch`, region, and pointer may be compiler/runtime intrinsics, but
their public ownership/failure contract remains Foundation. No general pointer
opcode is exposed to Core/Hosted WVB. An unsafe WIR form must retain enough
evidence for verification; stripping source names cannot strip ABI identity,
range/lifetime facts required for safety, or diagnostic bounds.

## Verification slices after source freeze

1. grammar/editor System, unsafe block, and foreign declaration cases;
2. profile/import/target/ABI/symbol admission and rejection;
3. pointer kind, integer conversion, null, range, alignment, alias/lifetime tests;
4. scratch allocation/zeroing/accounting/release tests;
5. isolated conforming shim status/length/stale cases;
6. Core record malformed/boundary corpus and owned-copy proof;
7. exact record/report/hash comparison through direct and FFI paths;
8. isolated forbidden unwind/write/retain containment cases;
9. unsupported-target WIR/backend/link rejection.

One passing broader gate subsumes narrower checks on an unchanged tree. Never
run a deliberately corrupting shim inside the verification coordinator process.

## Performance record

Implementation must measure adapter call/thunk overhead, scratch construction
and zeroing, decode/copy/report time, peak/transient/retained bytes, compiler
unsafe-proof phase time, WIR/WVB/native size, and isolated-test launch cost.
Optimization may reuse admitted aligned storage or inline validation only while
ownership, target/ABI identity, exact outputs, containment, and the simple Core
oracle remain unchanged.
