# Windvale Linux console application target

## Status and purpose

`linux-x64-console-v1` is the first deterministic Linux host-executable target. It packages one already verified ABI-21 x86-64 native fragment as a sectionless, import-free ELF64 static position-independent application. The first implementation is Stage 0 hosted: the C# compiler, native backend, WVO writer, flat linker, and ELF adapter materialize the file, while digest-pinned portable Windvale modules supply every live layout value, every final byte through a sparse construction recipe, and completed-container verification with recovered-native evidence. The resulting application executes without a dynamic loader, libc, or .NET.

This is the Linux twin of `windows-x64-console-v1`. It is a narrow executable-boundary proof, not a general Linux runtime, hosted-capability container, native compiler executable, or .NET-retirement milestone.

## Input boundary

The Windows and Linux adapters share one preparation boundary. It accepts a native fragment only after the independent native fragment verifier succeeds, then requires:

- target `x86-64-wvb-baseline-v21` and native ABI 21;
- exactly one exported, non-empty `Main() -> i32` entry;
- no required runtime services, and therefore no hosted capabilities;
- WVO production within the existing 4 MiB object bound;
- a successful base-zero `flat-x86-64-v1` link containing only code and read-only data;
- relative-i32 relocations only; and
- linked bytes and the entry offset exactly reproducing the verified native fragment.

The format adapters and their untrusted-byte verifiers remain separate. Portable generated record and dynamic byte storage is admitted within the fixed arenas below. Operations requiring the runtime service table are rejected.

## Process entry and result

The ELF entry is an exact 158-byte Linux x86-64 stub followed by zero padding to byte 160 and then the unchanged linked native image. The stub is expressed by `Linker/Startup/Linux-X64-Console.wva`; its assembled WVO code and four typed relative relocations must reproduce the independently encoded C# recovery writer exactly. The stub:

1. invokes Linux x86-64 `mmap` syscall 9 for a private anonymous 64 MiB read/write stack mapping;
2. exits with result `1` if the mapping fails, otherwise switches to its aligned upper boundary;
3. obtains the writable execution context through RIP-relative addressing;
4. publishes RIP-relative record- and text-arena bases into that context;
5. supplies the context through both retained System V bridge positions in `RSI` and `RDX` and clears the other argument registers;
6. calls the native fragment's exported `Main` through one relative displacement;
7. preserves successful results from `0` through `255` and maps every other successful result or packed nonzero native status to result `1`; and
8. terminates through Linux x86-64 `exit` syscall 60, followed by an unreachable WVA `trap` boundary.

The private stack mapping prevents the program from inheriting a smaller ambient shell stack limit. Its 64 MiB size covers the retained 1,024-call budget at the current 32 KiB maximum generated frame plus bounded outgoing cells. The fixed execution limits remain ABI 21's defaults: 1,000,000 charged instructions and call depth 1,024.

Linux wait status exposes only eight process-result bits. The startup check prevents implicit truncation by admitting exactly `0` through `255`, matching the Windows container's portable process-result contract; the underlying Windvale `Main() -> i32` semantics remain unchanged. Version 1 emits no diagnostic text for a native trap.

## Memory contract

The executable uses only its two entry syscalls and has no ELF imports, interpreter, dynamic table, or heap allocation. Its writable load segment contains:

| Region | Virtual bytes | Initial rule |
| --- | ---: | --- |
| ABI-21 execution context | 112 | Exact version, size, budgets, arena lengths, and otherwise zero |
| Record arena | 2,097,152 | Retained loader-zeroed compatibility extent; context base is installed by the entry stub |
| Dynamic text/byte arena | 16,777,216 | Loader-zeroed; context base is installed by the entry stub |

Only the 112-byte context is present in the file. The remaining writable virtual extent is zero-filled by the ELF loader. Service, argument, output, file-input, and file-output pointers remain zero. Dynamic text and byte allocations retain checked cursor and bounds behavior. ABI-21 generated records use verified frame-owned backing, do not read or advance the retained record arena, and leave its cursor at zero.

## Canonical ELF64 layout

All integers are little-endian. Unlisted and padding bytes are zero. File and load alignment is 4 KiB.

