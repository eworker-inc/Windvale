# Windvale Seed implementation

## Status

The core Seed milestone is implemented and qualified. Later compiler-bootstrap, shared-native, and operating-system slices consume these contracts and are summarized here only where they clarify current ownership.

## End-to-end path

```text
UTF-8 source
    |
lexer and parser
    |
bounded static source-module composition
    |
semantic symbols and typed WIR
    |
deterministic stack-bytecode lowering
    |
canonical sectioned .wvb module
    |
bounded decoder and verifier
    |
verified-module boundary
    |
reference runtime and explicit host capabilities

Windvale source, WVA 1, or Stage 0 producer
    |
canonical WVO 1.0 object bytes
    |
bounded object decoder and verifier
```

The runtime cannot execute raw module bytes or an unverified `Bytecodeˉmodule`. Its public constructor accepts only `Verifiedˉmodule`, which can be obtained through `Moduleˉverifier` or `Moduleˉcodec.Readˉandˉverify`.

## Ownership

### Windvale compiler

`Compiler/Windvale/` owns the compiler implementation written in Windvale:

- The first Windvale-written streaming lexer over strict UTF-8 bytes, with Stage 0 token identities and bounded source/failure coordinates
- A Windvale-written declaration and body pass exposing module/declaration/body byte spans and counts through streaming cursors without token or declaration collections, including inferred locals, named record literals, and recursive `else if`
- A compiler-owned canonical packed source-set reader that gives portable semantic phases indexed immutable views over the root and ordered dependencies
- A Windvale-written import-graph phase that owns bounded module resolution, root reachability, and cycle rejection without host paths or collections
- A Windvale-written declaration/signature symbol phase with independently validated packed declaration evidence, transitive visibility, deterministic nominal indices, and stable namespace/type failures
- A Windvale-written parameter/local and body-reference binder with independently validated packed WVLB evidence
- A Windvale-written typed WVIR producer with explicit blocks, temporaries, source spans, and an independent packed-directory validator
- A Windvale-written WVIR-to-WVB backend that emits one canonical verified module from a validated source graph

This implementation is the qualified Windvale bytecode compiler: Stage 0 builds Stage 1, and Stage 1 reproduces the exact Stage 2 WVB from the committed 12-module inventory on Windows and Debian. Cross-host-qualified ABI 22 also runs Stage 1 as verified native x86-64 and reproduces the same Stage 2 bytes under a bounded host arena. Standalone native-tool packaging remains open.

### Reference compiler

`Compiler/Reference/` owns the independent C# Stage 0 and recovery implementation:

- Source locations and stable compiler diagnostics
- Tokenization and strict string-literal handling
- Recursive-descent and precedence parsing, including named record fields and block-form `else if`
- Explicit bounded source-module graph validation and deterministic static composition of dependency records, enums, functions, and explicitly re-approved catalog capabilities
- Module, capability, data, record, enum, function, local, and nominal type binding
- Typed, stack-independent WIR with explicit blocks and terminators
- Deterministic lowering from WIR into stack bytecode
- A bounded x86-64 kernel-entry target that lowers the specified linear system-profile WIR subset into independently verified WVO

WIR uses virtual temporaries and local slots. The C# reference compiler lowers its typed WIR to bytecode and, for the narrow target specified by [Windvale-X64-Kernel-Target.md](../../Specifications/Windvale-X64-Kernel-Target.md), to a code-only WVO kernel entry. That target accepts only one linear `Main`, constant text output through an explicit capability adapter, and a constant result; it is not the general native backend. The portable Windvale compiler publishes the separate WVIR 1 contract. Its backend assigns every WIR temporary a bytecode local and emits one complete WVB 1.11 module from a validated WVSS graph whose root is portable, hosted, or system. It currently internalizes portable dependency functions and nominal types while preserving root static data, explicit catalog capabilities, profile, and exports. The Stage 0 candidate additionally admits profile-compatible capability-bearing libraries only when every importer explicitly redeclares the complete transitive requirement set; the Windvale-written compiler has not yet adopted that extension. WVIR retains stable WVSD declaration identities; the backend resolves each identity through its owner source, translates it to ordinal WVB function, data, and capability indices, uses canonical nominal identities as Types indices, and emits canonical functions, root exports, types, capabilities, explicit data, and cross-module interned text literals. The operand stack stays empty between WIR operations and at block boundaries. This intentionally verbose form remains easy to inspect, verify, and compare byte for byte with the reference compiler. Runtime linkage remains separate and is not required by static source composition.

