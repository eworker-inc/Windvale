# Windvale changelog

Windvale has not issued a stable release. Development history and qualification evidence are recorded in Git, `Documents/Decisions/`, and `Documents/Project/Seed-Verification-Evidence.md`.

Once releases begin, Windvale will use `v0.y.z` tags while public contracts remain experimental. A `0.y` change may revise an experimental contract without backward compatibility, but release notes must identify affected formats and migration expectations. A future `1.0.0` release requires an explicit stability and support decision.

## Unreleased

### Added

- Deterministic WVA signed-immediate ALU/test, signed multiply, bounded rotate/shift, and typed SIB base/index/scale/`disp32` memory operations in both the Windvale assembler and independent C# oracle.
- An expanded WVA x86-64 foundation with typed 32/64-bit general-purpose registers, definition-local labels and deterministic near fixups, register move/ALU/compare/test operations, conditional branches, stack and indirect-control operations, and RIP-relative symbol addressing in both the Windvale assembler and independent C# oracle.
- The [Windvale Community Source License 1.0](LICENSE), [E-Worker Inc](https://eworker.ca) stewardship, contributor agreement, and third-party notice policy.
- Vendor-neutral AI-authorship and attribution policy.
- Public contribution, governance, security, support, conduct, and project-identity policies.
- The public [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale) repository, issue and pull-request templates, private vulnerability reporting, and public development workflow.
- Change-aware lightweight, website, and qualification CI scopes, with a targeted site/editor/Wasm/support gate that avoids unrelated Seed qualification for website-only changes.
- Windows and Linux repository verification workflows.
- Repository-owned Windvale syntax highlighting and Visual Studio Code language support for `.wv` source files.
- A GitHub-rendered visual progress dashboard with evidence-based roadmap indicators and a dated SVG phase map.
- Opt-in deterministic per-function instruction reporting in the C# reference runtime and Seed CLI.
- The accepted first x86-64 UEFI/QEMU boot environment and a path-independent dependency preflight over exact EDK II firmware inputs.
- The first verified WVA-to-Windvale kernel execution seam, linking an assembled machine shim to compiler-generated `.wv` Main on kernel-owned memory.
- Windvale-owned post-memory boot evidence and a bidirectional WVA seam for Main entry and console-byte output.
- The accepted Windvale-native interpreter/JIT/AOT direction and explicit qualification gate for retiring .NET from the normal Windows and Linux workflow.
- Qualified exact Windvale bytecode compiler self-reproduction: Stage 0 builds Stage 1, and Stage 1 builds a byte-identical Stage 2 from the same committed 12-module inventory on Windows and Debian.
- The cross-host-qualified Windvale Project 1 manifest, `windvale build` command, portable Windvale-written parser, and native hosted shell for selecting and inspecting one root plus explicit source dependencies without changing import, WVSS, or WVB semantics.
- The cross-host-qualified shared native path through ABI 14/context 6, including all eleven native service leaves, bounded borrowed and arena-backed values, deterministic interpreter/JIT/WVO-AOT agreement, and live Windvale-produced process-input leaves.
- A cross-host-qualified Windvale publication planner that owns every live executable-image extent and canonical runtime-service placement before the narrow Windows/Linux W^X adapter allocates memory.
- A cross-host-qualified Windvale publication-lifetime graph that gates allocate, copy, seal, invoke, and release operations while one internal C# owner contains raw Windows/Linux executable-memory authority.
- A cross-host-qualified probe-17 terminal invalid-opcode boundary with a kernel-owned vector-6 IDT, deterministic panic transcript, and separate pinned-QEMU success and fault images.
- The accepted capability-oriented Windvale OS architecture: a small kernel written primarily in `.wv`, a bounded `.wva` machine layer, isolated services, AOT system code, and no permanent C#/.NET dependency.
- A cross-host-qualified WVA-owned Q35 poweroff adapter, with exact assembler parity and clean pinned-QEMU exit after the successful kernel path.
- Cross-host-qualified WVA-owned normalized x86-64 entries for invalid opcode and general protection, one explicit ring-0 trap-frame prefix, and three deterministic pinned-QEMU boot scenarios.
- Cross-host-qualified kernel-owned x86-64 page tables with a low-1-GiB identity map, null-page guard, NX/WP enforcement, fixed read-only/executable boot window, WVA-owned CR3 mechanics, and continued success/fault evidence under the new root.
- Cross-host-qualified fixed in-guest WVB admission: an AOT Windvale verifier checks one exact embedded WVB 1.6 identity before the bridge can execute its separately AOT-compiled form, while malformed and changed inputs fail closed.
- The first protected Windvale OS process candidate: a separate CPL3 root/thread executes the admitted Windvale AOT module, uses a generation/rights-checked capability for bounded register IPC and syscall exit, contains one deliberate user fault, and preserves terminal kernel-fault behavior.
- The first Windvale OS init/resource-service candidate: a receive-only Windvale service and send-only admitted client execute under separate CPL3 roots, exchange one kernel-owned message through deterministic wait/wake coordination, and preserve service completion after a contained client fault.
- The first user-space Windvale OS bytecode interpreter candidate: an AOT-built portable Windvale interpreter executes the exact admitted WVB subset at CPL3, records its runtime/input identities and role-specific W^X bounds, and sends the interpreted result to the independent init service without linking the program's AOT derivative.
- A section-derived Windvale OS interpreter profile that validates the WVB envelope and seven ordered sections, derives code and metadata payloads, and executes a second compiler-produced module after its longer name moves the code section; the enlarged process, paging, and aligned-memory contracts retain explicit bounds and pinned-QEMU evidence.
- Cross-host-qualified runtime-supplied Windvale OS WVB: the hosted CPL3 interpreter fetches fixed `boot:main.wvb` through an exact WVA-owned ABI-16 leaf, while the admitted program occupies a separate RO/NX page and is absent from the interpreter WVB and linked RX image.
- The first cross-host-qualified Windvale-selected OS boot-resource grant: init owns the fixed admitted WVB, selects resource `1` in `.wv`, and requests one checked immutable RO/NX client alias before the unchanged user-space interpreter can read it.
- The first qualified Windvale OS terminal resource-borrow cleanup: ordinary client exit and contained client fault both remove the WVB alias and private service/resource publication while preserving init ownership and one historical grant.
- The first qualified typed Windvale OS resource pair: init atomically grants the admitted WVB plus a separate execution budget, WVA validates exact typed lookup, the Windvale interpreter charges each opcode, and terminal cleanup removes both aliases and their complete publication.
- ABI 17's bounded 2,048-cell native frame envelope, admitting the exact compiler past its former 1,049-local blocker and reporting the next lowered-value limit with the exact function and required slot.
- Cross-host-qualified ABI 18 typed block-scoped physical values, clearing the exact compiler's slot-2,049 blocker without raising the 2,048-cell limit and reducing Probe 32's retained interpreter WVO by 27.5% and deepest native stack by 59.5%.
- Cross-host-qualified ABI 19 one-byte construction with exact `u8` boundary behavior, independently verified arena/descriptor/store shape, scalar/result alias rejection, and exact compiler preflight advanced to `Bytesˉfromˉu16ˉlittle`; all four pinned-QEMU Probe-32 identities remain exact.
- Probe 30's first reclaimed process extent: exact LIFO release zeroes a terminal client's 42 pages, rebuilds generation 2 at the same physical root, generation-stamped resource records reject stale state, and both clients execute the same typed WVB/budget input.
- Qualified Probe 31's exact-WVB portability proof: the canonical 493-byte `Sum-Data.wv` WVB runs to `29` through Windows/Linux reference and native paths plus a protected Windvale OS interpreter, exercising immutable data, a loop, branches, locals, and internal calls in both rebuilt client generations.
- Cross-host OS-test execution in the regular non-Fast verifier, qualifying probe 24 with all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 while keeping the long golden/native CLI gates out of the focused inner loop.
- A permanent UTF-8 native-process verifier boundary and detached/redirected macron regression check, closing the recurring launcher-only decoding failure.
- Cross-host-qualified ABI 15/context 7 native whole-file output through exact Windows and Linux leaves, advancing the exact compiler WVB beyond its `file.write_bytes` admission blocker.
- Cross-host-qualified ABI 16 bounded internal calls: four fast register parameters plus verified 16-byte stack cells through the language's 64-parameter limit, advancing exact compiler preflight to its single 1,049-local function.
- Cross-host-qualified bounded exact-compiler publication: deterministic size attribution justifies an 8 MiB fragment ceiling under unchanged ABI 20, the 4,556,121-byte compiler passes independent decoding and W^X publication, and native execution reaches the retained 1 MiB record-arena boundary before output.
- Cross-host-qualified bounded exact-compiler execution: measured demand of 1,480,096 record bytes justifies a 2 MiB host arena, under which the native compiler emits the exact 815-byte Stage 0 WVB while retaining ABI 20, generated bytes, execution-scoped lifetime, and independently sized Windvale OS contexts.
- Cross-host-qualified nominal record and enum identities throughout native machine IR plus a deterministic record-storage planner; exact compiler liveness projects a 1,489-cell worst frame under the unchanged ABI-20 2,048-cell ceiling without yet changing record allocation or claiming full native reproduction.
- Cross-host-qualified deterministic absolute native record-storage maps for persistent locals and block-local results, independently checked against CFG and value lifetimes and pinned for the exact compiler without changing ABI 20 or selected machine bytes.
- The first narrow Windows host-executable target: `windvale compile --target windows-x64-console-v1` packages capability-free scalar ABI-20 programs as deterministic import-free PE32+ files with an independently verified startup/context boundary and direct Windows execution without loading .NET.
- The matching narrow Linux host-executable target: `windvale compile --target linux-x64-console-v1` packages the same verified scalar fragment as a deterministic sectionless static-PIE ELF, owns its stack through a bounded Linux `mmap`, and exits through direct syscalls without an interpreter, libc, or .NET.
- Paired portable process-result normalization and atomic executable publication: both native containers preserve only successful results `0` through `255`, map every other result or native failure to `1`, and publish a fully written and prepared sibling file through one replacement so failure leaves the requested output unchanged.
- The public [`windvale.ca`](https://windvale.ca/) project home, system-responsive light and dark themes, new Windvale visual identity, and same-origin [`windvale.ca/playground/`](https://windvale.ca/playground/) browser playground.
- Windvale-authored WebAssembly backend profiles: a portable `.wv` selector emits deterministic import-free modules from bounded scalar WVB, while execution ABI 1 preserves checked arithmetic and ABI 2 adds exact instruction limits for loops, sequential `if`/`if/else` regions, bounded acyclic direct calls, and their composition. Exact success, `WVR3007`, or `WVR3011` status, result validity, and instruction accounting are preserved across control regions and callees without an engine trap.
- Playground integration for the bounded direct-Wasm subset: a digest-pinned `.wv` backend lowers canonical WVB, a disposable browser worker validates and executes the import-free module, and the UI compares result and instruction evidence with the .NET reference interpreter.
- A standalone `/playground/wasm-demo/` route that checks and executes the pinned 2,729-byte profile-7 three-function loop-and-conditional Windvale-generated artifact without loading Blazor or .NET, while keeping Stage 0 artifact-production provenance explicit.
- A separate Windvale support page with configurable one-time Stripe tiers, explicit public-recognition consent, and a validated Cloudflare KV-backed supporter roll that keeps personal data out of repository history.

### Changed

- The Windvale-written lexer uses bounded keyword, identifier, and Unicode-whitespace dispatch while preserving the Seed lexical contract.
- Compiler folders now describe implementation roles: `Compiler/Windvale` contains the Windvale-written compiler, while `Compiler/Reference` contains the independent C# reference/recovery compiler.
- Assembler folders now describe implementation roles: `Assembler/Windvale` contains the Windvale-written assembler, while `Assembler/Reference` contains the independent C# Stage 0 reference/recovery assembler. Canonical WVA inputs remain under `Examples/Assembler`.
- Linker folders now describe implementation roles: `Linker/Windvale` contains the Windvale-written flat-image linker, while `Linker/Reference` contains the independent C# Stage 0 reference/recovery linker and its currently C#-only UEFI target adapter. Canonical WVA provider inputs remain under `Examples/Linker`.

### Current development status

- Windvale Seed, its runtime and bytecode foundation, the object model, assembler, linker, Foundation modules, complete portable compiler, and exact bytecode compiler self-reproduction have Windows/Debian qualification evidence.
- Native compiler execution, Windvale-native platform memory authority, complete execution-support lifetime ownership, the shared JIT/AOT backend, .NET retirement, and a general Windvale OS runtime remain active or planned milestones rather than completed releases; qualified Probe 31 proves one exact nontrivial WVB across Windows, Linux, and Windvale OS but does not provide a general allocator, arbitrary loader, dynamic resource namespace, transferable capability system, scheduler, general interpreter/JIT, or complete kernel runtime.
