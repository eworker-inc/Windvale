# Windvale assembler and native-lowering architecture

## Status

Accepted direction under
[Decision 0802](../Decisions/0802-Share-X64-Encoding-Without-Compiling-Through-WVA.md).
The current WVA assembler and native x86-64 lowerer already agree through WVO
1.0 but still contain separate encoding and object-writing implementations.
Shared implementation migration is pending and remains evidence-directed.

## Outcome

Windvale keeps a direct typed compiler path and an independently useful textual
assembler while sharing the exact low-level code that genuinely has two or more
production consumers.

```text
Windvale source
  -> typed WIR
  -> canonical WVB
  -> verified native semantic/ABI lowering ----\
                                               \
WVA source -> WVA parser and semantic checks ---> x86-64 encoder
                                                    |       |
                                                    |       `-> bounded JIT patch/publication plan
                                                    `-> WVO records -> WVO verifier -> linker/AOT
```

The diagram shows ownership and reusable contracts, not a requirement to build
one complete in-memory machine program. A producer may measure and emit bounded
instruction or object spans while preserving the same canonical result.

## Current implementation facts

The repository reached native independence through focused producers before this
shared boundary was selected:

- `Assembler/Windvale/Wva-Assembler-Core.wv` owns complete WVA scanning,
  semantics, direct x86-64 encoding, and direct WVO record serialization;
- `Compiler/Windvale/Native-X64-Lowering-*.wv` owns compiler-specific validation,
  planning, machine templates, patches, and a specialized complete/segmented WVO
  writer;
- `Object-Model/Windvale/Wvo-Object-Construction.wv` provides a verified small
  WVO constructor already reused by focused compiler and OS object producers;
  and
- `Foundation/Machine-Contracts.wv` shares exact alignment and machine-name
  validation, not an instruction encoder.

Those implementations are qualified evidence, not mistakes to erase. Their
successful bytes remain migration oracles. The accepted direction prevents new
overlap from growing without an owner while avoiding a rewrite whose only result
would be a different internal arrangement.

## Meaning of lowering

“Lowering” names several different transformations:

| Transformation | Owned decisions |
| --- | --- |
| Windvale source to typed WIR | Language meaning, types, ownership, control flow, diagnostics, and source locations. |
| WIR to WVB | Portable verified operations and deterministic distributable encoding. |
| Verified WIR/WVB to native plan | ABI, frames, value placement, checked-operation expansion, service calls, metering, machine instruction selection, and patch intent. |
| WVA to encoding requests | Explicit registers, operands, labels, symbols, instructions, source locations, and assembly diagnostics. |
| Encoding requests to bytes | Exact target prefixes, opcodes, ModRM/SIB, immediates, displacements, and patch fields. |
| Machine spans to WVO | Sections, symbols, relocation records, order, limits, and canonical verification. |
| WVO to final image | Global symbol resolution, final layout, relocation values, entry selection, and output-container policy. |

Only the last part of native lowering overlaps the assembler. The compiler must
still decide what the program means and which machine operations implement that
meaning. The assembler receives those machine choices explicitly from WVA.

## Ownership boundaries

### Compiler

The compiler owns:

- WIR/WVB validation and semantic agreement;
- target selection and ABI version;
- function, frame, value, record, variant, descriptor, and call layout;
- checked arithmetic, bounds, metering, and failure-path expansion;
- instruction selection and ordered patch intent; and
- compiler-facing diagnostics and source mappings.

It submits target-specific encoding requests directly. It does not format WVA,
invoke the WVA parser, or inherit language semantics from assembly syntax.

### Assembler

The Assembler area owns two separable parts:

1. the WVA frontend: UTF-8 admission, tokens, declarations, definitions, labels,
   source locations, semantic checks, and WVA diagnostics; and
2. the target encoder: typed x86-64 registers, widths, conditions, memory
   operands, immediates, primitive instruction forms, encoded length, bytes, and
   patch-field descriptions.