### Bytecode

`Runtime/Windvale.Bytecode/` owns:

- Immutable bytecode contracts and nominal record schemas
- Tagged nominal enum schemas and exact value shapes
- Canonical seven-section serialization
- Strict UTF-8 and little-endian binary decoding
- Size, count, range, canonical-order, and signature limits
- Instruction decoding
- Control-flow and operand-stack type verification
- Module inspection, disassembly, and SHA-256 identity

Every function is checked for valid branch boundaries, index and type use, matching stack states at merges, reachable instructions, valid returns, and an exact declared maximum stack.

### Shared native execution

`Runtime/Windvale.Native/` owns the current bounded ABI-22 execution path, strict fragment reconstruction, W^X JIT and WVO/AOT execution, versioned execution context, twelve native service leaves including exact Windows/Linux file output, verified internal calls through 64 parameters, bounded dynamic-value ownership/checkpoints, publication layout, and narrow platform adapters. Windvale-written source under `Compiler/Windvale/` validates and constructs the two live process-input leaves, plans every executable-image extent and canonical service placement, and independently lowers the accepted scalar/control/general-call subset plus bounded immutable i32/text/bytes data, borrowed descriptor operations, service-backed text operations, generation-owned bytes concatenation, enum operations including nonzero-first tables, multi-block record construction and calls, and all six hosted calls declared by the real lowerer: `process.argument_count() -> u32`, `process.argument(u32) -> text`, `file.read_bytes(text) -> bytes`, `file.write_bytes(text, bytes) -> void`, `console.write_line(text) -> void`, and `diagnostic.write_line(text) -> void`. The candidate admits at most 512 functions, 64 immutable data declarations, and 64 nominal types, and generates canonical D4 WVO helper and data symbols through its focused layout module. It emits exact WVO `.text`, optional `.rodata`, symbols, and relative relocations. A complete signature pass admits forward and recursive scalar calls under shared instruction/depth budgets, while focused capability-table and capability-state, data, type-table, function-layout, descriptor, enum-instruction, record-allocation, record-local-liveness, record-storage, record-instruction, call-instruction, instruction-template, and object modules keep format responsibilities outside the already-large instruction core. Decision 0304 pins that exact accepted-subset tool, its native-produced fixed vector, and digest-bound candidate launchers. Decision 0497 reconstructs the current WVB and both target containers through the retained segmented native toolset on the current Windows host. Decision 0498 then uses the retained native source, lowering, linking, and hosted-container path to reconstruct the exact paired ordinary and segmented console-packager applications without a managed writer. Decision 0499 uses the retained raw lowerer, exact WVO oracle, and role-3 publisher construction to reconstruct the paired WVO publisher applications on that host without a managed writer or target self-publication. C# Stage 0 still owns complete lowering outside that accepted subset, OS allocation and protection, cache publication, invocation, arenas, and teardown until the measured transfers and [native-retirement gate](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md#native-retirement-gate) are complete.

