# Decision 0802: Share x86-64 encoding without compiling through WVA

- Date: 2026-08-20
- Status: Accepted direction; implementation migration pending
- Architecture: [assembler and native lowering](../Architecture/Assembler-And-Native-Lowering.md)
- Product target: [Windvale 1.0](../Project/Windvale-1.0-Product-Plan.md)

## Context

Windvale has three related native-code producers:

- the compiler's native lowerer selects ABI behavior, frames, operations,
  machine sequences, relocations, and WVO output from verified WIR or WVB;
- the WVA assembler parses explicit low-level source and encodes its requested
  x86-64 instructions, symbols, relocations, and WVO output; and
- focused compiler and operating-system producers construct smaller WVO objects
  from already selected machine bytes and records.

The durable architecture already expects the compiler and assembler to converge
on shared machine and object contracts. The current Windvale-written assembler
and native lowerer nevertheless contain separate x86-64 byte-emission and WVO
serialization paths. A portable WVO construction module is reused by several
smaller producers, but it does not yet replace the specialized complete or
segmented compiler/assembler writers.

One apparent simplification would make the compiler print textual WVA and invoke
the assembler. That would add UTF-8 formatting, allocation, parsing, diagnostics,
and another tool boundary while discarding typed lowering evidence. Another
apparent simplification would create a broad architecture-neutral machine IR
before a second architecture or optimizer needs it. Neither work has a current
product justification.

## Decision

Windvale compilation does **not** use textual WVA as an intermediate form. The
normal native path remains:

```text
verified WIR or WVB
  -> semantic and ABI lowering
  -> target-specific instruction requests and patch descriptions
  -> machine encoding
  -> WVO construction for AOT, or bounded publication for JIT
```

WVA remains a human- and tool-authored low-level source contract for startup,
runtime, operating-system, hardware-boundary, conformance, and other explicit
machine code. Its frontend parses WVA into the same target-specific encoding
requests where a shared request is useful. It is not a required compiler output,
portable compiler IR, or definition of Windvale source semantics.

Operating-system and compute work may extend WVA and its x86-64 encoder for a
current implementation, accepted near-term architecture, hardware experiment,
diagnostic tool, or performance investigation involving boot, interrupts,
syscalls, context switching, memory ordering, device I/O, virtualization,
vector compute, or profiling. Windvale need not wait until implementation is
blocked. A planned or exploratory addition must still name its owner, bounded
surface, hypothesis or expected benefit, work budget, stop condition, and
executable validation plan; it need not guarantee later production use. Add
textual syntax only when WVA authors need it; an already typed OS or compiler
producer may call the encoder directly. A single focused producer may retain an
exact qualified byte sequence until reuse, drift, or maintenance pressure
justifies extraction.

The `Assembler/` area owns x86-64 instruction encoding. Separate its reusable
target-specific operand/instruction encoder from the WVA textual frontend when
a real shared consumer is migrated. The compiler owns semantic lowering,
verification, ABI selection, frame and value layout, instruction selection,
metering, and runtime-call policy; it may call the Assembler-owned encoder but
must not call the WVA parser.

The `Object-Model/` area owns canonical WVO record encoding, construction, and
verification. Assembler, compiler, and focused system producers should reuse
that owner for overlapping WVO behavior. A large compiler object may use a
bounded planned or segmented writer rather than materializing the whole object
as one ordinary Windvale `bytes` value. Sharing the contract does not require
one allocation strategy or erase producer-specific layout validation.

Share only real overlap:

- exact register, width, condition, memory-operand, immediate, and primitive
  instruction encoding used by more than one live producer;
- exact patch-field and relocation descriptions;
- canonical WVO headers, sections, symbols, relocations, ordering, and final
  verification; and
- deterministic measurement and encoder conformance cases.

Keep these responsibilities separate:

- Windvale parsing, typing, ownership, WIR, WVB, and source diagnostics;
- WVA tokenization, declaration rules, source locations, and assembly
  diagnostics;
- compiler ABI, frame, register/value placement, checked-operation expansion,
  metering, and service-call selection;
- linker symbol resolution, final addresses, relocation application, and output
  container policy; and
- producer-specific publication, authority, and resource limits.

Do not build a general architecture-neutral machine IR, compiler-generated WVA
path, inline assembly feature, macro assembler, optimizer, disassembler, or new
architecture merely to make the sharing appear complete. Add one only for a
named current or accepted near-term consumer and accepted contract.

Do not place accelerator device instructions in x86-64 WVA. NVIDIA and AMD
kernels use the shared Windvale frontend followed by a separately verified
target-scoped kernel representation and provider backend. CPU-side driver and
provider code may use WVA where it genuinely needs explicit x86-64 operations,
but PTX, cubin, fat binary, SPIR-V, AMDGPU code object, and other device formats
remain accelerator backend or provider artifacts. A native GPU assembler is not
required until a measured capability, performance, independence, or Windvale OS
need cannot be met by the selected bounded provider path.

Do not rewrite already-qualified producers solely to reduce similar-looking
code. Begin extraction when a current or accepted near-term Windvale 1.0, OS,
compute, instruction, relocation, object, JIT/AOT, tooling, maintenance,
correctness, or performance investigation would benefit from shared ownership
or otherwise duplicate work. Migrate the smallest coherent family first and
require byte-identical WVO or machine output, unchanged diagnostics and limits,
and focused Windows/Linux evidence before expanding the shared surface.

## Consequences

- Compiling Windvale source never requires producing, storing, or reparsing WVA.
- WVA remains useful for code whose input is naturally explicit machine
  instructions, especially startup and OS/platform boundaries.
- OS and compute work can proactively add the smallest coherent qualified WVA
  or encoder family justified by a current or accepted near-term plan without
  committing Windvale to a complete x86-64 catalog.
- Accelerator kernels remain in the shared compiler architecture without
  turning the CPU assembler into a multi-vendor GPU assembler.
- The assembler remains independently useful even though the compiler reuses
  its encoding library.
- AOT WVO production and JIT publication may share instruction encoding and
  patch descriptions without forcing the JIT to serialize WVO first.
- Existing WVA and native-lowering outputs remain the byte-identity oracles
  during migration; pinned tool identities change only through deliberate
  promotion.
- The Windvale 1.0 gate does not require refactoring every historical instruction
  sequence. It requires one coherent owner for selected shared behavior and a
  documented exception wherever selected production paths intentionally remain
  independent.

## Reconsideration triggers

Revisit this decision if a measured compiler workload benefits from textual WVA
as an optional diagnostic artifact, if a second native architecture requires a
small target-neutral selection layer, if JIT and AOT cannot share an encoder
without weakening one path's bounds, or if independent encoders provide a
specific security benefit that outweighs their drift and maintenance cost.
Treat a future native NVIDIA or AMD device encoder as a separate target decision
rather than an extension of x86-64 WVA.