The encoder is reusable without importing the WVA grammar. WVA-only privileged
or platform operations may remain focused assembler operations until another
producer needs their primitive encoding.

### Object model

The Object-Model area owns WVO tags, record encoding, canonical order,
construction, verification, and admission profiles. Producers own the semantic
choice and layout of their sections, symbols, and relocations.

The existing small constructor may continue returning one immutable `bytes`
value. Compiler-scale production requires a compatible planned or segmented
profile so sharing does not introduce a whole-object allocation, repeated
concatenation, or an authority increase. Both profiles must produce the same WVO
bytes when given the same admitted records.

### Linker and publishers

The linker continues to consume complete verified WVO semantics. It chooses
final section addresses, resolves imports/exports, applies relocations, and
produces the selected image. PE, ELF, UEFI, flat-image, W^X, file-publication,
and process-authority rules remain outside the instruction encoder.

A JIT publisher may consume encoded bytes and bounded local patch descriptions
without constructing WVO. It still uses the same ABI, instruction encoding, W^X,
cache, synchronization, resource, and verification rules selected for that JIT
profile.

## Operating-system demand and accelerator code

Windvale OS is a first-class consumer of WVA and the x86-64 encoder. Current
implementation, an accepted near-term architecture, hardware exploration,
diagnostic tooling, or performance research may add an instruction form,
relocation, or other machine contract for boot, interrupts, syscalls, context
switching, memory ordering, device I/O, virtualization, vector compute, or
profiling. This work is evidence-directed; it does not need to wait until a
blocked implementation already exists:

- add WVA syntax only when human- or tool-authored assembly must request the
  operation;
- let an already typed compiler or OS producer call the reusable encoder
  directly when no textual assembly contract is needed;
- permit a planned or exploratory consumer when it has a named owner, bounded
  surface, hypothesis or expected benefit, work budget, stop condition, and
  executable validation plan; it need not guarantee later production use;
- retain an irreducible focused byte sequence in its existing owner when it has
  only one consumer and exact golden evidence is clearer than generalization;
  and
- specify target features, privilege, faults, operands, encoding, bounds, and
  emulator or physical-hardware evidence for every admitted addition.

Windvale may therefore implement a coherent instruction or relocation family
before the first production call site when doing so unblocks the accepted OS or
compute roadmap, improves testability, or reduces predictable duplication. The
roadmap does not authorize an unreviewed complete x86-64 catalog.

Accelerator device code is a separate target family. The x86-64 WVA assembler
may build CPU-side startup, driver, provider, and queue-management code, but it
does not encode NVIDIA or AMD GPU kernels. The accepted accelerator direction
uses the same Windvale lexer, parser, type system, ownership analysis, and WIR
orchestration, followed by a separately verified target-scoped kernel
representation and provider backend. The first reserved interfaces are the
software oracle and SPIR-V; a vendor adapter may instead or additionally lower
admitted kernels to a versioned NVIDIA or AMD artifact when its capability and
evidence require that path.

PTX, cubin, fat binary, SPIR-V, AMDGPU code object, and vendor-library artifacts
are provider or backend formats. None defines Windvale semantics, enters WVA,
or justifies a second source compiler. Windvale should not implement an NVIDIA
native-instruction assembler or AMDGPU assembler for the first physical-provider
proof. Such an encoder becomes justified only when a named capability,
performance result, toolchain-independence requirement, or Windvale OS driver
cannot be met through the admitted target representation and bounded vendor
provider.

The least-work near-term physical proof is:

1. qualify the exact kernel and operation contract through the software oracle;
2. lower the restricted kernel contract to an exact SPIR-V profile and execute
   it through a hosted Vulkan provider on Windows or Linux, recording separate
   evidence from one NVIDIA and one AMD device; and
3. add CUDA/PTX, HIP/ROCm, or vendor-library paths only for a named operation,
   feature, or measured performance gap that the common provider cannot meet.