The accepted-subset lowerer also exposes segmentable WVO regions, bounded
function batches, and an immutable exact-position publication cursor. Its
focused hosted staging shell writes each nonempty cursor value to a distinct
bounded resource and writes the versioned `WVOP 1` manifest last. That is a
multipart transport seam, not atomic publication. A focused capability-free
reader owns canonical manifest serialization and strictly validates bounded
counts, extents, indices, and contiguous positions without touching host
resources. A tiny capability-free bridge maps its exact statuses across ABI
22's borrowed-`bytes`/scalar-return convention and exposes only revalidated
object/count/entry scalars to a fixed native caller. A fixed native adapter
also receives bounded metadata chunks through a focused scalar bridge. The
shared Windvale reader validates the compiler-produced WVO header, `.text`,
optional `.rodata`, 32 MiB section extents, following metadata boundaries, and
minimum record tail without constructing one whole object value. A second
bounded reader consumes the complete compiler-produced symbol chunk, validates
its data/function/Main order and ranges, and fixes the exact relocation-table
extent. A third reader validates the complete canonical relocation chunk,
exposes the exact text-chunk count, and checks zero placeholders plus canonical
padding inside each actual bounded text chunk. A fourth typed cursor replays
the retained lowering publication, binds every nonempty actual chunk to its
exact manifest entry, compares arbitrary code, data, and metadata bytes, and
requires complete final coverage without constructing one whole WVO value. A
fixed native adapter must preserve those snapshots, derive and bind the staged
identities, consume the exact verified chunk sequence, and enter
the qualified sibling-replacement transaction before the managed publisher can
leave the normal path.

### Object model

`Object-Model/Windvale.ObjectModel/` owns:

- Immutable WVO 1.0 section, symbol, and relocation contracts
- Canonical x86-64-first object serialization
- Strict ASCII machine names and bounded little-endian decoding
- Size, alignment, range, canonical-order, symbol-reference, relocation-placeholder, and overlap verification
- Object inspection and SHA-256 identity

`Object-Model/Windvale/Wvo-Object-Verification.wv` owns the corresponding Windvale-written bounded reader and verifier. The smaller `Wvo-Object-Core.wv` owns only its self-test, hosted command shell, SHA-256 identity, and complete read-only report path. `Windvale-Wvo-Object.wvproj` composes those focused modules with byte ordering and SHA-256, while paired profile-6 Windows/Linux candidates reuse the existing eleven-service read-only inspector package without adding platform assembly or file-write authority. Decision 0308 also composes the portable verification module with the existing native publication transaction, so accepted-subset lowering now publishes a private whole-object candidate atomically without a second parser or managed wrapper.

The C# object verifier returns a `Verifiedˉobject`; both Stage 0 CLI object commands decode and verify before reporting. During native-candidate qualification this project remains the independent recovery/differential oracle. Decision 0301 now supplies digest-bound native WVO verification and inspection launchers, but they remain candidate entry points until the grouped Windows/Linux gate promotes their exact containing commit. After promotion they become the ordinary path while C# remains explicitly named recovery evidence until the complete retirement gate passes.

### Windvale assembler

`Assembler/Windvale/` owns the assembler implementation written in Windvale:

- Bounded WVA 1 line and token scanning over immutable source bytes
- Complete grammar, declaration, ordering, context, local-label, reference, register-width, and limit validation
- Named-definition offset and size derivation
- Typed 32/64-bit GPR, REX/ModRM/SIB, local-control, immediate scalar, multiply, shift/rotate, stack, indirect-control, RIP-relative, indexed-memory, machine, and data encodings
- Canonical WVO relocation and object-byte construction
- Hosted input/output composition with publication only after complete success

### Reference assembler

