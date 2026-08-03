# Windvale Windows console application target

## Status and purpose

`windows-x64-console-v1` is the first deterministic Windows host-executable target. It packages one already verified ABI-21 x86-64 native fragment as an import-free PE32+ console application. The first implementation is Stage 0 hosted: the C# compiler, native backend, WVO writer, flat linker, and PE adapter materialize the file, while digest-pinned portable Windvale modules supply every live layout value, every final byte through a sparse construction recipe, and completed-container verification with recovered-native evidence. The resulting application executes without loading .NET.

This is a narrow executable-boundary proof. It is not a general Windows runtime, hosted-capability container, native compiler executable, or .NET-retirement milestone.

## Input boundary

The adapter accepts one complete native fragment only after the independent native fragment verifier succeeds. Version 1 additionally requires:

- target `x86-64-wvb-baseline-v21` and native ABI 21;
- exactly one exported, non-empty `Main() -> i32` entry;
- no required runtime services, and therefore no hosted capabilities;
- WVO production within the existing 4 MiB object bound;
- a successful base-zero `flat-x86-64-v1` link containing only code and read-only data;
- relative-i32 relocations only; and
- linked bytes and the entry offset exactly reproducing the verified native fragment.

Portable operations that use generated record or dynamic byte storage remain admitted within the fixed arenas below. Operations requiring the runtime service table are rejected even when they are capability-free.

## Process entry and result

The PE entry is an exact 98-byte Windows x64 stub followed by zero padding to byte 112 and then the unchanged linked native image. The stub is expressed by `Linker/Startup/Windows-X64-Console.wva`; its assembled WVO code and four typed relative relocations must reproduce the independently encoded C# recovery writer exactly. The stub:

1. reserves the required Windows x64 shadow/alignment space;
2. obtains the writable execution context through RIP-relative addressing;
3. publishes RIP-relative record- and text-arena bases into that context;
4. supplies the context in `RDX` and its retained Windows/System-V bridge duplicate in `R8`;
5. calls the native fragment's exported `Main` through one relative displacement;
6. returns successful `i32` results from `0` through `255` unchanged; and
7. maps every other successful result and every packed nonzero native status to process result `1`.

The fixed execution limits are ABI 21's defaults: 1,000,000 charged instructions and call depth 1,024. Windows terminates the process when its primary entry thread returns. Restricting the portable process-result range to `0` through `255` makes direct process observation identical to Linux; the underlying Windvale `Main() -> i32` semantics remain unchanged. Version 1 emits no diagnostic text for a native trap.

## Memory contract

The executable has no PE imports and uses no heap or OS allocation API. Its writable `.data` mapping contains:

| Region | Virtual bytes | Initial rule |
| --- | ---: | --- |
| ABI-21 execution context | 112 | Exact version, size, budgets, arena lengths, and otherwise zero |
| Record arena | 2,097,152 | Retained loader-zeroed compatibility extent; context base is installed by the entry stub |
| Dynamic text/byte arena | 16,777,216 | Zero-filled; context base is installed by the entry stub |

The first 512 `.data` bytes are present in the file; the remaining virtual extent is loader-zeroed. Service, argument, output, file-input, and file-output pointers remain zero. Dynamic text and byte allocations retain checked cursor and bounds behavior. ABI-21 generated records use verified frame-owned backing, do not read or advance the retained record arena, and leave its cursor at zero.

The PE reserves 64 MiB of stack with a 64 KiB initial commit, covering the retained 1,024-call budget at the current 32 KiB maximum generated frame plus bounded outgoing cells. It declares a 1 MiB heap reserve but version 1 does not use it.

## Canonical PE32+ layout

All integers are little-endian. Unlisted and padding bytes are zero. File alignment is 512 bytes and section alignment is 4 KiB.

