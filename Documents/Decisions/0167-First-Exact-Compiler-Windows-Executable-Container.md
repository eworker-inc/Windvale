# Decision 0167: First exact-compiler Windows executable container

- Date: 2026-08-03
- Status: Implemented initial Windows candidate; the current paired format-3 containers and direct Stage 2 gate are cross-host qualified by Decision 0168
- Adds: canonical WVA startup and deterministic independently verified format-3 Windows PE32+ compiler candidate
- Retains: ABI 22, the exact compiler and service bytes, the bounded runtime-data plan, ordinary 4 MiB behavior, and all existing application bytes

## Context

Decisions 0161, 0163, and 0164 fix the exact compiler's service image, bounded runtime state, and Linux container. Windows needs the same independently inspectable boundary without making PE, Win32, C#, or .NET part of Windvale semantics. The executable must preserve the already measured compiler and service bytes, bind only the declared host adapters, keep writable runtime state non-executable, and fail closed before native execution when any retained contract is inconsistent.

Live loader evidence exposed two bootstrap details that a structural writer alone could not establish. `WideCharToMultiByte` has four register arguments and four stack arguments under the Windows x64 ABI; every stack cell, including the two null default-character pointers, must be initialized explicitly. The fixed name and data arenas are also already mapped as zero-initialized pages of the PE image. Calling `VirtualAlloc(MEM_COMMIT)` over that image mapping fails even though the exact native file-input leaf legitimately asks its adapter to commit each slot before first use.

## Decision

- Define `Windows-X64-Hosted-Compiler.wva` as the canonical x86-64 startup source. Its 1,510-byte code section contains the exported startup and a local checked mapped-page adapter, 38 imports, and 58 typed relative-i32 relocations. The retained instantiated-local/unpatched-external template is SHA-256 `59a3f3b794c5b81bde8385aab77d86fae01bfc0c728bc5f412459cff5eb7310a`; the encoded WVO is SHA-256 `55f4782e976038c2d68bb91aeabb75518103524e9d5caaf1cc9f0662ab5a0feb`.
- Import exactly twelve `KERNEL32.dll` functions and `CommandLineToArgvW` from `SHELL32.dll`. Do not import `VirtualAlloc`, a C runtime, the CLR, or any unrelated facility.
- Exclude `argv[0]`, admit at most 67 arguments, convert at most 4,096 UTF-16 code units per argument into at most 4,096 non-NUL UTF-8 bytes, admit at most 65,536 aggregate bytes, and call the exact UTF-8 service before publishing immutable descriptors. Initialize all eight `WideCharToMultiByte` arguments explicitly.
- Bind the exact file-input commit slot to the local mapped-page adapter. It accepts only `MEM_COMMIT` plus `PAGE_READWRITE`, a positive name extent no larger than the 1 MiB name stride or the exact 4 MiB data extent, a 4 KiB-aligned address, checked addition without wrap, and an endpoint wholly inside the corresponding fixed arena. It returns the already mapped address; every other request returns null.
- Emit one PE32+ console image with deterministic zero timestamp/checksum, 4 KiB section alignment, 512-byte file alignment, and three sections: RX `.text`, RW/NX `.data`, and read-only discardable `.reloc`. The first data page owns imports, the second owns the exact 4,096-byte runtime header, and the remaining 409,022,464 bytes are loader-provided demand-paged zero state.
- Keep construction and verification separate. The verifier parses the DOS, COFF, optional, directory, section, import, relocation, padding, bundle, startup, and runtime contracts; checks exact file boundaries and reserved zeros; and rejects truncation, trailing bytes, and mutations across every owned region. A separately assembled WVA object must reproduce the instantiated startup, including its local relocation, exactly.
- Extend the existing exact-compiler AOT case rather than compiling the 17 MiB native compiler again. Build each platform bundle and container twice from the one compiled fragment, compare exact bytes, apply the shared malformed corpus, and on Windows launch the raw PE against the canonical function-only source and require the exact expected WVB.

## Local evidence

The focused Release test passes with zero warnings on Windows. Two independent constructions produce identical 17,157,120-byte PE files with SHA-256 `8864dd8638a947bd10a13803355783b5f3ead6482889803ef4e2d86a425d2c46`.

| Region | File placement/bytes | Virtual placement/bytes |
| --- | ---: | ---: |
| Headers | 0 / 512 | 0 / 4,096 |
| `.text` | 512 / 17,147,904 | 4,096 / 17,147,731 |
| `.data` | 17,148,416 / 8,192 | 17,154,048 / 409,030,656 |
| `.reloc` | 17,156,608 / 512 | 426,184,704 / 12 |
| Complete image | 17,157,120 bytes | 426,188,800 bytes |

The Windows loader ran the raw candidate, startup converted and published both arguments, the exact native compiler read the source through the bounded snapshot service, durably wrote the output, emitted the canonical status line, returned zero, and produced the byte-identical expected 815-byte WVB. The child image contains no CLR directory or CLR imports; .NET participates only in the retained Stage 0 parent construction and verification process.

## Cross-host evidence

Exact descendant `db20fefaa3333b7b78392ba12141d1ae2b6bb0c2` passes GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 87 Seed tests including the golden compiler contract, all 38 OS tests, and the native CLI gate. The paired containers retain their pinned identities, and each directly reproduces the exact 599,868-byte compiler WVB from the canonical twelve-source inventory without a named .NET runtime in the child.

## Consequences

At this decision boundary, both permanent hosts had deterministic format-3 executable candidates derived from the same ABI-22 compiler and verified service bundle. Windows had direct loader and small-source execution evidence while Linux had construction/parser evidence only. Decision 0168 subsequently advances the Windows stack contract and cross-host qualifies canonical Stage 2 reproduction on both hosts.

Decision 0168 completes direct paired reproduction and Decision 0169 supplies public atomic publication. The retained C# path remains the explicit recovery oracle until the broader native-retirement gate is complete.

## Reconsider when

- A qualified Windows version rejects the position-independent empty relocation block or maps the RW/NX extent differently.
- A future exact service changes its name/data commit sizes or arena ownership contract.
- Direct Stage 2 reproduction exposes a missing argument, path, publication, instruction-budget, or process-result rule.
- A smaller compatible runtime allocation plan replaces the fixed snapshot strides.