`Assembler/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVA 1 line/token parsing, typed register/memory/immediate operands, local fixups, and stable source diagnostics
- Canonical symbol and section declaration validation
- Named-definition offset and size derivation
- The same expanded x86-64 instruction and data encodings through an independent encoder
- WVO relocation creation and production through the independent object verifier

Both implementations own the same WVA contract and remain byte-for-byte differential oracles. The reference assembler depends only on the object model and returns no bytes until the resulting object passes `Objectˉverifier`; qualification routes Windvale-written output through that independently owned verifier. Neither implementation resolves symbols, chooses final addresses, applies relocations, defines an ABI, or produces an executable image.

### Windvale linker

`Linker/Windvale/` owns the linker implementation written in Windvale:

- Complete immutable WVO validation and link-wide aggregate limits
- Object-private locals plus unique global export/import resolution
- Actual-address alignment and deterministic section contribution order
- Bounded flat-image construction with zero padding and materialized BSS
- Checked `absolute-u32` and `relative-i32` relocation application
- Independent complete-image reconstruction before publication
- Path-free canonical map construction and hosted publish-after-success composition
- Portable paired-console layout, sparse exact construction, segmented completed-container verification, and recovered-native evidence

Decision 0501 reconstructs the standard linker's exact WVB, raw-lowerer WVO
oracle, independently staged and transported fragment, and paired profile-4
applications on the current Windows host. The distinct segmented image path
keeps both target standard linkers outside their own construction, while the
retained same-release compiler, lowerer, staging/link/transport, and hosted
toolsets remain explicit bootstrap seeds.

Decision 0502 uses the retained native Project 1 compiler, raw lowerer,
standard linker, and hosted construction toolsets to reconstruct the exact
console-application-verifier WVB, WVO oracle, linked fragment, and paired
profile-7 applications on the current Windows host. The two-snapshot verifier
does not participate in constructing itself. The retained same-release tools
remain bootstrap seeds, so independent Linux execution, clean previous-seed
renewal, qualification, promotion, and recovery release remain open.

Decision 0503 uses those retained native compiler, raw-lowerer, standard-link,
hosted-container, and publisher-construction boundaries to reconstruct the
exact console-application-publisher WVB, WVO oracle, linked fragment, target
bases, and paired applications on the current Windows host. Explicit overlay
variant 4 owns the role-specific metadata, structure, target, object, import,
and materialization evidence without invoking either target publisher. This is
narrow same-release construction ownership. The final candidate refresh binds
the current file-input leaf and replaces only stale final application bytes and
digests; independent Linux reconstruction
and execution, clean previous-seed renewal, qualification, promotion, and
recovery release remain open.

Decision 0504 composes the existing native Project 1, compiler-bootstrap,
portable-compiler, and paired WebAssembly backend boundaries into the complete
current-Windows generation-and-verification command. One generic launcher
admits the exact host backend, constructs an import-free module privately, and
publishes only after validation. The strict Node.js engine, record-arena, and
compiler probes pass without a normal .NET invocation. Independent Linux
execution, backend-package reconstruction, cross-browser evidence, grouped
qualification, promotion, and the final recovery release remain open.

Decision 0505 reuses the same qualified Project 1 builder plus native WVB
verifier and inspector for the first qualification-only manual checks in both
broad Seed scripts. Four exact WVB products and one malformed-project
preservation case replace nine managed invocations per host script. The new
Hello and bytecode-header manifests live beside their source and resolve paths
relative to that directory; this is a component-local organization choice, not
a repository-root project convention. Managed execution, packaging, harness,
and later qualification phases remain explicit.

Decision 0506 then consumes the exact Sum WVB from that helper through the
native lowerer, WVO verifier, flat linker, and paired version-1 console
packager. The complete map and every intermediate identity are fixed; the
current Windows product executes to result `29`. The paired helper therefore
owns this qualification composition without treating the frozen Stage 0
`compile --target` command as the ordinary constructor. General target
coverage, broad execution and harness work, and independent Linux evidence
remain outside this narrow transfer.

Decision 0507 advanced the WVB execution product from a historical packaged
seed to an exact retained-WVB profile-5 reconstruction. It kept that WVB as the
explicit source boundary, lowered and linked it with native tools, and
constructed both target applications through the shared hosted-verifier path.
Decision 0509 supersedes that active boundary while preserving this evidence as
recovery provenance.

Decision 0508 then uses that current runner in the paired Seed front-door
helper for three exact capability-free execution checks. Sum, the Foundation
header reader, and the composed-project module must return `29`, `1`, and `42`
and retain their exact bytes. The broad scripts no longer repeat those plain
runs through Stage 0, while managed reporting, capability authorization, and
the remaining broad harness stay explicit.

Decision 0509 replaces Decision 0507's retained source boundary with the
complete Project 1 closure. The native front door now builds the exact current
runner WVB, and the same construction route pins its WVO, fragment, and paired
profile-5 applications. The runner's optional overall-instruction report moves
the Sum fixture's exact count into the nine-case native Seed helper. The frozen
Stage 0 compiler remains an intentionally divergent recovery/differential
implementation rather than the current source-product oracle.

Decision 0510 grows that helper around four real Foundation components. The
single-component manifests live in `Foundation/`; only the demo aggregates
remain at the repository common ancestor because Project 1 correctly forbids
parent-directory escape. Native construction and inspection own all eight
products, and the one-million-instruction runner executes the three demos
inside its current value-memory envelope. Byte Construction's 4 MiB demo stays
with the frozen differential runtime until the native value allocator owns that
shape. This replaces fifteen more managed calls per broad host script without
pretending the remaining broad harness is native.

Decision 0511 extends the same ownership boundary to the native-stencil,
UTF-8, integer-format, and shared service-code source products. Eight exact
Project 1 builds and seven native inspections move out of each broad managed
script while the retained bridge/leaf comparisons remain independent. The
single-component manifests live beside `Compiler/Windvale` or
`Runtime/Windvale`; only the Stencil demo remains a repository-root aggregate
because it spans `Examples/Compiler` and `Compiler/Windvale`. A later
workspace/reference layer should remove that placement pressure without
weakening Project 1 containment. The demo's 20-million-step execution remains
managed rather than silently widening the fixed runner policy.

Decision 0512 extends component ownership to the full output, file-output, and
file-input generator closures. Eleven exact native Project 1 builds and three
bridge inspections move out of each broad managed script while byte-for-byte
embedded bridge and platform-leaf comparisons remain. Every touched manifest
lives beside `Runtime/Windvale` source and the three former repository-root
bridge manifests are removed. This is build/inspection ownership, not a claim
that the capability-bearing execution or broad differential harness is native.

Decision 0513 extends the same ownership rule to fixed service leaves, enum
metadata, native publication planning, and service-bundle materialization.
Twelve exact Project 1 builds and eleven inspections leave each broad managed
script while their retained bridge, leaf, and fragment comparisons remain.
Ten manifests live beside their Runtime or Compiler sources. Only the two
service-bundle manifests remain at repository root because their closure spans
Compiler publication, Foundation byte construction, and Runtime
materialization. This is build/inspection ownership, not native ownership of
the capability-bearing execution or broad differential harness.

Decision 0514 extends that ownership rule through the runtime-table and entry
metadata layer. Sixteen exact Project 1 builds and eight bridge inspections
leave each broad managed script while retained bridge-WVB and fragment
comparisons remain. All sixteen manifests live beside their
`Runtime/Windvale` sources, and the eight obsolete root bridge manifests are
removed. This is build/inspection ownership, not native ownership of
capability-bearing execution or the broad differential harness.

Decision 0515 transfers the next contiguous construction block: hosted-tool
metadata admission/construction, startup instantiation, four hosted-container
products, runtime-header construction, and publication lifetime. The paired
native helper owns twelve additional builds and nine inspections; the broad
managed scripts consume those exact outputs only for retained WVB, startup-WVO,
and linked-fragment comparisons. Single-component manifests are local to their
Runtime, Linker, or Compiler owner, but projects spanning Foundation, Runtime,
and Linker remain explicit repository-root aggregates.

Decision 0516 transfers the first three Windvale-written source-compiler
phases. The paired helper now builds the lexer core/demo, declaration-parser
core/demo/tool, and body-parser core/demo/tool and inspects the three core
type/export surfaces. Their cross-component Project 1 manifests remain root
aggregates because Project containment must not permit an Examples or Compiler
manifest to escape upward to Foundation. The broad managed scripts retain only
the three demo runs and five capability-bearing hosted-tool runs from this
block. The current native runner's inability to complete those demos is an
explicit execution gap, not a reason to keep ordinary construction managed.

### Reference linker and target adapters

`Linker/Reference/` owns the independent C# Stage 0 and recovery implementation:

- WVO decoding through the independently verified object-model boundary
- The same symbol resolution, layout, relocation, reconstruction, and canonical map contract
- SHA-256 flat-image evidence and deterministic failure diagnostics
- The C#-only deterministic UEFI PE32+ application adapter and its independent verifier
- The first deterministic import-free Windows x64 console adapter for capability-free scalar ABI-20 fragments, including its exact startup/context boundary, sparse-recipe materialization, and independent PE recovery verifier
- The paired deterministic sectionless Linux x64 static-PIE adapter over the same verified fragment, including its bounded mapped stack, exact syscall boundary, sparse-recipe materialization, and independent ELF recovery verifier

Both linker implementations own the same `flat-x86-64-v1` contract and remain byte-for-byte differential oracles. They do not parse WVA, encode instructions, mutate input objects, or define portable semantics. The raw flat image remains a deterministic memory snapshot. The narrow UEFI adapter consumes successful flat-link evidence without changing portable link semantics; the Windows and Linux console targets additionally consume one independently verified ABI-20 fragment, reproduce it through WVO/link, and supply target-specific exact process-entry/context adapters. Portable Windvale now plans and describes every container byte and verifies completed PE/ELF bytes through two bounded chunks; Decision 0303 pins a Windvale-native bounded materializer candidate, while C# retains candidate construction, ordinary-path publication, evidence checks, and independently structured PE/ELF recovery until promotion. General hosted-service PE/ELF and Windvale OS target adapters remain later explicit contracts.

### Runtime

`Runtime/Windvale.Runtime/` owns:

- Typed runtime values
- Deterministic local defaults
- Checked `i32` arithmetic
- Checked `u32` arithmetic and `u8` values
- Compile-time checked, storage-free typed scalar and enum constants
- Immutable byte sequences and zero-copy slice views
- Immutable nominal record values and field access
- Nominal enum values, names, equality, and invariant bounded formatting
- Bounded `u8`, little-endian `u16`, `u32`, and signed `i32` reads
- Strict UTF-8 validation/encoding/decoding, safe ASCII quoting, and explicit `u8` to `u32` conversion
- Immutable byte concatenation, fixed-width little-endian construction, and exact SHA-256 identity
- Immutable data access and bounds traps
- Function frames and call-depth limits
- Instruction accounting and execution limits
- Capability authorization, host-support preflight, invocation, and return-value validation
- Bounded launcher arguments, first-read immutable hosted file snapshots, bounded file output, deterministic console output, and separate diagnostics

The interpreter uses ordinary portable .NET APIs and has no Windows-specific or Linux-specific execution path. Immutable byte values use height-balanced persistent trees so concatenation and slicing share storage while reads remain bounded by tree height; strict UTF-8, SHA-256, and hosted output materialize one contiguous value only at their native boundary. Resources are injected through `ICapabilityˉhost`, keeping execution independent from ambient process arguments, files, and console state. The CLI owns the native path adapter and maps it into the hosted file contract.

### CLI

`Tools/Windvale.Tool/` owns argument parsing, strict UTF-8 compiler and assembly input, bounded object input, native hosted-file adaptation, file output, diagnostic presentation, capability grants, and command exit codes. It does not reimplement compiler, assembler, linker, verifier, inspector, or runtime behavior.

## Determinism

- Capabilities, data, functions, and exports are serialized in ordinal name order.
- Source file paths and timestamps are not stored in Seed modules.
- Integers have defined widths and little-endian representation.
- Text is encoded as strict UTF-8.
- The conformance suite compares complete output bytes and fixed SHA-256 identities.

The current bytecode 1.11 golden modules are:

- `Sumˉdata`: `76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df`
- `Helloˉwindvale`: `0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f`
- `Readˉwvbˉheader`: `c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793`
- `Compositionˉdemo`: `2a3acaf08c23075ee2a9701ba1b35dfe2cb83fca27eb669102a9d0dbfff53419`
- `Foundationˉmachineˉcontracts`: `f624739461dea01862121daf234b3a838dfcafd73753e3124a038b7efa8b4fa3`
- `Machineˉcontractsˉdemo`: `69106233197b3dbc33f23184eaa443505e8595aa056e9e2e10659a33eeefeea3`
- `Foundationˉbyteˉordering`: `27a3c24b5cc358a4f67e2e1959b5e80559918f0176c52e08648e638212e6dece`
- `Foundationˉbyteˉorderingˉdemo`: `fbaf423b6e4eac5c18b644dc27f1fa20fca8798519596485cd7497b44979533f`
- `Foundationˉdecimalˉparsing`: `bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37`
- `Foundationˉdecimalˉparsingˉdemo`: `d323f8fa9178583990394a37872a8ee522320084ef4741eac26cb0f86c21b453`
- `Foundationˉbyteˉconstruction`: `3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8`
- `Foundationˉbyteˉconstructionˉdemo`: `ab594976ced7a84573ade0aa50fb4370d96b8004c8b9a5ec1e888968c7b3bf8f`
- `Compilerˉsourceˉlexer`: `411c7d9679fc53a600c15d2d132b4ac62aa410e45a67f63f76e08efb89da6b3e`
- `Compilerˉsourceˉlexerˉdemo`: `f83ff53dd2ffa1808bbf5c9ca2056f8dbb386308d52142f720ddf26420a6c2db`
- `Compilerˉsourceˉdeclarationˉparser`: `8a0bafe3b0faebfd20e882be59a37af659158fb674cf58aba5adf2284050c6eb`
- `Compilerˉsourceˉdeclarationˉparserˉdemo`: `9e7ff36a3aa8b0a1cf5b4698ef6ab14f8be40f59fd4dffc4ab327813028e8fbf`
- `Compilerˉsourceˉdeclarationˉparserˉtool`: `ad07772ae002683c58899e09e4a323b594ca4957b9f526fca5dc6f4340fd85f0`
- `Compilerˉsourceˉbodyˉparser`: `68a340644274f220224a0c2c08058c78c82bcb0d3edff71402cfce5071121589`
- `Compilerˉsourceˉbodyˉparserˉdemo`: `2a4e44f3c652e9c91ed2dd5c6b3eb1f30f580d937953dd99b26b0eba535a738f`
- `Compilerˉsourceˉbodyˉparserˉtool`: `0a69617d83408b8cf0c99b0efa0e83b24357f36f1de72729c5c513736607ec4f`
- `Compilerˉsourceˉset`: `1121320e20d83f685c559ea2d0cff8b8e57583d047a3c6aaf9f5c1fdc9423acb`
- `Compilerˉsourceˉsetˉdemo`: `ac7fb0e04cf042ab9f9f3bfc8f344f0fdbcdc4198189b65f152eaead84b07742`
- `Compilerˉsourceˉsetˉtool`: `6e8b8c8aaa6fe2c5735719a9b317e8897cf70f87828ea1be5d26d670bc2ed30f`
- `Compilerˉsourceˉgraph`: `9c1ae01b93b9a598fd6b726071dad9a8b4c6fe47d9c8e2d060eff9451724c85b`
- `Compilerˉsourceˉgraphˉdemo`: `a762e564411e9fe72b906c3c37521c9047bb40b1267d2fb46223f382f1c7966c`
- `Compilerˉsourceˉgraphˉtool`: `0a23a10c6abb9eb82229300ab92324f3298fcbf26d3be0948dbc984274a9ac10`
- `Compilerˉsourceˉsymbols`: `a7df71802871d48561c8045d7e997266365d74f7e5158d531164ae636d57a5e7`
- `Compilerˉsourceˉsymbolsˉdemo`: `4cf84322af1cd514bc7ac9ac5e752ef689bb1729e83ea9021b9660c823243457`
- `Compilerˉsourceˉsymbolsˉtool`: `58732a7cb3352f1f61ba4cecb65ae0280aecc975ca06eca359a2881e14477a66`
- `Compilerˉsourceˉbindings`: `a772a75fe625f47e165ca190e76d8cd59fa0b591a0270a5817e02e0fac62542c`
- `Compilerˉsourceˉbindingsˉdemo`: `563caeb4a76fb34d6c2b2b8340260cc1da518c4cbaad9e5f355201f6bd1fa933`
- `Compilerˉsourceˉbindingsˉtool`: `17e877b3c59d2f9a99d26be4c478f10ce8879e6bce925b65894d158fd4a6e0a9`
- `Compilerˉsourceˉwir`: `c4c3bd9164ccdf75acd1140e74c256295bb1f8ea8bdbf69cdcd3225ceea70fbb`
- `Compilerˉsourceˉwirˉdemo`: `7f533fcb38a9311ba4d390b814ea3741ab25d5db9ac2167bd9f4f6b58bddc02f`
- `Compilerˉsourceˉwirˉtool`: `7fbfc8f57620dd81a5d2024310a21a8ce32d56cc986d94b39ca03428c1404db5`
- `Compilerˉsourceˉwvb`: `c4602b6c026a65e0b9de11c025768b7f652ee73640b6f5ff1806d40ee5d0071b`
- `Compilerˉsourceˉwvbˉdemo`: `ef5a7cad94cce135dd937756980f9268fa2964f49dbb4fccca95ba4d09713fc9`
- `Compilerˉsourceˉwvbˉtool`: `18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754`
- `Wvˉdumpˉcore`: `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753`
- `Wvoˉobjectˉcore`: `a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db`
- `Wvaˉassemblerˉcore`: `1589f2750fa8fcf98ed1058814907f7e03eed0ac368467999118e25fb8195a7f`
- `Wvˉlinkerˉcore`: `02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874`

The canonical WVO 1.0 representative object is 189 bytes with SHA-256 `006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`.

The canonical WVA 1 `Hello-Object.wva` output is SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`.