Before allocating the file, the adapter evaluates the versioned [Windvale console-application plan](Windvale-Console-Application-Plan.md) over the native-image size and entry offset. It independently recomputes and checks every returned field. The portable [construction recipe](Windvale-Console-Application-Construction.md) then supplies the exact headers, startup, native-copy span, context, relocation data, and implicit zero gaps. Stage 0 validates and materializes that recipe and compares every completed byte with its recovery writer.

| Region | Contract |
| --- | --- |
| DOS/PE headers | `MZ`, `e_lfanew = 0x80`, x86-64 COFF, timestamp and symbol metadata zero |
| Optional header | PE32+, image base `0x140000000`, console subsystem 3, OS/subsystem version 6.0 |
| `.text` | Read/execute; entry stub, alignment padding, and exact native linked image |
| `.data` | Read/write; fixed context plus loader-zeroed arenas |
| `.reloc` | Read/discardable; one 12-byte block containing only absolute padding entries |

The image is dynamic-base, high-entropy-VA, and NX-compatible. All executable references are relative, so relocation changes preserve them without an absolute fixup. There is no import, export, resource, exception, TLS, debug, or certificate directory.

The complete file is bounded to 4,196,352 bytes. Canonical `Sum-Data.wv` produces a 5,120-byte PE with SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`. The virtual image additionally contains the fixed arenas and is independently checked with overflow-safe arithmetic.

## Independent verification

The portable [console-application verifier](Windvale-Console-Application-Verification.md) first treats the PE as segmented untrusted bytes, regenerates its canonical recipe, checks every container-owned byte and zero gap, and returns the recovered native bytes and entry through fixed evidence. `Windowsˉconsoleˉapplicationˉverifier.Verify` independently parses the same untrusted PE. It checks the outer size before fixed reads; every DOS, COFF, optional-header, directory, section, permission, raw, virtual, and padding field; the exact startup instruction shapes and all four relative targets; the initial execution context; and the relocation block.

The writer requires both verifiers to reproduce the verified flat link before publication. Differential tests independently compile and evaluate the Windvale layout, construction, and verification modules; compare their complete serialized evidence with the C# layout and byte oracles; assemble the WVA startup; require its exact symbol and relocation contract; instantiate the four final-image displacements; and compare all 98 startup bytes with the PE. The existing malformed PE corpus drives both completed-container verifiers. The native fragment verifier remains responsible for generated machine-code semantics before packaging.

## Diagnostics

Writer failures return no application bytes:

| Code | Meaning |
| --- | --- |
| `WVW1001` | Null, malformed, unknown-target, or otherwise unverified native fragment. |
| `WVW1002` | Descriptor entry, required runtime service, capability, or missing scalar `Main`. |
| `WVW1003` | WVO production, object limit, link failure, or failure to reproduce the native fragment. |
| `WVW1004` | Independent PE verification or recovered-input comparison failure. |

The untrusted-byte verifier throws a bounded format exception:

| Code | Meaning |
| --- | --- |
| `WVW2001` | File size, derived extent, or trailing-byte failure. |
| `WVW2002` | DOS header or stub failure. |
| `WVW2003` | PE signature or COFF header failure. |
| `WVW2004` | PE32+ optional-header or directory failure. |
| `WVW2005` | Section layout, size, or permission failure. |
| `WVW2006` | Base-relocation block failure. |
| `WVW2007` | Nonzero canonical padding. |
| `WVW2008` | Startup instruction, address, entry call, or exit mapping failure. |
| `WVW2009` | Initial execution-context or arena-bound failure. |

## Deliberate limits

Version 1 has no console output despite selecting the Windows console subsystem, arguments, file access, diagnostic channel, runtime-service table, PE imports, separately protected read-only data, persistent heap, thread contract, unwind metadata, debugger metadata, code signing, embedded application WVB, load-time application-WVB verification, or install-time cache identity. Windvale now owns the exact WVA startup, portable layout, and sparse byte construction, but the Stage 0 compiler, recipe materializer/recovery writer, and untrusted-container verifier are still required to construct and qualify the executable. Hosted applications and standalone native tools require later target versions rather than implicit access to ambient Windows state.
