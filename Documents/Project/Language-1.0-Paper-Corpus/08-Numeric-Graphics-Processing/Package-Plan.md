# Workload 8 package and execution plan

## Mapping

Package identity: `windvale.paper.language1.numeric_graphics` version 1.

| File | Module | Profile | Authority |
| --- | --- | --- | --- |
| `Source/Numeric-Graphics-Types.wv` | `Numericˉgraphicsˉtypes` | Core | library |
| `Source/Numeric-Graphics-Fixture.wv` | `Numericˉgraphicsˉfixture` | Core | library |
| `Source/Numeric-Graphics-Transform.wv` | `Numericˉgraphicsˉtransform` | Core | library |
| `Source/Numeric-Graphics-Audit.wv` | `Numericˉgraphicsˉaudit` | Core | library |
| `Source/Numeric-Graphics-Report.wv` | `Numericˉgraphicsˉreport` | Core | library |
| `Source/Numeric-Graphics-Application.wv` | `Numericˉgraphicsˉapplication` | Core | application |

All modules target Windows, Linux, and Windvale. There are no package-data
objects or capability requirements. The 32 fixture words are typed immutable
module data; they are not serialized through host object layout.

## Bounds

| Limit | Reference | Hard paper ceiling |
| --- | ---: | ---: |
| lanes / each fixed array | 8 | 64 |
| fixed input/expected words | 32 | 256 |
| output vector items | 8 | 64 |
| output budget | 4,096 bytes | 65,536 bytes |
| report budget / exact output | 4,096 / 328 bytes | 65,536 bytes |
| slice views live during fill | 4 | 4 |
| scalar FMA operations | 8 | 64 |
| audit checks | 18 | 64 |
| tasks / capabilities / unsafe calls | 0 | 0 |

The launcher transfers one 8,192-byte root memory budget. The application
splits two exact 4,096-byte children before vector construction. Output backing
retains the first charge; the immutable report retains the second. Failure
releases unreturned owners through lexical cleanup. No lane count, target vector
width, or host thread multiplies memory authority.

## Execution order

1. Validate all limits before splitting.
2. Split output and report budgets.
3. Reserve an eight-item f32 vector and initialize all positions.
4. Create three checked immutable whole-array slices and one exclusive whole-
   vector mutable slice.
5. Process lane indices 0 through 7, one explicit fused call and replacement
   each.
6. End every slice borrow and freeze the vector.
7. Compare all output bits and execute 18 strict boundary/conversion checks.
8. Build and freeze the canonical report.
9. Publish the immutable sequence, Copy audit record, and immutable text.

No partial vector or report is published on failure. A mutable slice cannot
remain live across vector freeze.

## Source record

The reviewed source contains 6 files, 851 LF-terminated lines, 33 top-level
declarations (22 functions, 3 records, 1 variant, 3 constants, and 4 module-data
declarations), and 26,518 UTF-8 bytes. The largest module is
`Numeric-Graphics-Audit.wv` at 268 lines / 9,660 bytes. These are source facts,
not compiler or runtime performance claims.

Implementation measurements must record tokens, phase time/peak memory, generic
instances, WIR blocks/operations, strict-float lowering choices, vectorized or
scalar instructions, WVB/native bytes, execution time, and peak memory on both
hosts and every claimed target.