The canonical Windvale Linking 1 two-object image is 24 bytes with SHA-256 `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a`; its canonical map SHA-256 is `31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4`.

Changes to those hashes require a reviewed bytecode/compiler-contract change rather than an automatic fixture refresh.

## Safety boundaries

- Individual and aggregate source-module input, assembly, module, and object sizes are bounded before sustained processing.
- Binary lengths and offsets use checked conversions and remaining-buffer checks.
- The reader rejects malformed UTF-8, unsupported flags, version mismatches, missing bytes, and trailing bytes.
- The verifier rejects unknown opcodes, truncated operands, bad indices, invalid data uses, stack underflow, type mismatches, invalid branches, inconsistent merges, unreachable instructions, and invalid maximum-stack declarations.
- The assembler rejects malformed structure, noncanonical declarations, mismatched definitions, invalid section contexts, unknown references, numeric-width violations, and objects that fail independent WVO verification.
- The linker rejects malformed objects, aggregate-limit violations, duplicate exports, unresolved or kind-mismatched imports, invalid entry selection, image/address overflow, relocation overflow, and candidates that fail independent whole-image reconstruction.
- Hosted capabilities must be declared in the module, separately authorized, supported by the selected adapter, and validated again on return.
- Hosted arguments and file-byte reads/writes have strict count, UTF-8, snapshot, and allocation bounds; normal and diagnostic output remain separate.
- Runtime signed or unsigned overflow, array bounds, byte-range bounds, strict UTF-8 decoding, bounded text construction, instruction limits, call-depth limits, and hosted resource failures use stable runtime codes.

## Deliberate Seed limits

The core Seed language/runtime contract does not include optimization, a general heap contract, garbage collection, mutable arrays or record fields, nested records, flags enums, general text builders, floating point, catchable exceptions, threads, async work, raw pointers, foreign calls, file enumeration or mutation beyond bounded whole-file replacement, runtime module linkage, package discovery, or a general host executable-container writer. Later repository layers provide a bounded shared native backend, narrow capability-free Windows and Linux console containers, and an experimental OS probe, but those do not silently broaden Seed semantics. The qualified Windvale and Stage 0 linkers remain deliberately limited to verified WVO inputs and one raw flat-memory-image target; UEFI, Windows, and Linux packaging are separate target adapters. Decision 0302 supplies a digest-bound native flat-linker candidate, but the Stage 0 command remains the ordinary recovery route until grouped Windows/Linux qualification promotes the exact candidate commit.

These are scope boundaries, not assertions that the current language model is final.