Before allocating the file, the adapter evaluates the versioned [Windvale console-application plan](Windvale-Console-Application-Plan.md) over the native-image size and entry offset. It independently recomputes and checks every returned field. The portable [construction recipe](Windvale-Console-Application-Construction.md) then supplies the exact header page, startup, native-copy span, context, and implicit zero gaps. Stage 0 validates and materializes that recipe and compares every completed byte with its recovery writer.

| Region | Contract |
| --- | --- |
| ELF header | ELF64, little-endian, System V, x86-64, `ET_DYN`, entry `0x1000`, no section table |
| Header `PT_LOAD` | Read-only page containing the ELF/program headers and version note |
| Code `PT_LOAD` | Read/execute; exact startup, alignment padding, and native linked image |
| Data `PT_LOAD` | Read/write; fixed context plus loader-zeroed arenas |
| `PT_NOTE` | Read-only `Windvale` owner, note type 1, format version 1 |
| `PT_GNU_STACK` | Read/write and non-executable; records the owned 64 MiB requirement |

There is no `PT_INTERP`, `PT_DYNAMIC`, writable/executable load, section table, symbol table, relocation table, debug data, build ID, or runtime dependency. Equal file offsets and virtual addresses plus RIP-relative executable references allow the kernel to choose one position-independent load bias without fixups.

The complete file is bounded to 4,202,608 bytes. Canonical `Sum-Data.wv` produces an 8,304-byte ELF with SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.

## Independent verification

The portable [console-application verifier](Windvale-Console-Application-Verification.md) first treats the ELF as segmented untrusted bytes, regenerates its canonical recipe, checks every container-owned byte and zero gap, and returns the recovered native bytes and entry through fixed evidence. `Linuxˉconsoleˉapplicationˉverifier.Verify` independently parses the same untrusted ELF. It checks the outer size before fixed reads; the complete ELF identification and header; all five exact program headers; load sizes, permissions, address/offset agreement, and derived extents; the version note; every padding region; the exact startup instruction shapes and four relative targets; the mmap and exit syscall boundaries; and the initial execution context.

The writer requires both verifiers to reproduce the verified flat link before publication. Differential tests independently compile and evaluate the Windvale layout, construction, and verification modules; compare their complete serialized evidence with the C# layout and byte oracles; assemble the WVA startup; require its exact symbol and relocation contract; instantiate the four final-image displacements; and compare all 158 startup bytes with the ELF. The existing malformed ELF corpus drives both completed-container verifiers, and the paired tests require PE and ELF recovery to agree on the same native image and `Main` offset.

## Diagnostics

Writer failures return no application bytes:

| Code | Meaning |
| --- | --- |
| `WVL1001` | Null, malformed, unknown-target, or otherwise unverified native fragment. |
| `WVL1002` | Descriptor entry, required runtime service, capability, or missing scalar `Main`. |
| `WVL1003` | WVO production, object limit, link failure, or failure to reproduce the native fragment. |
| `WVL1004` | Independent ELF verification or recovered-input comparison failure. |

The untrusted-byte verifier throws a bounded format exception:

| Code | Meaning |
| --- | --- |
| `WVL2001` | File size, derived extent, or trailing-byte failure. |
| `WVL2002` | ELF identification or main-header failure. |
| `WVL2003` | Program-header kind, position, or fixed metadata failure. |
| `WVL2004` | Linked-code size or memory-size failure. |
| `WVL2005` | Windvale version-note failure. |
| `WVL2006` | Startup instruction, stack mapping, relative target, entry call, or exit mapping failure. |
| `WVL2007` | Initial execution-context or arena-bound failure. |
| `WVL2008` | Nonzero canonical padding. |

## Publication and deliberate limits

The CLI defaults this target to `.elf`. On Linux it sets mode `0755` after writing the completely verified bytes; cross-construction on Windows cannot carry Unix executable-mode metadata and the transferred file must be made executable by its packaging or installation step.

Version 1 has no console output, arguments, environment access, file access, diagnostic channel, runtime-service table, dynamic linking, libc, persistent heap, threads, unwind metadata, debugger metadata, signing, embedded WVB, load-time WVB verification, or install-time cache identity.

The C# adapter is the Stage 0 oracle and recovery implementation. Windvale now owns the exact WVA startup, portable layout plan, and sparse byte construction with complete oracle agreement. Recipe materialization and the untrusted-container verifier remain named replacement seams for portable Windvale `.wv` cores. Normal verification ownership moves only after the Windvale implementation reproduces exact rejection behavior across Windows and Linux.