This order provides physical acceleration before Windvale owns native GPU
instruction encoders or complete GPU drivers. A Windvale OS provider comes
later, after PCIe discovery, interrupts, memory ownership, DMA/IOMMU isolation,
queue accounting, provider teardown, firmware, and device reset have qualified
owners. The accelerator semantic and safety contract is defined in the
[accelerator compute and AI design](../Project/Windvale-Accelerator-Compute-And-AI-Design.md);
device assignment and OS containment remain under
[Decision 0171](../Decisions/0171-Future-Virtualization-And-Accelerator-Architecture.md).

## Minimal reusable contract

The first shared encoder surface should be no larger than the first migrated
overlap. It needs to express only:

- one x86-64 instruction identity;
- exact register and operand widths;
- register, immediate, condition, and admitted memory operands;
- the instruction's encoded length and bytes; and
- zero or one bounded patch-field description when the primitive form needs
  later target resolution.

Compound WVA statements and compiler checked operations remain expansions into
primitive instructions owned by their respective producer unless the exact
compound operation itself gains another consumer. This prevents a convenience
syntax or one compiler template from becoming the universal machine model.

An encoder call must reject an invalid operand combination before returning
bytes. It must not consult host architecture, locale, paths, process state,
ambient features, or a symbol table. The caller owns feature/profile admission
and converts an admitted patch description into a local branch fixup, WVO
relocation, or JIT patch according to its own contract.

## Work-avoidance rules

The following work is deliberately unnecessary now:

- emitting WVA during ordinary Windvale compilation;
- parsing WVA to compile an already typed Windvale program;
- a target-neutral machine IR without a second target or optimizer consumer;
- a full x86 instruction catalog beyond selected compiler, runtime, and OS use;
- NVIDIA or AMD device instructions in x86-64 WVA;
- a native GPU assembler before a selected backend has a measured need for one;
- inline assembly in Windvale Language 1.0;
- a macro/preprocessor language for WVA;
- a production disassembler merely to test the encoder;
- changing WVO 1.0, the linker, ABI, or WVA syntax when extraction alone is
  sufficient; and
- migrating stable WVA-only operations that have no second consumer and no
  correctness or maintenance pressure.

Optional compiler-generated WVA may be reconsidered later as a debugging or
teaching artifact. It must not become the executable semantic path or a required
build intermediate.

## Evidence-directed migration

Start shared implementation when a named current consumer, accepted near-term
OS or compute consumer, hardware experiment, tooling need, predictable
duplication, measured defect, or maintenance burden justifies extraction. Then:

1. select the smallest instruction and relocation family used by both WVA and
   native lowering;
2. freeze direct positive, negative, boundary, and byte-identity vectors from
   both current producers;
3. implement the Assembler-owned target encoder without changing WVA syntax,
   WVB semantics, ABI, or WVO;
4. move the WVA frontend and native lowerer to that encoder for only the selected
   family;
5. reuse or extend the Object-Model writer only where record construction
   actually overlaps, retaining segmented compiler output where required;
6. require byte-identical successful output, unchanged failure behavior and
   bounds, and no material time or memory regression; and
7. expand when a current consumer, accepted near-term consumer, hardware
   experiment, diagnostic tool, or performance investigation benefits from the
   next coherent family.

The retained WVA golden/differential corpus, native-lowering WVO corpus,
independent WVO verifier, interpreter/AOT/JIT agreement cases, and Windows/Linux
focused owners provide the migration oracle. A successful extraction is an
implementation improvement, not a new source-language or object-format feature.

## Windvale 1.0 boundary

Windvale 1.0 needs a coherent native toolchain, not a cosmetically complete
refactor. For every instruction and WVO behavior selected by two production
paths, the release must either use the shared owner or record a deliberate
independent implementation with exact differential evidence and a reason to
retain it. WVA-only platform instructions and compiler-only semantic templates
do not require forced generalization. Accelerator qualification additionally
requires the shared Windvale frontend and software oracle to agree with each
selected physical provider; it does not require GPU device code to pass through
WVA or the x86-64 encoder.
