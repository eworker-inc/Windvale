# Decision 0432: Fixed native scalar-x64 golden object

- Status: Implemented current-host focused evidence; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0429](0429-Fixed-Native-Assembler-Golden-Objects.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0429 transferred the canonical Hello, expanded-register/control, and
typed narrow-scalar assembler examples. The managed positive surface retained
one compact distinct source covering signed immediate ALU encodings, multiply,
fixed-count shifts and rotates, signed shift, displacement extremes, and
base/index/scale memory operands. Those bytes are part of WVA semantics and are
useful permanent evidence; generated all-register loops remain better owned by
the frozen differential corpus or managed recovery checks.

## Decision

Promote the exact existing managed raw literal to
`Examples/Assembler/Scalar-X64.wva`. The 639-byte LF-only source has SHA-256
`e76cb94b82857e097e734f6bdf01b3383487fd8a69f05214d74a1b69e261ae0e`.
Add it to the existing paired `Test-Assembler-Golden` commands as
`scalar-x64`; do not create another lane.

Both host commands must:

1. pin the complete source identity;
2. assemble it twice through the digest-bound native assembler;
3. require the exact two-line success report;
4. require the 199-byte WVO at SHA-256
   `e1cce07329b6183ebae26ebe252be7d2e754c4aeea08ffe6452c74d60d6ea64a`;
5. independently admit the first WVO; and
6. compare both native products byte for byte.

The exact summary is
`assembly status=valid object-bytes=199 sections=1 symbols=1 relocations=0 offset=639 line=29 column=1`.
The assembler-golden lane grows from three to four cases. The 2,054-byte plan
remains 24 suites and grows from 3,048 to 3,049 cases at SHA-256
`af263e8fe53075a75a21fa76f9277bf50aa8ae18cf628a6df1373f39591ac18b`.

## Evidence and consequences

A static extraction check proves the repository example is exactly the managed
raw literal, apart from the repository file's terminal LF. The focused managed
assertion passes 1/1 in 479 ms and proves the complete expected code bytes,
zero relocations, deterministic assembly, and exact linked image. Temporary WVO
export lines were removed, `Program.cs` returned byte for byte to its committed
state, and the generated directory was sent to the Recycle Bin.

The reviewed Windows command
`Test-Retirement-Suite.cmd --filter assembler-golden` passes all four cases in
2.4 seconds without starting .NET. The other 23 lanes, broad local verifier,
Linux execution, and grouped retirement gate were not run.

The fixed native lane now owns every compact static positive WVA program from
the managed assembler surface. Large generated register matrices and random
differential sequences remain in their existing bounded owners. No assembler,
linker, compiler, runtime, WebAssembly, or product artifact changed.

## Reconsideration triggers

Update the source and exact WVO identity together when accepted WVA syntax,
x86-64 encoding policy, WVO serialization, or the semantic contents of this
fixture change. Add another golden source only when it covers a stable distinct
encoding family that cannot be diagnosed adequately by the differential corpus.
