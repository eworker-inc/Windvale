# Workload 10 package and execution plan

## Module mapping

Package identity: `windvale.paper.language1.foreign_record` version 1.

| File | Module | Profile | Target scope | Authority |
| --- | --- | --- | --- | --- |
| `Source/Foreign-Record-Types.wv` | `Foreignˉrecordˉtypes` | Core | windows, linux, windvale | library |
| `Source/Foreign-Record-Decode.wv` | `Foreignˉrecordˉdecode` | Core | windows, linux, windvale | library |
| `Source/Foreign-Record-Report.wv` | `Foreignˉrecordˉreport` | Core | windows, linux, windvale | library |
| `Source/Foreign-Record-System.wv` | `Foreignˉrecordˉsystem` | System | linux.x86_64.sysv_amd64_c_v1 | system |
| `Source/Foreign-Record-Application.wv` | `Foreignˉrecordˉapplication` | System | linux.x86_64.sysv_amd64_c_v1 | application |

The build plan selects exported `Foreignˉrecordˉapplication.Run`, binds the
exact registered ABI contract and exact symbol, verifies the concrete target,
and transfers one owned root budget. There is no package data or capability
binding.

## Bounds

| Limit | Reference | Hard paper ceiling |
| --- | ---: | ---: |
| foreign calls | 1 | 1 |
| scratch bytes / alignment | 64 / 8 | exact |
| returned record bytes | 24 | 64 |
| payload bytes | 4 | 44 |
| live foreign write regions | 1 | 1 |
| live foreign pointers | 1 | 1 |
| unsafe blocks / foreign declarations | 1 / 1 | 1 / 1 |
| retained diagnostics | 0 | 16 |
| report budget / exact output | 4,096 / 62 bytes | 4,096 |
| tasks / queues / retries / recursion | 0 | 0 |

The launcher supplies one root budget. The application splits three exact
4,096-byte children for scratch, copied payload, and report. Scratch construction
commits exactly 64 initialized bytes plus bounded owner metadata and 8-byte
alignment. The returned payload and report retain their own children; scratch
is released before the successful result escapes.

## Execution order

1. Validate six limits and the exact concrete target/ABI/symbol binding.
2. Split scratch, payload, and report budgets.
3. Construct aligned zeroed foreign scratch.
4. Inside one unsafe value block, create one checked exclusive whole-scratch
   write region, derive its borrow-tied pointer, call once, and return i64 only.
5. End pointer/region lifetimes before any Windvale byte observation.
6. Translate negative status or prove nonnegative length `<= 64`.
7. Borrow exactly the returned initialized prefix and decode in Core.
8. Copy the exact payload into independent immutable bytes.
9. Render/verify the exact report and publish safe record plus text.

## Source record

The reviewed source contains 5 files, 943 LF-terminated lines, 44 top-level
declarations, and 30,304 UTF-8 bytes. The largest cohesive module is
`Foreign-Record-System.wv` at 289 lines / 10,683 bytes. These are source facts,
not compiler/runtime performance claims.

Implementation measurements must record tokens, phase time/peak memory, generic
instances, ownership/unsafe proof evidence, WIR blocks/operations, ABI-thunk
bytes, WVB/native bytes, call/decode/report time, scratch/payload/transient
memory, and isolated containment outcomes on the exact target.
